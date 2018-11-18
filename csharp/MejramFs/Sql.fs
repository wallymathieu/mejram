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
  let value = string (r.Item ordinal)
  ["yes"; "true"; "1"] 
  |> List.tryFind ((=) value) |> Option.isSome

let columns c =
  
  //
  let toColumn (r:IDataReader)=
    let key = {TableName= r.GetString 0; ColumnName= r.GetString 1}
    {ColumnKey=key; ColumnType=r.GetString 2; NotNullConstraint=getBoolean 3 r|>not; Number=r.GetInt16 4 }
  executeReader @"select c.TABLE_NAME , c.COLUMN_NAME , c.DATA_TYPE, c.IS_NULLABLE as IsNullable, c.ordinal_position
                              from INFORMATION_SCHEMA.COLUMNS c  
                              inner join INFORMATION_SCHEMA.TABLES t on c.TABLE_NAME = t.TABLE_NAME  
                              where t.TABLE_TYPE = 'BASE TABLE' " Map.empty toColumn c
  |> Seq.distinctBy (fun c->c.ColumnKey)

let foreignKeyConstraints c :ForeignKeyConstraint seq=
  let sql = @"
            SELECT
                con.CONSTRAINT_NAME as constraint_name,
                tcon.table_name tcon_table_name,
                pkcon.table_name pkcon_table_name,
                con_col.column_name con_col_column_name,
                pkcon_col.column_name pkcon_col_column_name
            FROM 
                INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS con
                JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tcon ON con.CONSTRAINT_NAME = tcon.CONSTRAINT_NAME
                JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS pkcon ON 
            	    con.UNIQUE_CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
            	    AND pkcon.CONSTRAINT_TYPE = 'PRIMARY KEY'
                JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE con_col ON con_col.CONSTRAINT_NAME = con.CONSTRAINT_NAME
                JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE pkcon_col ON pkcon_col.CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME "
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
  let sql = @"SELECT 
              pkcon.constraint_name,
              pkcon.table_name, 
              pkcon_col.column_name
              FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pkcon 
              JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE pkcon_col ON pkcon_col.CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
              WHERE pkcon.CONSTRAINT_TYPE = 'PRIMARY KEY'"
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
