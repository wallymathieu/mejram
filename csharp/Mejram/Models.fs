namespace Mejram.Models

type ColumnKey={TableName:string;ColumnName:string}
type Column={ColumnKey:ColumnKey;Number:int;ColumnType:string;NotNullConstraint:bool}
with
  member this.ColumnName=this.ColumnKey.ColumnName
  member this.TableName=this.ColumnKey.TableName
type PrimaryKeyConstraint={PrimaryKeyName:string; PrimaryKeys: ColumnKey list}
with
  member this.TableName=
    let first =this.PrimaryKeys |> List.head
    first.TableName 

type ForeignKeyColumn={From:ColumnKey;To:ColumnKey }
type ForeignKeyConstraint= {ForeignKeyName:string; Columns: ForeignKeyColumn list}
with
  /// From table
  member this.TableName=
    let first =this.Columns |> List.head
    first.From.TableName 

type Table={
  TableName: string
  Columns: Column list
  PrimaryKey: PrimaryKeyConstraint option
  ForeignKeys: ForeignKeyConstraint list}
module Table=
  /// Primal keys are primary keys that does not have any foreign keys.
  /// In a database, you would expect the domain entities to have primal keys.
  /// If other entities have such keys, then it can be due to ORM implementation restrictions.
  let hasPrimalKey(this:Table)=
    match this.PrimaryKey with
    | Some pk -> 
      let foreignKeysColumns = 
        this.ForeignKeys
        |> List.collect (fun fk->fk.Columns)
        |> List.map (fun fk->fk.From)
      let inForeignKeys key=List.contains key foreignKeysColumns
      pk.PrimaryKeys 
        |> List.exists inForeignKeys 
        |> not
    | None -> false

type Table with
  member this.HasPrimalKey() = Table.hasPrimalKey this

[<AutoOpen>]
module Model=
  let inline columnName(r:^a) = ( ^a : ( member get_ColumnName: unit->string ) (r) )
  let inline tableName(r:^a) = ( ^a : ( member get_TableName: unit->string ) (r) )
