module MejramFs.Sql
open System.Data
open MejramFs.Models
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
    {ColumnKey=key; ColumnType=r.GetString 2; NotNullConstraint=getBoolean 3 r|>not; Number=r.GetInt16 4 }
  executeReader (sprintf @"SELECT c.table_name, c.column_name, c.data_type, c.is_nullable, c.ordinal_position
                  FROM INFORMATION_SCHEMA.COLUMNS c  
                  INNER JOIN INFORMATION_SCHEMA.TABLES t ON c.TABLE_NAME = t.TABLE_NAME  
                  WHERE %s" tableFilter) Map.empty toColumn c
  |> Seq.distinctBy (fun c->c.ColumnKey)

let foreignKeyConstraints c :ForeignKeyConstraint seq=
  let sql = (sprintf @"
            SELECT
                con.CONSTRAINT_NAME as constraint_name,
                tcon.table_name tcon_table_name,
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
                JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE con_col ON con_col.CONSTRAINT_NAME = con.CONSTRAINT_NAME
                JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE pkcon_col ON pkcon_col.CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
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
     ForeignKeys=list
      |> Seq.map snd
      |> Seq.map (fun (key,fkey)->{From=key;To=fkey})
      |> Seq.toList
  })

let primaryKeyConstraints c : PrimaryKeyConstraint seq=
  let sql = (sprintf @"SELECT 
              pkcon.constraint_name,
              pkcon.table_name, 
              pkcon_col.column_name
              FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pkcon 
              JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE pkcon_col ON pkcon_col.CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
              JOIN INFORMATION_SCHEMA.TABLES t ON pkcon.TABLE_NAME = t.TABLE_NAME
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

let tables c=
  let columns = columns c
                |> Seq.groupBy (fun col->col.TableName)
                //|> Map.ofSeq
  let tables = columns |> Seq.map fst
  let columnMap =columns |> Map.ofSeq
  let foreignKeyConstraintsMap = 
                foreignKeyConstraints c
                |> Seq.groupBy (fun constr->constr.TableName)
                |> Map.ofSeq
  let primaryKeyConstraintsMap = 
                primaryKeyConstraints c
                |> Seq.groupBy (fun constr->constr.TableName)
                |> Map.ofSeq
  let tableNameToTable tableName=
    let columns = Map.find tableName columnMap
    { TableName=tableName
      Columns=columns |> Seq.toList
      PrimaryKey= Map.tryFind tableName primaryKeyConstraintsMap |> Option.map Seq.head
      ForeignKeys= Map.tryFind tableName foreignKeyConstraintsMap |> Option.map Seq.toList |> Option.defaultValue [] }
  tables |> Seq.map tableNameToTable
