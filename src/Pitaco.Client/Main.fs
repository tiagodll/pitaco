module Pitaco.Client.Main

open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client

open Microsoft.JSInterop
open Pitaco.Shared.Model
open Pitaco.Client.DashboardService

type Page =
    | [<EndPoint "/">] Homepage
    | [<EndPoint "/dashboard">] Dashboard
    | [<EndPoint "/signup">] SignUp
    | [<EndPoint "/comments/{wskey}">] CommentsPage of wskey:string


type Model =
    {
        page: Page
        error: string option
        dashboard: DashboardState
        commentBox: CommentBox.Model
        auth: Auth.Model
    }
and DashboardState = {
    website: Website
}

let initModel =
    {
        page = Homepage
        error = None
        dashboard = {
            website = { key=""; url=""; title="" }
        }
        commentBox = CommentBox.init()
        auth = Auth.init()
    }

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Error of exn
    | ClearError
    | AuthMsg of Auth.Msg
    | CommentBoxMsg of CommentBox.Msg




let update (js:IJSRuntime) remote message model =
    match message with
    | SetPage page ->
        match page with
        | CommentsPage wskey -> { model with page=page; commentBox = {model.commentBox with wskey = wskey}}, Cmd.none // CommentBox.loadComments wskey remote
        | _ -> { model with page = page }, Cmd.none

    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    
    | ClearError ->
        { model with 
            error = None; 
            auth = {model.auth with error = None}; 
            commentBox = {model.commentBox with error = None}}, Cmd.none

    | AuthMsg msg' ->
        match msg' with
        | Auth.Msg.RecvSignUp x ->
            match x with
            | None -> model, Cmd.ofMsg (SetPage Dashboard)
            | Some e -> 
                let res', cmd' = Auth.update js remote msg' model.auth
                { model with auth = res' }, Cmd.map AuthMsg cmd'

        | _ -> 
            let res', cmd' = Auth.update js remote msg' model.auth
            { model with auth = res' }, Cmd.map AuthMsg cmd'
            
    | CommentBoxMsg msg' ->
        let res', cmd' = CommentBox.update js remote msg' model.commentBox
        { model with commentBox = res' }, Cmd.map CommentBoxMsg cmd'


let router = Router.infer SetPage (fun model -> model.page)

let errorNotification err clear =
    div [attr.classes ["notification"; "is-warning"]] [
        button [attr.classes ["delete"]; on.click clear ] []
        span [] [text err]
    ]

let homePage model dispatch =
    div [attr.classes ["homepage"]] [
        h3 [] [text "why to use this app?"]
        ul [attr.classes ["like-list"] ] [
            span [] [text "good question? <br> should we have a markdown parser here?"]
        ]
        a [on.click (fun _ -> dispatch <| SetPage Dashboard)] [text "Sign in"]
        br []
        span [] [text "not a member yet?"]
        a [on.click (fun _ -> dispatch <| SetPage SignUp)] [text "Sign up"]
    ]

let dashboardPage model (user:Website) dispatch =
    div [attr.classes ["likes-list"]] [
        h3 [] [ text <| user.title + " - " + user.url]
        button [on.click (fun _ -> dispatch (AuthMsg (Auth.SendSignOut)))] [text "Sign out"]
        span [] [text model.dashboard.website.url]
        span [] [text model.dashboard.website.title]

    ]

let header element =
    div [] [
        nav [attr.classes ["navbar"; "is-dark"]; "role" => "navigation"; attr.aria "label" "main navigation"] [
            div [attr.classes ["navbar-brand"]] [
                a [attr.classes ["navbar-item"; "has-text-weight-bold"; "is-size-5"]; attr.href "/"] [
//                            img [attr.style "height:40px"; attr.src "https://github.com/fsbolero/website/raw/master/src/Website/img/wasm-fsharp.png"]
                    text "  Pitaco"
                ]
            ]
        ]
        element
    ]


let view model dispatch =
    section [] [
        // BODY
        cond model.page <| function
        | Dashboard ->
            cond model.auth.signIn.signedInAs <| function
                | Some user -> header <| dashboardPage model user dispatch
                | None -> header <| Auth.signInPage model.auth (fun x -> dispatch (AuthMsg x))
                
        | Homepage ->
            cond model.auth.signIn.signedInAs <| function
                | Some user -> header <| dashboardPage model user dispatch
                | None -> homePage model dispatch
        
        | SignUp ->
            header <| Auth.signUpPage model.auth (fun x -> dispatch (AuthMsg x))

        | CommentsPage url ->
            CommentBox.commentsPage model.commentBox (fun x -> dispatch (CommentBoxMsg x))
        
        //notification
        div [attr.id "notification-area"] [
            match model.error with
            | None -> empty
            | Some err -> errorNotification err (fun _ -> dispatch ClearError)

            match model.auth.error with
            | None -> empty
            | Some err -> errorNotification err (fun _ -> dispatch ClearError)

            match model.commentBox.error with
            | None -> empty
            | Some err -> errorNotification err (fun _ -> dispatch ClearError)

            match model.auth.signIn.signInFailed with
            | false -> empty
            | true -> errorNotification "Sign in failed. Use any username and the password \"password\"."  (fun _ -> dispatch ClearError)
        ]
    ]

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let dashboardService = this.Remote<DashboardService>()
        let update = update this.JSRuntime dashboardService
        
        let init _ = 
            this.JSRuntime.InvokeVoidAsync("Log", {|ws="sign in"|}).AsTask() |> ignore
            initModel, Cmd.ofMsg (AuthMsg Auth.GetSignedInAs)
        
        Program.mkProgram init update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
