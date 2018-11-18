namespace MejramFs
open MejramFs.Models
module ProbableForeignKeyAnalysis=
    open System
    open System.Text.RegularExpressions

    type Parameters = { TablePrefix:string list; KeyNames:string list}
    with
        member __.Default = {TablePrefix=["tbl"]; KeyNames=["id"] }
    let getProbableForeignKeys (tables: Table list) (p:Parameters)=
        let tablePrefixes = p.TablePrefix 
                            |> List.map Regex.Escape
                            |> String.concat "|"
                            |> fun s-> Regex(sprintf "^(%s)" s, RegexOptions.IgnoreCase)
        let idColumnPostfixes = 
                            p.KeyNames
                            |> List.map Regex.Escape
                            |> String.concat "|"
                            |> fun s-> Regex(sprintf "(%s)$" s, RegexOptions.IgnoreCase)
        let tableNameTrim t = tablePrefixes.Replace(t, "")
        let replaceColumnPrefix text=Regex.Replace(text, @"^[a-zA-Z]{2,4}_", "")
        let columnWithKeyName keyName a=
            replaceColumnPrefix(a.ColumnName).Equals(keyName, StringComparison.InvariantCultureIgnoreCase)
        let tableMatchingColumn (column:Column) t=
            tableNameTrim(t.TableName) = idColumnPostfixes.Replace(column.ColumnName, "")
            && t.TableName <> column.TableName
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
            |> Option.map (fun t->{ ForeignKeyName=String.concat "_" [|c.TableName; t.TableName; c.ColumnName|]; 
                                     ForeignKeys=[ {From=c.ColumnKey; To=getPrimaryKey t} ]} ))
        
module Say =
    let hello name =
        printfn "Hello %s" name
