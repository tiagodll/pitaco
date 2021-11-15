module Pitaco.Client.Dashboard

open System.Security.AccessControl
open Elmish
open Bolero.Remoting.Client
open Microsoft.JSInterop
open Bolero.Html

open Pitaco.Shared.Model
open Pitaco.Client.DashboardService

type Model = {
    website: Website
    pages: WsPage list
    error: string option
}
and Page = {
    url: string
    comments: Comment list
}

let init() = {
    website = { key=""; url=""; title="" }
    pages = []
    error = None
}

type Msg =
    | PagesLoaded of WsPage list
    | Error of exn

let loadPages key remote =
    Cmd.OfAsync.either remote.getPagesWithComments (key) PagesLoaded Error

let update (js:IJSRuntime) remote message model =
    match message with
    | PagesLoaded pages ->
        {model with pages = pages}, Cmd.none
        
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none



let dashboardPage model (user:Website) dispatch =
    div [attr.classes ["likes-list"]] [
        span [] [text model.website.url]
        span [] [text model.website.title]
        forEach model.pages <| fun page ->
            li [attr.classes ["link"]] [
                span [attr.``style`` "font-weight: bold"] [text page.url]
                forEach page.comments <| fun comment ->
                    div [] [text <| comment.text + " - " + comment.author]
            ]
    ]