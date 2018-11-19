module Tests
open TestsFs
open Npgsql
open System
open Xunit
open MejramFs
open FSharp.Data
let inline getTableName(r:^a) = ( ^a : ( member get_TableName: unit->String ) (r) )
let inline getColumnName(r:^a) = ( ^a : ( member get_ColumnName: unit->String ) (r) )
let inline tableNameStartsWithPaymentP r = (getTableName r).StartsWith "payment_p"
type SakilaTables = JsonProvider<"sakila.Tables.json">
let sakila = SakilaTables.Load "sakila.Tables.json"
            |> Seq.filter (fun t->not <| tableNameStartsWithPaymentP t)
let tablesInDb=
  use conn = new NpgsqlConnection("Server=127.0.0.1;Port=5432;Database=sakila;User Id=test;Password=test;")
  conn.Open()
  Sql.tables conn
  |> Seq.toList
  |> Seq.filter (fun t->not <| tableNameStartsWithPaymentP t)
let tableChanges = 
  let inline tableNameToLower t =(getTableName t).ToLower()
  Seq.changes sakila tableNameToLower tablesInDb tableNameToLower
let columnChanges=
  let inline columnNameToLower t = (getColumnName t).ToLower()
  tableChanges.ToChange 
  |> List.map (fun (t1,t2)-> Seq.changes t1.Columns columnNameToLower t2.Columns columnNameToLower)

[<Fact>]
let ``No tables missing`` () =
  Assert.Empty tableChanges.ToRemove
[<Fact>]
let ``No tables added`` () =
  Assert.Empty tableChanges.ToAdd
[<Fact>]
let ``No columns added`` () =
  let toAdd = columnChanges 
              |> List.filter (fun cchanges->not<| List.isEmpty cchanges.ToAdd)
  Assert.Empty toAdd
[<Fact>]
let ``No columns missing`` () =
  let toRemove = columnChanges 
                 |> List.filter (fun cchanges->not<| List.isEmpty cchanges.ToRemove)
  Assert.Empty toRemove
[<Fact>]
let ``No columns changed`` () =
  let toChange = columnChanges 
                 |> List.collect (fun cchanges->cchanges.ToChange)
                 |> List.filter (fun (c1,c2)->not ( c1.AttributeNumber = int c2.Number 
                                                    && c1.ColumnType = c2.ColumnType 
                                                    && c1.NotNullConstraint = c2.NotNullConstraint))
  Assert.Empty toChange