namespace Pitaco.Dashboard.Server

open System
open Microsoft.AspNetCore.Hosting
open Bolero.Remoting
open Bolero.Remoting.Server

open Pitaco.Dashboard
open Pitaco.Shared.Model

type DbUser = { 
    url: string
    title: string
    password: string 
}
//type DbComment = {
//    url: string
//    comment: string
//    author: string
//}

module DashboardServiceHelper =
    let DbUserToWebsite user = {title=user.title; url=user.url}

type DashboardService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.DashboardService.DashboardService>()

    let mutable users = [{url="tiago.dalligna.com"; title="cyborg"; password="asd"}]
    let mutable comments = [{url="tiago"; text="this is the best website ever!!!"; author="honest person"}]

    override this.Handler =
        {
            getWebsite = ctx.Authorize <| fun () -> async {
                match List.tryFind (fun x -> x.url = ctx.HttpContext.User.Identity.Name) users with
                | None -> return None
                | Some x -> return Some <| DashboardServiceHelper.DbUserToWebsite x
            }

            signIn = fun (username, password) -> async {
                match List.tryFind (fun x -> x.url=username && x.password=password) users with
                    | None -> 
                        return None
                    | Some x -> 
                        do! ctx.HttpContext.AsyncSignIn(username, TimeSpan.FromDays(365.))
                        return Some {url=x.url; title=x.title}
            }

            signOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            signUp = fun (signUpRequest) -> async {
                users <- List.append [{ url=signUpRequest.url; title=signUpRequest.title; password=signUpRequest.password}] users
                return None
            }

            addComment = fun (comment) -> async {
                comments <- List.append [{ url=comment.url; text=comment.text; author=comment.author}] comments
                return None
            }

            getComments = fun (url) -> async {
                return comments
                |> List.filter (fun x -> x.url = url)
            }
        }
