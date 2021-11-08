namespace Pitaco.Dashboard.Server

open System
open Microsoft.AspNetCore.Hosting
open Bolero.Remoting
open Bolero.Remoting.Server
open Pitaco.Dashboard

type DashboardService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Main.DashboardService>()

    override this.Handler =
        {
            getWebsite = ctx.Authorize <| fun () -> async {
                return {
                    url = "www.test.com"
                    name = "The test"
                }
            }

            //addBook = ctx.Authorize <| fun book -> async {
            //    books.Add(book)
            //}

            //removeBookByIsbn = ctx.Authorize <| fun isbn -> async {
            //    books.RemoveAll(fun b -> b.isbn = isbn) |> ignore
            //}

            signIn = fun (username, password) -> async {
                if password = "password" then
                    do! ctx.HttpContext.AsyncSignIn(username, TimeSpan.FromDays(365.))
                    return Some username
                else
                    return None
            }

            signOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            getUsername = ctx.Authorize <| fun () -> async {
                return ctx.HttpContext.User.Identity.Name
            }
        }
