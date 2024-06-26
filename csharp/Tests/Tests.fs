module Tests
open TestsFs
open Npgsql
open System
open Xunit
open Mejram
open Mejram.Models
open FSharp.Data
[<AutoOpen>]
module Model=
  let inline columnName(r:^a) = ( ^a : ( member get_ColumnName: unit->string ) (r) )
  let inline tableName(r:^a) = ( ^a : ( member get_TableName: unit->string ) (r) )

let inline tableNameToLower t =(tableName t).ToLower()
let inline columnNameToLower t = (columnName t).ToLower()
let inline tableNameStartsWithPaymentP r = (tableName r).StartsWith "payment_p"
type SakilaTables = JsonProvider<"sakila.Tables.json">
let sakila = SakilaTables.Load "sakila.Tables.json"
            |> Seq.filter (fun t->not <| tableNameStartsWithPaymentP t)
let sakila_1 = Newtonsoft.Json.JsonConvert.DeserializeObject<Table ResizeArray>( IO.File.ReadAllText( "sakila_1.Tables.json"))
                        |> Seq.filter (fun t->not <| tableNameStartsWithPaymentP t)
let sakilaWithTblPrefix = Newtonsoft.Json.JsonConvert.DeserializeObject<Table ResizeArray>( IO.File.ReadAllText("sakila_tbl.Tables.json"))
type TC = Analysis.TableNameConventions
let defaultConventions = TC.Default()
let createConn ()=
  let defaultStr = "Server=127.0.0.1;Port=5432;Database=sakila;User Id=test;Password=test;"
  let sakilaConn = Environment.GetEnvironmentVariable("SAKILA_TEST_CONN")
  let connStr = if String.IsNullOrEmpty( sakilaConn ) then defaultStr else sakilaConn
  let conn = new NpgsqlConnection(connStr)
  conn.Open()
  conn
let tablesInDb= lazy (
  use conn =createConn()
  Sql.tables conn
  |> Seq.toList
  |> Seq.filter (not << tableNameStartsWithPaymentP) )
let tableChanges = lazy (
  Seq.changes sakila tableNameToLower tablesInDb.Value tableNameToLower )
let columnChanges= lazy (
  tableChanges.Value.ToChange 
  |> List.map (fun (t1,t2)-> Seq.changes t1.Columns columnNameToLower t2.Columns _.ColumnName().ToLower()) )
let findTableWithName name tables = tables |> Seq.find ((=) name << tableNameToLower)
[<Fact>]
let ``No tables missing`` () =
  Assert.Empty tableChanges.Value.ToRemove
[<Fact>]
let ``No tables added`` () =
  Assert.Empty tableChanges.Value.ToAdd
[<Fact>]
let ``No columns added`` () =
  let toAdd = columnChanges.Value
              |> List.filter (fun cchanges->not<| List.isEmpty cchanges.ToAdd)
  Assert.Empty toAdd
[<Fact>]
let ``No columns missing`` () =
  let toRemove = columnChanges.Value
                 |> List.filter (fun cchanges->not<| List.isEmpty cchanges.ToRemove)
  Assert.Empty toRemove
[<Fact>]
let ``No columns changed`` () =
  let toChange = columnChanges.Value
                 |> List.collect (fun cchanges->cchanges.ToChange)
                 |> List.filter (fun (c1,c2)->not ( c1.AttributeNumber = c2.Number 
                                                    && c1.ColumnType = c2.ColumnType 
                                                    && c1.NotNullConstraint = c2.NotNullConstraint))
  Assert.Empty toChange
[<Fact>]
let ``There are sample foreign keys`` () =
  let payment = tablesInDb.Value |> Seq.find ((=) "payment" << tableNameToLower)
  Assert.Contains({
    ForeignKeyName="payment_staff_id_fkey"
    Columns=[ 
    { From={TableName="payment";ColumnName="staff_id"}
      To={TableName="staff";ColumnName="staff_id"}} ] }, payment.ForeignKeys)
  Assert.Contains({
    ForeignKeyName="payment_rental_id_fkey"
    Columns=[ 
    { From={TableName="payment";ColumnName="rental_id"}
      To={TableName="rental";ColumnName="rental_id"}} ] }, payment.ForeignKeys)
  Assert.Contains({
    ForeignKeyName="payment_customer_id_fkey"
    Columns=[ 
    { From={TableName="payment";ColumnName="customer_id"}
      To={TableName="customer";ColumnName="customer_id"}} ] }, payment.ForeignKeys)
[<Fact>]
let ``There is sample primary key`` () =
  let payment = findTableWithName "payment" tablesInDb.Value
  Assert.Equal(Some {
    PrimaryKeyName="payment_pkey"
    PrimaryKeys=[{TableName="payment";ColumnName="payment_id"}] }, payment.PrimaryKey)

[<Fact>]
let ``Can get count for each foreign key`` () =
  use conn =createConn()
  let map =tablesInDb.Value |> Seq.map (fun t-> t.TableName, t) |> Map.ofSeq
  let keyWeights = tablesInDb.Value
                   |> Seq.collect (fun t-> t.ForeignKeys)
                   |> Seq.map( fun fk->fk.ForeignKeyName, Sql.keyWeight fk map conn)
                   |> Seq.toList
  Assert.Contains( ("store_address_id_fkey", Some 0), keyWeights)
[<Fact>]
let ``Can infer primary keys`` () =
  let store = findTableWithName "store" tablesInDb.Value
  let primary = Analysis.probablePrimaryKeys [store] defaultConventions
  Assert.Contains({TableName="store";ColumnName="store_id"}, primary)

[<Fact>]
let ``Can infer foreign keys`` () =
  let tables = ["store"; "staff"; "address"] |> List.map (fun name->findTableWithName name tablesInDb.Value)
  let foreignKeys = Analysis.probableForeignKeys tables defaultConventions
                    |> Seq.map (fun fk->fk.ForeignKeyName)
                    |> Seq.toList
  Assert.Contains("store__address__address_id", foreignKeys)
  Assert.Contains("staff__address__address_id", foreignKeys)
  Assert.Contains("staff__store__store_id", foreignKeys)

[<Fact>]
let ``Primal keys`` () =
  let tables = tablesInDb.Value |> Seq.filter _.HasPrimalKey() |> Seq.map tableNameToLower |> Seq.sort |> Seq.toList
  Assert.Equal<string list>(["customer"; "actor"; "category"; "film"; "address" 
                             "city";"country"; "inventory";"language";"payment"
                             "rental";"staff";"store"] |> List.sort, tables)
let listOfManyToManyToTableName manyToMany= (manyToMany: MaybeManyToManyTable seq) |> Seq.map _.Table |> Seq.map tableNameToLower |> Seq.sort |> Seq.toList

[<Fact>]
let ``Can infer probable many to many tables`` () =
  let analyzed = Analysis.probableManyToManyTables sakila_1 defaultConventions
  let manyToMany = analyzed |> listOfManyToManyToTableName
  Assert.Equal<string list>(["film_actor"; "film_category"], manyToMany)
  Assert.Equal(2, analyzed |>  Seq.filter _.HasMatchingOutgoingForeignKeys() |> Seq.length)
[<Fact>]
let ``Can infer probable many to many tables with tbl_ prefix`` () =
  let analyzed = Analysis.probableManyToManyTables sakilaWithTblPrefix defaultConventions
  let manyToMany = analyzed |> listOfManyToManyToTableName
  Assert.Equal<string list>(["tbl_film_actor"; "tbl_film_category"], manyToMany)
  Assert.Equal(2, analyzed |>  Seq.filter _.HasMatchingOutgoingForeignKeys() |> Seq.length)