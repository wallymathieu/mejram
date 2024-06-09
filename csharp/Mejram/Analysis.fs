module Mejram.Analysis
open Mejram.Models
open System
open System.Text.RegularExpressions
open System.Collections.Generic
open System.Globalization
open FSharpPlus
type PluralizationStrategy=
    /// The strategy of not pluralizing the table name
    | DoNotPluralize
    /// The strategy of adding an 's' to the end of the table name
    | PluralizeWithS
    /// A custom pluralizer. The input is the table name and the output is the pluralized table name.
    | Pluralizer of (string -> string)

type TableNameConventions = {
    /// Table prefixes to be ignored when looking for primary keys. These are usually "tbl_" or "tbl".
    TablePrefix:string IReadOnlyCollection
    /// Common key name prefixes. These are usually "_id" or "id".
    KeyNames:string IReadOnlyCollection

    /// Infixes that are used when naming many-to-many tables.
    /// An empty string or "_" are common infixes.
    ManyToManyInfixes:string IReadOnlyCollection
    /// Strategies to pluralize table names.
    /// The default strategies is either to add an 's' to the end of the table name
    /// or to not pluralize the table name.
    PluralizationStrategies: PluralizationStrategy IReadOnlyCollection
}
with
    static member Default = {
        TablePrefix = ["tbl_";"tbl";""]
        KeyNames = ["id";"_id"]
        ManyToManyInfixes = ["_"; ""]
        PluralizationStrategies = [DoNotPluralize; PluralizeWithS]
    }
[<CustomEquality;CustomComparison>]
type StringIgnoreCase = {CaseInsensitiveValue : string}
    with
        static member Create (s:string) = {CaseInsensitiveValue = s}

        interface IComparable<StringIgnoreCase> with
            member x.CompareTo(y) = StringComparer.InvariantCultureIgnoreCase.Compare(x.CaseInsensitiveValue, y.CaseInsensitiveValue)
        interface IComparable with
            member x.CompareTo(y) =
                match y with
                | :? StringIgnoreCase as y -> StringComparer.InvariantCultureIgnoreCase.Compare(x.CaseInsensitiveValue, y.CaseInsensitiveValue)
                | _ -> failwith "Cannot compare StringIgnoreCase with non-StringIgnoreCase"

        override x.Equals(y) =
            match y with
            | :? StringIgnoreCase as y -> x.CaseInsensitiveValue.Equals(y.CaseInsensitiveValue, StringComparison.InvariantCultureIgnoreCase)
            | _ -> false
        override x.GetHashCode() =  StringComparer.InvariantCultureIgnoreCase.GetHashCode(x.CaseInsensitiveValue)
        
module TableNameConventions=
    /// Regex to search for strings that start with a table prefix.
    let tablePrefixes (p:TableNameConventions) =
        p.TablePrefix |> map Regex.Escape |> String.concat "|"
        |> fun s-> Regex(sprintf "^(%s)" s, RegexOptions.IgnoreCase)
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
    let isIdColumn = columnName >> idColumn.IsMatch
    let findTableIdColumn column=
       let t = tableName column
       let m = idNamedTableColumn.Match <| columnName column
       m.Success && m.Groups.["value"].Value.Trim('_').Equals(t, StringComparison.InvariantCultureIgnoreCase)
    tables |> map (fun t->
        let tryFindColumn p = tryFind p t.Columns
        tryFindColumn isIdColumn
        |> Option.orElseWith (fun () -> tryFindColumn findTableIdColumn)
    ) |> choose (Option.map (fun c-> c.ColumnKey))
    |> Seq.toIReadOnlyList
[<CompiledName("ProbableForeignKeys")>]
let probableForeignKeys (tables: Table seq) (p:TableNameConventions) : _ IReadOnlyList =
    let tablePrefixes=TC.tablePrefixes p
    let idColumnPostfixes=TC.idColumnPostfixes p
    let tableNameTrim t = tablePrefixes.Replace(t, "")
    let tableMatchingColumn (column:Column) t=
        let trimmedTable = tableNameTrim(t.TableName)
        let columnWithoutPostfix = idColumnPostfixes.Replace(column.ColumnName, "")
        trimmedTable = columnWithoutPostfix && t.TableName <> column.TableName
    let idColumnPostfixesMatches (c:Column) = idColumnPostfixes.IsMatch c.ColumnName
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
        |> Option.map (fun t->{ ForeignKeyName=String.concat "__" [|c.TableName; t.TableName; c.ColumnName|]
                                Columns=[ {From=c.ColumnKey; To=getPrimaryKey t} ]} ))
    |> Seq.toIReadOnlyList

/// Search for tables that look like many to many based on their names.
[<CompiledName("ProbableNamedManyToManyTables")>]
let probableNamedManyToManyTables (tables: Table seq) (p:TableNameConventions) : Table IReadOnlyList =
    let tableNames = map (fun t-> StringIgnoreCase.Create t.TableName) tables 
    let tableNameSet = Set.ofSeq tableNames
    let pluralized tableName =
        p.PluralizationStrategies 
        |> map (fun s->
                match s with
                | DoNotPluralize -> tableName
                | PluralizeWithS -> sprintf "%ss" tableName
                | Pluralizer f -> f tableName)
    /// Returns the pluralized table name if it is a match. Note the default pluralization strategies:
    /// <br/>
    /// e.g. "customers" is a match for "customersclients" and "customers_clients"
    /// <br/>
    /// "customer" is a match for "customerclients" and "customer_clients"
    let startsWithNamePluralized tableName (possibleMatch:string) =
        pluralized tableName 
        |> Seq.tryFind (fun p-> possibleMatch.StartsWith(p, StringComparison.InvariantCultureIgnoreCase))

    let endsWithPluralized tableName (possibleMatch:string)=
        pluralized tableName
        |> Seq.tryFind (fun p-> possibleMatch.EndsWith(p, StringComparison.InvariantCultureIgnoreCase))

    let looksLikeManyToMany (t:Table) =
        let tableNamesWithoutSelf = 
            tableNameSet 
            |> Set.remove (StringIgnoreCase.Create t.TableName)
            |> Set.map (fun s-> s.CaseInsensitiveValue)
        let maybeFirstTableName = Seq.tryPick (fun ot -> 
            startsWithNamePluralized ot t.TableName) tableNamesWithoutSelf
        match maybeFirstTableName with
        | Some firstTableName ->
            let tableNameWithoutFirstTable = t.TableName.Substring(firstTableName.Length)
            let maybeSecondTableName = Seq.tryPick (fun ot -> 
                endsWithPluralized ot tableNameWithoutFirstTable) tableNamesWithoutSelf
            match maybeSecondTableName with
            | Some secondTableName ->
                let withoutSecond = tableNameWithoutFirstTable.Substring(0, tableNameWithoutFirstTable.Length - secondTableName.Length)
                p.ManyToManyInfixes 
                |> tryFind (fun infix-> withoutSecond.Equals(infix, StringComparison.InvariantCultureIgnoreCase))
                |> Option.isSome
            | None -> false
        | None -> false
    tables |> filter looksLikeManyToMany |> Seq.toIReadOnlyList
