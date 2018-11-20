namespace Mejram.Models

type ColumnKey={TableName:string;ColumnName:string}
type Column={ColumnKey:ColumnKey;Number:int16;ColumnType:string;NotNullConstraint:bool}
with
  member this.ColumnName=this.ColumnKey.ColumnName
  member this.TableName=this.ColumnKey.TableName
type PrimaryKeyConstraint={PrimaryKeyName:string; PrimaryKeys: ColumnKey list}
with
  member this.TableName=
    let first =this.PrimaryKeys |> List.head
    first.TableName 

type ForeignKeyColumn={From:ColumnKey;To:ColumnKey }
type ForeignKeyConstraint= {ForeignKeyName:string; ForeignKeys: ForeignKeyColumn list}
with
  member this.TableName=
    let first =this.ForeignKeys |> List.head
    first.From.TableName 

type Table={
  TableName: string
  Columns: Column list
  PrimaryKey: PrimaryKeyConstraint option
  ForeignKeys: ForeignKeyConstraint list}
module Table=
  /// Primal keys are keys that does not have any foreign keys.
  let hasPrimalKey(this:Table)=
    match this.PrimaryKey with
    | Some pk -> 
      let foreignKeysColumns = 
        this.ForeignKeys
        |> List.collect (fun fk->fk.ForeignKeys)
        |> List.map (fun fk->fk.From)
      let inForeignKeys key=List.contains key foreignKeysColumns
      pk.PrimaryKeys 
        |> List.exists inForeignKeys 
        |> not
    | None -> false
