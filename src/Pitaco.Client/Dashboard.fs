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
//and WsPage = {
//    url: string
//    comments: Comment list
//}

let init() = {
    website = { key=""; url=""; title="" }
    pages = []
    error = None
}

type Msg =
    | PagesLoaded of WsPage list
    | DeleteComment of string
    | CommentDeleted of string
    | Error of exn

let loadPages remote key =
    Cmd.OfAsync.either remote.getPagesWithComments (key) PagesLoaded Error

let deleteComment remote key =
    Cmd.OfAsync.either remote.deleteComment key CommentDeleted Error

let update (js:IJSRuntime) remote message model =
    match message with
    | PagesLoaded pages ->
        {model with pages = pages}, Cmd.none
        
    | DeleteComment key ->
        model, deleteComment remote key
        
    | CommentDeleted key ->
        let mapPages (page:WsPage) = {page with comments = List.filter (fun c -> c.key <> key) page.comments}
        {model with pages = (List.map mapPages model.pages)}, Cmd.none
        
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
                   div [] [
                       span [] [text <| " - " + comment.text + " - " + comment.author]
                       i [attr.classes ["clickable icon mdi mdi-delete"]; attr.style[""]; on.click (fun _ -> dispatch <| DeleteComment comment.key)] []
                   ]
            ]
    ]