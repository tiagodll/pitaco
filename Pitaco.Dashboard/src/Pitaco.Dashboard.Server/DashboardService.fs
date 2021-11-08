namespace Pitaco.Dashboard.Server

open System
open Microsoft.AspNetCore.Hosting
open Bolero.Remoting
open Bolero.Remoting.Server
open Pitaco.Dashboard
open System.Collections.Generic

type DbUser = { 
    url: string
    password: string 
}

type DashboardService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Main.DashboardService>()

    let users = new List<DbUser>()

    override this.Handler =
        {
            getWebsite = ctx.Authorize <| fun () -> async {
                return {
                    url = "www.test.com"
                    title = "The test"
                }
            }

            //addBook = ctx.Authorize <| fun book -> async {
            //    books.Add(book)
            //}

            //removeBookByIsbn = ctx.Authorize <| fun isbn -> async {
            //    books.RemoveAll(fun b -> b.isbn = isbn) |> ignore
            //}

            signIn = fun (username, password) -> async {
                return try Some (users.Find(fun x -> x.url=username && x.password=password)).url
                        with
                        | ex -> None
            }
                //match List.tryFind (fun x -> x.url=username && x.password=password) users with
                //| None -> 
                //    return None
                //| Some x -> 
                //    do! ctx.HttpContext.AsyncSignIn(username, TimeSpan.FromDays(365.))
                //    return Some x.url
            //}

            signOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            getUsername = ctx.Authorize <| fun () -> async {
                return ctx.HttpContext.User.Identity.Name
            }

            signUp = fun (title, url, password) -> async {
                users.Add({ url=url; password=password }) |> ignore
            }
        }
