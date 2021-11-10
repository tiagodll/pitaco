namespace Pitaco.Dashboard.Server

open System
open Microsoft.AspNetCore.Hosting
open Bolero.Remoting
open Bolero.Remoting.Server
open Pitaco.Dashboard
open System.Collections.Generic

type DbUser = { 
    url: string
    title: string
    password: string 
}

type DashboardService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.DashboardService.DashboardService>()

    let mutable users = [{url="tiago.dalligna.com"; title="cyborg"; password="asd"}]

    override this.Handler =
        {
            getWebsite = ctx.Authorize <| fun () -> async {
                return {
                    url = "www.test.com"
                    title = "The test"
                }
            }

            signIn = fun (username, password) -> async {
                match List.tryFind (fun x -> x.url=username && x.password=password) users with
                    | None -> 
                        return None
                    | Some x -> 
                        do! ctx.HttpContext.AsyncSignIn(username, TimeSpan.FromDays(365.))
                        return Some x.url
            }

            signOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            getUsername = ctx.Authorize <| fun () -> async {
                return ctx.HttpContext.User.Identity.Name
            }

            signUp = fun (signUpRequest) -> async {
                users <- List.append [{ url=signUpRequest.url; title=signUpRequest.title; password=signUpRequest.password}] users
                return None
            }
        }
