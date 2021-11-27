namespace Pitaco.Server

open System
open Microsoft.AspNetCore.Hosting
open Bolero.Remoting
open Bolero.Remoting.Server

open Pitaco
open Pitaco.Database
open Pitaco.Database.Types
open Pitaco.Shared.Model

type DashboardService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.DashboardService.DashboardService>()

    override this.Handler =
        {
            getWebsite = ctx.Authorize <| fun () -> async {
                return Queries.Website.ById ctx.HttpContext.User.Identity.Name
            }

            signIn = fun (url, password) -> async {
                match Queries.Website.ByUrlAndPassword url password with
                | None ->
                    return None
                | Some u ->
                    do! ctx.HttpContext.AsyncSignIn(u.key, TimeSpan.FromDays(365.))
                    return Some u
            }

            signOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            signUp = fun (signUpRequest) -> async {
                let makeKey url =
                    String.filter (fun x -> x <> '.' && x <> '/') url
                    
//                if signUpRequest.password <> signUpRequest.password2 then
//                    return (Some "Passwords dont match")
                
                Queries.Website.Add {
                    id = makeKey(signUpRequest.url)
                    url = signUpRequest.url
                    title = signUpRequest.title
                    password = signUpRequest.password
                    timestamp = DateTime.MinValue
                }
                
                return None
            }
            

            addComment = fun (p:addCommentParam) -> async {
                let cmt = {
                          id = Guid.NewGuid().ToString()
                          wsId = p.wskey
                          url = p.url
                          authorId = p.author // TODO: generate authorid and set a cookie to allow users to delete comments
                          author = p.author
                          text = p.text
                          timestamp = DateTime.MinValue
                      }
                return match Queries.Comment.Add cmt with
                        | None -> None
                        | Some ex -> Some ex.Message
            }

            getComments = fun (url) -> async {
                return Queries.Comment.ByUrl url
                        |> Array.toList
            }
            
            getPagesWithComments = fun (key) -> async {
                let ws = Queries.Website.ById key
                let urls = Queries.Comment.UrlsByWebsite key
                
                return urls
                    |> Array.map (fun url -> {
                            wskey = key
                            url = url
                            comments = Queries.Comment.ByUrl url |> Array.toList
                        })
                    |> Array.toList
            }

            ping = fun () -> async {
                Pitaco.Database.Migrations.migrate |> ignore
                return "pong"
            }
        }
