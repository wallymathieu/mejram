module Tests
open TestsFs
open Npgsql
open System
open Xunit
open MejramFs
open FSharp.Data



type SakilaTables = JsonProvider<"sakila.Tables.json">
let sakila = SakilaTables.Load "sakila.Tables.json"
let tablesInDb=
  use conn = new NpgsqlConnection("Server=127.0.0.1;Port=5432;Database=sakila;User Id=test;Password=test;")
  conn.Open()
  Sql.tables conn
  |> Seq.toList
[<Fact>]
let ``No tables missing`` () =
  let changes = Seq.changes sakila (fun t->t.TableName.ToLower()) tablesInDb (fun t->t.TableName.ToLower())
  Assert.Empty changes.ToRemove
[<Fact>]
let ``No tables added`` () =
  let changes = Seq.changes sakila (fun t->t.TableName.ToLower()) tablesInDb (fun t->t.TableName.ToLower())
  Assert.Empty changes.ToAdd
