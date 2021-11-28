module Pitaco.Database.Queries

open System
open System.Collections.Generic
open Microsoft.Data.Sqlite
open Pitaco.Shared.Model
open Pitaco.Database.Types

let connection = new SqliteConnection (Environment.GetEnvironmentVariable("CONNECTION_STRING"))

let runNQ (cmd:SqliteCommand) =
    connection.Open()
    let r = cmd.ExecuteNonQuery()
    connection.Close()

let runQ (cmd:SqliteCommand) f =
    connection.Open()
    let reader = cmd.ExecuteReader()
    while reader.Read() do
        f(reader)
    connection.Close()
    

module Website =

    let ByUrlAndPassword url password =
        let mutable result = None
        let selectSql = "SELECT * FROM websites WHERE url=@url AND password=@password"
        let command = new SqliteCommand(selectSql, connection)
        command.Parameters.AddWithValue("@url", url) |> ignore
        command.Parameters.AddWithValue("@password", password) |> ignore
        
        runQ command (fun (reader:SqliteDataReader) ->
            result <- Some { key=reader.["id"].ToString(); url=reader.["url"].ToString(); title=reader.["title"].ToString()}
        )
        result

    let ById id =
        let mutable result = None
        let selectSql = "SELECT * FROM websites WHERE id=@id"
        let command = new SqliteCommand(selectSql, connection)
        command.Parameters.AddWithValue("@id", id) |> ignore
        
        runQ command (fun (reader:SqliteDataReader) ->
            result <- Some { key=reader.["id"].ToString(); url=reader.["url"].ToString(); title=reader.["title"].ToString()}
        )
        result

    let Add (website:DbWebsite) =
        use command = new SqliteCommand("INSERT INTO websites (id, url, title, password) VALUES (@id, @url, @title, @password)", connection)       
        command.Parameters.AddWithValue("@id", website.id) |> ignore
        command.Parameters.AddWithValue("@url", website.url) |> ignore
        command.Parameters.AddWithValue("@title", website.title) |> ignore
        command.Parameters.AddWithValue("@password", website.password) |> ignore
        runNQ command

module Comment =

    let Add (comment:DbComment) =
        use command = new SqliteCommand("INSERT INTO comments (id, ws_id, url, author_id, author, text) VALUES (@id, @ws_id, @url, @author_id, @author, @text)", connection)       
        command.Parameters.AddWithValue("@id", comment.id) |> ignore
        command.Parameters.AddWithValue("@ws_id", comment.wsId) |> ignore
        command.Parameters.AddWithValue("@url", comment.url) |> ignore
        command.Parameters.AddWithValue("@author", comment.author) |> ignore
        command.Parameters.AddWithValue("@author_id", comment.authorId) |> ignore
        command.Parameters.AddWithValue("@text", comment.text) |> ignore
        try
            runNQ command
            None
        with msg -> 
            Some msg
    
    let Delete id =
        use command = new SqliteCommand("DELETE FROM comments WHERE id=@id", connection)       
        command.Parameters.AddWithValue("@id", id) |> ignore
        try
            runNQ command
            None
        with msg -> 
            Some msg

    let ByUrl url =
        let result = new List<Comment>()
        let command = new SqliteCommand("SELECT * FROM comments WHERE url=@url", connection)
        command.Parameters.AddWithValue("@url", url) |> ignore
        runQ command (fun (reader:SqliteDataReader) ->
            result.Add({
                key = reader.["id"].ToString()
                wskey = reader.["ws_id"].ToString()
                url = reader.["url"].ToString()
                author = reader.["author"].ToString()
                text = reader.["text"].ToString()
                timestamp = match DateTime.TryParse (reader.["timestamp"].ToString()) with
                            | true, date -> date
                            | false, _ -> DateTime.MinValue
            })
        )
        result.ToArray()
        
    let UrlsByWebsite wsId =
        let result = new List<string>()
        let command = new SqliteCommand("SELECT DISTINCT url FROM comments WHERE ws_id=@ws_id", connection)
        command.Parameters.AddWithValue("@ws_id", wsId) |> ignore
        runQ command (fun (reader:SqliteDataReader) ->
            result.Add(reader.["url"].ToString())
        )
        result.ToArray()