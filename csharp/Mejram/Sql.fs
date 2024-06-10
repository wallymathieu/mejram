module Mejram.Sql
open System.Data
open Mejram.Models
open System
open System.Collections.Generic
open Microsoft.FSharp.Linq
open System
[<AutoOpen>]
module internal Internals=
  let executeReader text (parameters:Map<string,obj>) map (c:IDbConnection) =
    use command= c.CreateCommand()
    command.CommandText <- text
    command.CommandType <- CommandType.Text
    for kv in parameters do 
      let p= command.CreateParameter()
      p.ParameterName <- kv.Key
      p.Value <- kv.Value
      command.Parameters.Add p |> ignore
    use reader = command.ExecuteReader()
    seq{
      while reader.Read() do 
        yield map reader
    }|> Seq.toArray
  let getBoolean (ordinal:int) (r:IDataReader)=
    let value = (string (r.Item ordinal)).ToLower()
    ["yes"; "true"; "1"] |> List.contains value
  let tableFilter = "t.TABLE_TYPE = 'BASE TABLE' AND t.TABLE_SCHEMA <> 'pg_catalog' AND t.TABLE_SCHEMA <> 'information_schema'"
  let columns c =
    
    //
    let toColumn (r:IDataReader)=
      let key = {TableName= r.GetString 0; ColumnName= r.GetString 1}
      {ColumnKey=key; ColumnType=r.GetString 2; NotNullConstraint=getBoolean 3 r|>not; Number=r.GetInt32 4 }
    executeReader (sprintf @"SELECT c.table_name, c.column_name, c.data_type, c.is_nullable, c.ordinal_position
                    FROM INFORMATION_SCHEMA.COLUMNS c  
                    INNER JOIN INFORMATION_SCHEMA.TABLES t ON c.TABLE_NAME = t.TABLE_NAME  
                    WHERE %s" tableFilter) Map.empty toColumn c
    |> Seq.distinctBy (fun c->c.ColumnKey)
  
  let foreignKeyConstraints c :ForeignKeyConstraint seq=
    let sql = (sprintf @"
              SELECT
                  con.constraint_name as constraint_name,
                  tcon.table_name con_table_name,
                  pkcon.table_name pkcon_table_name,
                  con_col.column_name con_col_column_name,
                  pkcon_col.column_name pkcon_col_column_name
              FROM 
                  INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS con
                  JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tcon ON con.CONSTRAINT_NAME = tcon.CONSTRAINT_NAME
                  JOIN INFORMATION_SCHEMA.TABLES t ON tcon.TABLE_NAME = t.TABLE_NAME
                  JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS pkcon ON 
                    con.UNIQUE_CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
                    AND pkcon.CONSTRAINT_TYPE = 'PRIMARY KEY'
                  JOIN INFORMATION_SCHEMA.key_column_usage con_col ON con_col.CONSTRAINT_NAME = con.CONSTRAINT_NAME
                  JOIN INFORMATION_SCHEMA.key_column_usage pkcon_col ON pkcon_col.CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
              WHERE %s" tableFilter)
    let map (r:IDataReader)=
      let constraint_name = r.GetString 0
      let con_table_name = r.GetString 1
      let pkcon_table_name = r.GetString 2
      let con_col_column_name = r.GetString 3
      let pkcon_col_column_name = r.GetString 4
      
      let con_key = {TableName= con_table_name; ColumnName= con_col_column_name}
      let pkcon_key = {TableName= pkcon_table_name; ColumnName= pkcon_col_column_name}
      
      (constraint_name, (con_key, pkcon_key))
    executeReader sql Map.empty map c
    |> Seq.groupBy fst
    |> Seq.map (fun (name, list)-> {
       ForeignKeyName=name;
       Columns=list
        |> Seq.map snd
        |> Seq.map (fun (key,fkey)->{From=key;To=fkey})
        |> Seq.toList
    })
  
  let primaryKeyConstraints c : PrimaryKeyConstraint seq=
    let sql = (sprintf @"
                SELECT
                  pkcon.constraint_name,
                  pkcon.table_name,
                  pkcon_col.column_name
                FROM INFORMATION_SCHEMA.table_constraints pkcon 
                JOIN INFORMATION_SCHEMA.key_column_usage pkcon_col ON pkcon_col.constraint_name = pkcon.constraint_name
                JOIN INFORMATION_SCHEMA.tables t ON pkcon.table_name = t.table_name
                WHERE pkcon.CONSTRAINT_TYPE = 'PRIMARY KEY' AND %s" tableFilter)
    let map (r:IDataReader)=
      let constraint_name = r.GetString 0
      let table_name = r.GetString 1
      let column_name = r.GetString 2
      
      let key ={TableName= table_name; ColumnName= column_name}
      (constraint_name, key)
    executeReader sql Map.empty map c
    |> Seq.groupBy fst
    |> Seq.map (fun (name, list)-> {
       PrimaryKeyName=name;
       PrimaryKeys=list
        |> Seq.map snd
        |> Seq.toList
    })
[<CompiledName("FSharpTableCount")>]
let tableCount tableName c=
  try
    let map (r:IDataReader)=
      r.GetInt32 0
    let sql =sprintf "SELECT COUNT(*) _count FROM %s" tableName
    executeReader sql Map.empty map c
    |> Seq.head |> Some
  with | _ -> None
[<CompiledName("TableCount")>]
let __tableCount tableName c= 
  tableCount tableName c |> Option.toNullable
[<CompiledName("FSharpKeyWeight")>]
let keyWeight (fk:ForeignKeyConstraint) (tables:IDictionary<string,Table>) c=
  let table = tables.[fk.TableName()]
  let canBeNull=fk.Columns |> List.exists
                            (fun p ->
                              let column = table.Columns |> Seq.find(fun col -> col.ColumnName() = p.From.ColumnName)
                              not <| column.NotNullConstraint)
  if canBeNull then
    try
      let map (r:IDataReader)=
        r.GetInt32 0
      let fksFilter = String.Join(" AND ", fk.Columns |> List.map (fun keys->keys.From.ColumnName+" IS NOT NULL "))
      let sql = sprintf @"SELECT COUNT(*) _count FROM %s WHERE %s" table.TableName fksFilter
      executeReader sql Map.empty map c
      |> Seq.head |> Some
    with | _ -> None
  else
    tableCount table.TableName c
[<CompiledName("KeyWeight")>]
let __keyWeight (fk:ForeignKeyConstraint) (tables:IDictionary<string,Table>) c= 
  keyWeight fk tables c |> Option.toNullable
[<CompiledName("Tables")>]
let tables c=
  let columns = columns c
                |> Seq.groupBy (fun col->col.TableName())
                //|> Map.ofSeq
  let tables = columns |> Seq.map fst
  let columnMap =columns |> Map.ofSeq
  let foreignKeyConstraints = foreignKeyConstraints c
  let primaryKeyConstraints = primaryKeyConstraints c
  let foreignKeyConstraintsMap =
                foreignKeyConstraints |> Seq.groupBy (fun constr->constr.TableName()) |> Map.ofSeq
  let primaryKeyConstraintsMap = 
                primaryKeyConstraints |> Seq.groupBy (fun constr->constr.TableName()) |> Map.ofSeq
  let tableNameToTable tableName=
    let columns = Map.find tableName columnMap
    { TableName=tableName
      Columns=columns |> Seq.toList
      PrimaryKey= Map.tryFind tableName primaryKeyConstraintsMap |> Option.map Seq.head
      ForeignKeys= Map.tryFind tableName foreignKeyConstraintsMap |> Option.map Seq.toList |> Option.defaultValue [] }
  tables |> Seq.map tableNameToTable
