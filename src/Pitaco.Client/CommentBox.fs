module Pitaco.Client.CommentBox

open Elmish
open Bolero.Remoting.Client
open Microsoft.JSInterop
open Bolero.Html

open Pitaco.Shared.Model
open Pitaco.Client.DashboardService

type Model = {
    wskey: string
    comments: Comment list
    draft: Comment
    error: string option
}

let init() = 
  {
    wskey = ""
    comments = []
    draft = {wskey=""; url=""; text=""; author=""}
    error = None
  }

type Msg =
    | CommentsLoaded of Comment list
    | SetCommentText of string
    | SetCommentAuthor of string
    | PostComment
    | CommentAdded of string option
    | Error of exn

let loadComments url remote =
    Cmd.OfAsync.either remote.getComments (url) CommentsLoaded Error
let postComment remote cmt =
    Cmd.OfAsync.either remote.addComment cmt CommentAdded Error

let update (js:IJSRuntime) remote message model =
    match message with
    | SetCommentText text ->
        {model with draft={model.draft with text=text}}, Cmd.none
    | SetCommentAuthor text ->
        {model with draft={model.draft with author=text}}, Cmd.none

    | CommentsLoaded comments ->
        {model with comments = comments}, Cmd.none

    | PostComment ->
        let validateText x =
            match model.draft.text with
            | "" -> {model with error = Some "Post cannot be empty"}, Cmd.none
            | _ -> x
        let validateAuthor x =
            match model.draft.author with
            | "" -> {model with error = Some "Author cannot be empty"}, Cmd.none
            | _ -> x
        
        (model, postComment remote {model.draft with wskey=model.wskey})
        |> validateText
        |> validateAuthor

    | CommentAdded err ->
        match err with
        | None -> 
            let comments' = List.append model.comments [model.draft]
            {model with comments=comments'; draft={wskey=""; url=""; text=""; author=""}}, Cmd.none
        | Some s ->
            {model with error = Some s}, Cmd.none

    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none



let commentsPage (model:Model) dispatch =
    div [] [
        h2 [] [text model.wskey]
        ul [] [
            forEach model.comments <| fun c ->
                li [] [text <| c.text + " - " + c.author]
        ]
        form [on.submit (fun _ -> dispatch PostComment)] [
            textarea [attr.``class`` "textarea"; bind.input.string model.draft.text (dispatch << SetCommentText)] []
            span [] [text "author:"]
            input [attr.``type`` "text"; bind.input.string model.draft.author (dispatch << SetCommentAuthor)]
            br[]
            button [attr.``type`` "submit"] [text "post comment"]
        ]
    ]