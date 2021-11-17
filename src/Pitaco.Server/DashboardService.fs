namespace Pitaco.Server

open System
open System.Linq
open Microsoft.AspNetCore.Hosting
open Bolero.Remoting
open Bolero.Remoting.Server
open Azure.Data.Tables
open Azure

open Pitaco
open Pitaco.Shared.Model

type DbWebsite(partitionkey, rowkey, timestamp, Url, Title, Password) =
    interface ITableEntity with
        member this.ETag
            with get (): ETag = 
                raise (System.NotImplementedException())
            and set (v: ETag): unit = 
                raise (System.NotImplementedException())
        member this.PartitionKey
            with get (): string = partitionkey
            and set (v: string): unit = 
                raise (System.NotImplementedException())
        member this.RowKey
            with get (): string = rowkey
            and set (v: string): unit = 
                raise (System.NotImplementedException())
        member this.Timestamp
            with get (): Nullable<DateTimeOffset> = 
                raise (System.NotImplementedException())
            and set (v: Nullable<DateTimeOffset>): unit = 
                raise (System.NotImplementedException())
    new() = DbWebsite(null, null, null, null, null, null)
    member val Url = Url with get, set
    member val Title = Title with get, set
    member val Password = Password with get, set
    member val Timestamp = timestamp with get, set
    member val PartitionKey = partitionkey with get, set
    member val RowKey = rowkey with get, set
    
type DbComment(partitionkey, rowkey, Url, Text, Author) =
    interface ITableEntity with
        member this.ETag
            with get (): ETag = 
                raise (System.NotImplementedException())
            and set (v: ETag): unit = 
                raise (System.NotImplementedException())
        member this.PartitionKey
            with get (): string = partitionkey
            and set (v: string): unit = 
                raise (System.NotImplementedException())
        member this.RowKey
            with get (): string = rowkey
            and set (v: string): unit = 
                raise (System.NotImplementedException())
        member this.Timestamp
            with get (): Nullable<DateTimeOffset> = 
                raise (System.NotImplementedException())
            and set (v: Nullable<DateTimeOffset>): unit = 
                raise (System.NotImplementedException())
    new() = DbComment(null, null, null, null, null)
    member val Url = Url with get, set
    member val Text = Text with get, set
    member val Author = Author with get, set
    member val PartitionKey = partitionkey with get, set
    member val RowKey = rowkey with get, set

module DashboardServiceHelper =
    let DbWebsitePartition = "websites"
    let DbWebsiteToWebsite (ws:DbWebsite) =
        {
            key = ws.RowKey
            url = match ws.Url with | null -> "" | u -> u
            title = match ws.Title with | null -> "" | u -> u
        }

    let DbCommentToComment (comment:DbComment) =
//        let time = comment.Timestamp
        {
            wskey = comment.PartitionKey
            key = comment.RowKey
            timestamp = DateTime.MinValue //comment.Timestamp
            url = match comment.Url with | null -> "" | u -> u
            text = match comment.Text with | null -> "" | t -> t
            author = match comment.Author with | null -> "" | a -> a
        }

type DashboardService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.DashboardService.DashboardService>()
    
    let connStr = Environment.GetEnvironmentVariable("CONNECTION_STRING")

    let tableService = TableServiceClient(connStr)
    let tableWebsites = tableService.GetTableClient("websites")
//    tableWebsites.CreateIfNotExists() |> ignore
    let tableComments = tableService.GetTableClient("comments")
//    tableComments.CreateIfNotExists() |> ignore


    override this.Handler =
        {
            getWebsite = ctx.Authorize <| fun () -> async {
                return $"RowKey eq '{ctx.HttpContext.User.Identity.Name}'"
                |> tableWebsites.Query<DbWebsite>
                |> Enumerable.ToArray
                |> Array.map DashboardServiceHelper.DbWebsiteToWebsite
                |> Array.tryHead
            }

            signIn = fun (url, password) -> async {
                return $"Url eq '{url}' and Password eq '{password}'"
                |> tableWebsites.Query<DbWebsite>
                |> Enumerable.ToArray
                |> Array.map DashboardServiceHelper.DbWebsiteToWebsite
                |> Array.tryHead
            }

            signOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            signUp = fun (signUpRequest) -> async {
                let makeKey url =
                    String.filter (fun x -> x <> '.' && x <> '/') url
                let row = new DbWebsite(DashboardServiceHelper.DbWebsitePartition,
                                        makeKey(signUpRequest.url),
                                        null,
                                        signUpRequest.url,
                                        signUpRequest.title,
                                        signUpRequest.password)
                
                let x = tableWebsites.AddEntity row |> ignore
                    
                return None
            }

            addComment = fun (comment) -> async {
                let rowKey = Guid.NewGuid().ToString()
                let row = new DbComment(comment.wskey,
                                        rowKey,
                                        comment.url,
                                        comment.text,
                                        comment.author)
                
                let x = tableComments.AddEntity row |> ignore
//                comments <- List.append [{ wskey=comment.wskey; key=comment.key; url=comment.url; text=comment.text; author=comment.author; timestamp=DateTime.Now}] comments
                return None
            }

            getComments = fun (url) -> async {
                return $"Url eq '{url}'"
                |> tableComments.Query<DbComment>
                |> Enumerable.ToArray
                |> Array.map DashboardServiceHelper.DbCommentToComment
                |> Array.toList
            }
            
            getPagesWithComments = fun (key) -> async {
                let comments =
                    $"PartitionKey eq '{key}'"
                    |> tableComments.Query<DbComment>
                    |> Enumerable.ToArray
                    |> Array.map DashboardServiceHelper.DbCommentToComment
                    |> Array.toList
                
                return comments
                |> List.distinctBy (fun x -> x.url)
                |> List.map (fun x -> {
                    wskey = x.wskey
                    url = x.url
                    comments = comments |> List.filter (fun y -> y.url = x.url)
                })
            }
        }
