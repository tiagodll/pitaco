module Pitaco.Database.Migrations                 
open System
open Microsoft.Data.Sqlite

type MigrationItem = {
    version: int
    content: string
}
let connection = new SqliteConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"))

let getCurrentVersion connection =
    let selectCommand = new SqliteCommand("SELECT value FROM Config WHERE label='DbVersion'", connection)
    let v = selectCommand.ExecuteScalar()
    match v with
        | null -> 
            "INSERT INTO Config (label, value) VALUES('DbVersion', '0');"
            |> (fun x -> (new SqliteCommand(x, connection)).ExecuteNonQuery())
            |> ignore
            0
        | x -> x.ToString() |> int

let applyMigration connection item =
    let cmd = new SqliteCommand(item.content, connection)
    cmd.ExecuteNonQuery() |> ignore

    let cmdVersion = new SqliteCommand("UPDATE Config SET value=@version WHERE label='DbVersion'", connection)
    cmdVersion.Parameters.AddWithValue("@version", item.version) |> ignore
    cmdVersion.ExecuteNonQuery() |> ignore

let migrate =
    connection.Open()

    "CREATE TABLE IF NOT EXISTS Config (label TEXT, value TEXT, timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)"
    |> (fun x -> (new SqliteCommand(x, connection)).ExecuteNonQuery())
    |> ignore

    let currentVersion = getCurrentVersion connection
    
    [|
        {version=1; content="""
        CREATE TABLE websites (
        id TEXT,
        url TEXT,
        title TEXT,                              
        password TEXT,
        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP);
        
        INSERT INTO websites(id, url, title, password) VALUES ('tiagodallignacom', 'tiago.dalligna.com', 'the blog', 'asd');
        
        CREATE TABLE comments (
        id TEXT,
        ws_id TEXT,
        url TEXT,
        author_id TEXT,
        author TEXT,
        text TEXT,
        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP);
        """}
    |]
    |> Array.filter (fun x -> x.version > currentVersion)
    |> Array.iter (applyMigration connection)

    connection.Close()