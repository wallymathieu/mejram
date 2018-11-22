module Mejram.Analysis
open Mejram.Models
open System
open System.Text.RegularExpressions

type TableNameConventions = { TablePrefix:string list; KeyNames:string list}
with
    static member Default = {TablePrefix=["tbl_";"tbl";""]; KeyNames=["id";"_id"] }
    
module TableNameConventions=
    let tablePrefixes (p:TableNameConventions) =
        p.TablePrefix |> List.map Regex.Escape |> String.concat "|"
        |> fun s-> Regex(sprintf "^(%s)" s, RegexOptions.IgnoreCase)
    let idColumnPostfixes (p:TableNameConventions) =
        p.KeyNames |> List.map Regex.Escape |> String.concat "|"
        |> fun s-> Regex(sprintf "(%s)$" s, RegexOptions.IgnoreCase)
    /// exactly table prefix and column postfix
    let exactlyPrefixAndPostfix (p:TableNameConventions)= 
        let prefix = p.TablePrefix |> List.map Regex.Escape |> String.concat "|"
        let postfix = p.KeyNames |> List.map Regex.Escape |> String.concat "|"
        Regex(sprintf "^(%s)(%s)$" prefix postfix, RegexOptions.IgnoreCase)
    let prefixAndValueAndPostfix (p:TableNameConventions)= 
        let prefix = p.TablePrefix |> List.map Regex.Escape |> String.concat "|"
        let postfix = p.KeyNames |> List.map Regex.Escape |> String.concat "|"
        Regex(sprintf "^(%s)(?<value>\w+)(%s)$" prefix postfix, RegexOptions.IgnoreCase)
        
module TC = TableNameConventions
[<CompiledName("ProbablePrimaryKeys")>]
let probablePrimaryKeys (tables: Table list) (p:TableNameConventions)=
    let idColumn= TC.exactlyPrefixAndPostfix p
    let idNamedTableColumn= TC.prefixAndValueAndPostfix p
    let findIdColumn = columnName >> idColumn.IsMatch
    let findTableIdColumn column=
       let t = tableName column
       let m = idNamedTableColumn.Match <| columnName column
       m.Success && m.Groups.["value"].Value.Trim('_').Equals(t, StringComparison.InvariantCultureIgnoreCase)
    tables |> List.map (fun t->
        let tryFind p = t.Columns |> List.tryFind p
        tryFind findIdColumn
        |> Option.orElseWith (fun () -> tryFind findTableIdColumn)
    ) |> List.choose (Option.map (fun c-> c.ColumnKey))
[<CompiledName("ProbableForeignKeys")>]
let probableForeignKeys (tables: Table list) (p:TableNameConventions)=
    let tablePrefixes=TC.tablePrefixes p
    let idColumnPostfixes=TC.idColumnPostfixes p
    let tableNameTrim t = tablePrefixes.Replace(t, "")
    let tableMatchingColumn (column:Column) t=
        let trimmedTable = tableNameTrim(t.TableName)
        let columnWithOutPostfix = idColumnPostfixes.Replace(column.ColumnName, "")
        trimmedTable = columnWithOutPostfix && t.TableName <> column.TableName
    let columnsWithIdPostfix=
        tables 
        |> List.collect (fun t-> t.Columns |> List.filter (fun c->idColumnPostfixes.IsMatch c.ColumnName))
    let getPrimaryKey t=
        match t.PrimaryKey with
        | Some p->p.PrimaryKeys |> Seq.head
        | None-> failwithf "Could not find primary key for table %s" t.TableName

    columnsWithIdPostfix
    |> List.choose (fun c->
        tables
        |> List.tryFind (tableMatchingColumn c)
        |> Option.map (fun t->{ ForeignKeyName=String.concat "__" [|c.TableName; t.TableName; c.ColumnName|]
                                ForeignKeys=[ {From=c.ColumnKey; To=getPrimaryKey t} ]} ))
    
