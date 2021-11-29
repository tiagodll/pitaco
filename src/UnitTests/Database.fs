module Pitaco.UnitTests

open System
open Microsoft.Data.Sqlite
open Pitaco.Database
open Xunit

[<Fact>]
let ``Run Migrations`` () =
    let connection = new SqliteConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"))
    connection.Open()
    
    Migrations.runMigrations(connection)
    let version = Migrations.getCurrentVersion(connection)
    
    connection.Close()
    Assert.True(version > 0)