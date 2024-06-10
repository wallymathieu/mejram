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

type Table with
  /// Primal keys are primary keys that does not have any foreign keys.
  /// In a database, you would expect the domain entities to have primal keys.
  /// If other entities have such keys, then it can be due to ORM implementation restrictions.
  member this.HasPrimalKey() =
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

[<AutoOpen>]
module Model=
  let inline columnName(r:^a) = ( ^a : ( member get_ColumnName: unit->string ) (r) )
  let inline tableName(r:^a) = ( ^a : ( member get_TableName: unit->string ) (r) )

/// A table that might be a many to many table.
type MaybeManyToManyTable = {Table: Table; FirstOtherTable: string; SecondOtherTable: string}
with
    /// Has matching outgoing foreign keys. This is a strong indicator that the table is a many to many connection table.
    member self.HasMatchingOutgoingForeignKeys() =
        let matchesOneOfTheTables c= c.To.TableName = self.FirstOtherTable || c.To.TableName = self.SecondOtherTable
        let matches =
            self.Table.ForeignKeys
            |> List.collect _.Columns
            |> List.filter matchesOneOfTheTables
            |> List.map _.To.TableName
            |> List.distinct
        matches |> List.length = 2

[<AutoOpen>]
module internal InternalModels=
    open System
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
