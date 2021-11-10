module Pitaco.Dashboard.Client.DashboardService

open Pitaco.Shared.Model
open Bolero.Remoting

/// Remote service definition.
type DashboardService =
    {
        getWebsite: unit -> Async<Website>
        // addBook: Website -> Async<unit>
        // removeBookByIsbn: string -> Async<unit>
        signIn : string * string -> Async<option<string>>
        getUsername : unit -> Async<string>
        signOut : unit -> Async<unit>
        signUp : SignUpRequest -> Async<string option>
    }

    interface IRemoteService with
        member this.BasePath = "/api"