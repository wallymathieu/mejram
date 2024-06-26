module Mejram.Analysis
open Mejram.Models
open System
open System.Text.RegularExpressions
open System.Collections.Generic
open FSharpPlus
[<CLIMutable>]
type TableNameConventions = {
    /// Table prefixes to be ignored when looking for primary keys. These are usually "tbl_" or "tbl".
    TablePrefix:string IReadOnlyCollection
    /// Common key name prefixes. These are usually "_id" or "id".
    KeyNames:string IReadOnlyCollection

    /// Infixes that are used when naming many-to-many tables.
    /// An empty string or "_" are common infixes.
    ManyToManyInfixes:string IReadOnlyCollection
}
with
    /// Default conventions
    static member Default() = {
        TablePrefix = ["tbl_";"tbl";""]
        KeyNames = ["id";"_id"]
        ManyToManyInfixes = ["_"; ""]
    }

[<AutoOpen>]
module TableNameConventions=
    /// Regex to search for strings that start with a table prefix.
    let tablePrefixes (p:TableNameConventions) =
        p.TablePrefix |> map Regex.Escape |> String.concat "|"
        |> fun s-> Regex(sprintf "^(%s)" s, RegexOptions.IgnoreCase)
    let tableNameTrim t p = (tablePrefixes p).Replace(t, "")

    /// Regex to search for strings that end with a column postfix.
    let idColumnPostfixes (p:TableNameConventions) =
        p.KeyNames |> map Regex.Escape |> String.concat "|"
        |> fun s-> Regex(sprintf "(%s)$" s, RegexOptions.IgnoreCase)
    /// Regex to search for strings that are exactly a table prefix and a column postfix.
    /// <br/>
    /// e.g. tbl_id, tblId, Id
    /// <br/>
    /// using the default conventions (where the table prefix can be empty).
    let exactlyPrefixAndPostfix (p:TableNameConventions)= 
        let prefix = p.TablePrefix |> map Regex.Escape |> String.concat "|"
        let postfix = p.KeyNames |> map Regex.Escape |> String.concat "|"
        Regex(sprintf "^(%s)(%s)$" prefix postfix, RegexOptions.IgnoreCase)
    /// Regex to search for strings that start with a table prefix and end with a column postfix.
    /// <br/>
    /// e.g. tbl_customer_id, tblCustomerId, CustomerId
    /// <br/>
    /// using the default conventions (where the table prefix can be empty).
    let prefixAndValueAndPostfix (p:TableNameConventions)= 
        let prefix = p.TablePrefix |> map Regex.Escape |> String.concat "|"
        let postfix = p.KeyNames |> map Regex.Escape |> String.concat "|"
        Regex(sprintf "^(%s)(?<value>\w+)(%s)$" prefix postfix, RegexOptions.IgnoreCase)

module TC = TableNameConventions
[<CompiledName("ProbablePrimaryKeys")>]
let probablePrimaryKeys (tables: Table seq) (p:TableNameConventions) : _ IReadOnlyList =
    let idColumn= TC.exactlyPrefixAndPostfix p
    let idNamedTableColumn= TC.prefixAndValueAndPostfix p
    let isIdColumn (c:Column) = c.ColumnName() |> idColumn.IsMatch
    let findTableIdColumn (column:Column)=
       let t = column.TableName()
       let m = column.ColumnName() |> idNamedTableColumn.Match
       m.Success && m.Groups.["value"].Value.Trim('_').Equals(t, StringComparison.InvariantCultureIgnoreCase)
    tables |> map (fun t->
        let tryFindColumn p = tryFind p t.Columns
        tryFindColumn isIdColumn
        |> Option.orElseWith (fun () -> tryFindColumn findTableIdColumn)
    ) |> choose (Option.map (fun c-> c.ColumnKey))
    |> Seq.toIReadOnlyList
[<CompiledName("ProbableForeignKeys")>]
let probableForeignKeys (tables: Table seq) (p:TableNameConventions) : _ IReadOnlyList =
    let idColumnPostfixes=TC.idColumnPostfixes p
    let tableNameTrim t = TC.tableNameTrim t p
    let tableMatchingColumn (column:Column) t=
        let trimmedTable = tableNameTrim(t.TableName)
        let columnWithoutPostfix = idColumnPostfixes.Replace(column.ColumnName(), "")
        trimmedTable = columnWithoutPostfix && t.TableName <> column.TableName()
    let idColumnPostfixesMatches (c:Column) = c.ColumnName() |> idColumnPostfixes.IsMatch
    let columnsWithIdPostfix=
        tables 
        |> Seq.collect (fun t-> t.Columns |> filter idColumnPostfixesMatches)
    let getPrimaryKey t=
        match t.PrimaryKey with
        | Some p->p.PrimaryKeys |> head
        | None-> failwithf "Could not find primary key for table %s" t.TableName

    columnsWithIdPostfix
    |> Seq.choose (fun c->
        tables
        |> tryFind (tableMatchingColumn c)
        |> Option.map (fun t->{ ForeignKeyName=String.concat "__" [|c.TableName(); t.TableName; c.ColumnName()|]
                                Columns=[ {From=c.ColumnKey; To=getPrimaryKey t} ]} ))
    |> Seq.toIReadOnlyList

/// Search for tables that look like many to many based on their names.
[<CompiledName("ProbableManyToManyTables")>]
let probableManyToManyTables (tables: Table seq) (p:TableNameConventions) : MaybeManyToManyTable IReadOnlyList =
    let tableNameTrim t = TC.tableNameTrim t p
    let tableNames = map (fun t-> StringIgnoreCase.Create t.TableName) tables 
    let tableNameSet = Set.ofSeq tableNames

    let startsWith tableName (possibleMatch:string) =
        let trimmed = tableNameTrim tableName
        if tableNameTrim possibleMatch |> _.StartsWith(trimmed, StringComparison.InvariantCultureIgnoreCase) then
            Some (trimmed, tableName)
        else
            None

    let endsWith tableName (possibleMatch:string)=
        let trimmed = tableNameTrim tableName
        if tableNameTrim possibleMatch |> _.EndsWith(trimmed, StringComparison.InvariantCultureIgnoreCase) then
            Some (trimmed, tableName)
        else
            None

    let looksLikeManyToMany (t:Table) : MaybeManyToManyTable option =
        let tableNamesWithoutSelf = 
            tableNameSet 
            |> Set.remove (StringIgnoreCase.Create t.TableName)
            |> Set.map (fun s-> s.CaseInsensitiveValue)
        monad {
            let! (trimmedFirst, firstTableName) = Seq.tryPick (fun ot -> startsWith ot t.TableName) tableNamesWithoutSelf
            let tableNameWithoutFirstTable = (tableNameTrim t.TableName).Substring(trimmedFirst.Length)
            let! (trimmedSecond,secondTableName) = Seq.tryPick (fun ot -> endsWith ot tableNameWithoutFirstTable) tableNamesWithoutSelf
            let withoutSecond = tableNameWithoutFirstTable.Substring(0, tableNameWithoutFirstTable.Length - trimmedSecond.Length)
            return! p.ManyToManyInfixes 
                    |> tryFind (fun infix-> withoutSecond.Equals(infix, StringComparison.InvariantCultureIgnoreCase))
                    |> Option.map (konst {Table = t; FirstOtherTable= firstTableName; SecondOtherTable= secondTableName}) }
    tables |> Seq.choose looksLikeManyToMany |> Seq.toIReadOnlyList
