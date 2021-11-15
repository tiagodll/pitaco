module Pitaco.Client.DashboardService

open Pitaco.Shared.Model
open Bolero.Remoting

type DashboardService =
    {
        getWebsite: unit -> Async<Website option>
        signIn : string * string -> Async<Website option>
        signOut : unit -> Async<unit>
        signUp : SignUpRequest -> Async<string option>
        getComments: string -> Async<Comment list>
        addComment: Comment -> Async<string option>
        getPagesWithComments: string -> Async<WsPage list>
    }

    interface IRemoteService with
        member this.BasePath = "/api"