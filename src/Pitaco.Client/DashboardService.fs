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
        addComment: addCommentParam -> Async<string option>
        deleteComment: string -> Async<string>
        getPagesWithComments: string -> Async<WsPage list>
        ping: unit -> Async<string>
    }

    interface IRemoteService with
        member this.BasePath = "/api"