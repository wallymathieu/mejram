module Tests
open TestsFs
open Npgsql
open System
open Xunit
open Mejram
open Mejram.Models
open FSharp.Data
let inline getTableName(r:^a) = ( ^a : ( member get_TableName: unit->String ) (r) )
let inline tableNameToLower t =(getTableName t).ToLower()
let inline getColumnName(r:^a) = ( ^a : ( member get_ColumnName: unit->String ) (r) )
let inline columnNameToLower t = (getColumnName t).ToLower()
let inline tableNameStartsWithPaymentP r = (getTableName r).StartsWith "payment_p"
type SakilaTables = JsonProvider<"sakila.Tables.json">
let sakila = SakilaTables.Load "sakila.Tables.json"
            |> Seq.filter (fun t->not <| tableNameStartsWithPaymentP t)
let tablesInDb=
  let defaultStr = "Server=127.0.0.1;Port=5432;Database=sakila;User Id=test;Password=test;"
  let sakilaConn = Environment.GetEnvironmentVariable("SAKILA_TEST_CONN")
  let connStr = if String.IsNullOrEmpty( sakilaConn ) then defaultStr else sakilaConn
  use conn = new NpgsqlConnection(connStr)
  conn.Open()
  Sql.tables conn
  |> Seq.toList
  |> Seq.filter (fun t->not <| tableNameStartsWithPaymentP t)
let tableChanges = 
  Seq.changes sakila tableNameToLower tablesInDb tableNameToLower
let columnChanges=
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
[<Fact>]
let ``There are sample foreign keys`` () =
  let payment = tablesInDb |> Seq.find ((=) "payment" << tableNameToLower)
  Assert.Contains({
    ForeignKeyName="payment_staff_id_fkey"
    ForeignKeys=[ 
    { From={TableName="payment";ColumnName="staff_id"}
      To={TableName="staff";ColumnName="staff_id"}} ] }, payment.ForeignKeys)
  Assert.Contains({
    ForeignKeyName="payment_rental_id_fkey"
    ForeignKeys=[ 
    { From={TableName="payment";ColumnName="rental_id"}
      To={TableName="rental";ColumnName="rental_id"}} ] }, payment.ForeignKeys)
  Assert.Contains({
    ForeignKeyName="payment_customer_id_fkey"
    ForeignKeys=[ 
    { From={TableName="payment";ColumnName="customer_id"}
      To={TableName="customer";ColumnName="customer_id"}} ] }, payment.ForeignKeys)
[<Fact>]
let ``There is sample primary key`` () =
  let payment = tablesInDb |> Seq.find ((=) "payment" << tableNameToLower)
  Assert.Equal(Some {
    PrimaryKeyName="payment_pkey"
    PrimaryKeys=[{TableName="payment";ColumnName="payment_id"}] }, payment.PrimaryKey)
  
[<Fact>]
let ``Primal keys`` () =
  let tables = tablesInDb |> Seq.filter Table.hasPrimalKey |> Seq.map tableNameToLower |> Seq.sort |> Seq.toList
  
  Assert.Equal<string list>(["customer"; "actor"; "category"; "film"; "address" 
                             "city";"country"; "inventory";"language";"payment"
                             "rental";"staff";"store"] |> List.sort, tables)
  