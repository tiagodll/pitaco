namespace Pitaco.Server

open System
open Microsoft.AspNetCore.Hosting
open Bolero.Remoting
open Bolero.Remoting.Server

open Pitaco
open Pitaco.Shared.Model

type DbUser = {
    key: string
    url: string
    title: string
    password: string 
}

module DashboardServiceHelper =
    let DbUserToWebsite user = {key=user.key; title=user.title; url=user.url}

type DashboardService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.DashboardService.DashboardService>()

    let mutable users = [{key="tiagodallignacom"; url="tiago.dalligna.com"; title="cyborg"; password="asd"}]
    let mutable comments = [{wskey="tiagodallignacom"; text="this is the best website ever!!!"; author="honest person"}]

    override this.Handler =
        {
            getWebsite = ctx.Authorize <| fun () -> async {
                let matchByKey x =
                    x.key = ctx.HttpContext.User.Identity.Name
                
                match List.tryFind matchByKey users with
                | None -> return None
                | Some x -> return Some <| DashboardServiceHelper.DbUserToWebsite x
            }

            signIn = fun (username, password) -> async {
                let verifyEmailAndPassword x =
                    x.url=username && x.password=password

                match List.tryFind verifyEmailAndPassword users with
                | None -> 
                    return None
                | Some x -> 
                    do! ctx.HttpContext.AsyncSignIn(x.key, TimeSpan.FromDays(365.))
                    return Some {key=x.key; url=x.url; title=x.title}
            }

            signOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            signUp = fun (signUpRequest) -> async {
//                let key = String. signUpRequest.url
                let makeKey url =
                    String.filter (fun x -> x <> '.' && x <> '/') url
                    
                users <- List.append [{ key=makeKey(signUpRequest.url); url=signUpRequest.url; title=signUpRequest.title; password=signUpRequest.password}] users
                return None
            }

            addComment = fun (comment) -> async {
                comments <- List.append [{ wskey=comment.wskey; text=comment.text; author=comment.author}] comments
                return None
            }

            getComments = fun (id) -> async {
                return comments
                |> List.filter (fun x -> x.wskey = id)
            }
        }
