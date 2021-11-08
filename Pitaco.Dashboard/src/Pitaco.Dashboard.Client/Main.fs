module Pitaco.Dashboard.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Homepage
    | [<EndPoint "/dashboard">] Dashboard
    | [<EndPoint "/signup">] SignUp

/// The Elmish application's model.
type Model =
    {
        page: Page
        pageState: PageState
        error: string option
    }

and PageState =
    {
        signIn: SigninState
        signUp: SignupState option
        dashboard: DashboardState
    }
and SigninState =
    {
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
    }

and SignupState =
    {
        title: string
        url: string
        password: string
        password2: string
    }
and DashboardState = 
    {
        website: Website
    }
and Website = 
    {
        url: string
        title: string
    }

let initModel =
    {
        page = Homepage
        error = None
        pageState = {
            signIn = {
                username = ""
                password = ""
                signedInAs = None
                signInFailed = false
            }
            signUp = None
            dashboard = {
                website = { url=""; title="" }
            }
        }
    }

/// Remote service definition.
type DashboardService =
    {
        getWebsite: unit -> Async<Website>
        //addBook: Website -> Async<unit>
        //removeBookByIsbn: string -> Async<unit>
        signIn : string * string -> Async<option<string>>
        getUsername : unit -> Async<string>
        signOut : unit -> Async<unit>
        signUp : string * string * string -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/api"

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | SetUsername of string
    | SetPassword of string
    | SetSignUpTitle of string
    | SetSignUpUrl of string
    | SetSignUpPassword of string
    | SetSignUpPassword2 of string
    | GetSignedInAs
    | GetUrls
    | RecvSignedInAs of option<string>
    | SendSignIn
    | RecvSignIn of option<string>
    | SendSignOut
    | RecvSignOut
    | Error of exn
    | ClearError


let signUpOrNew (signup:SignupState option) =
    match signup with
    | Some x -> x
    | None -> {
            title = ""
            url = ""
            password = ""
            password2 = ""
        }


let update remote message model =
    let onSignIn = function
        | Some _ -> Cmd.ofMsg GetUrls
        | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | SetUsername s ->
        { model with pageState={ model.pageState with signIn={ model.pageState.signIn with username = s }}}, Cmd.none
    | SetPassword s ->
        { model with pageState={ model.pageState with signIn={ model.pageState.signIn with password = s }}}, Cmd.none
    | SetSignUpTitle s ->
        let signUp' = {signUpOrNew model.pageState.signUp with title = s}
        { model with pageState={ model.pageState with signUp=Some signUp'}}, Cmd.none
    | SetSignUpUrl s ->
        let signUp' = {signUpOrNew model.pageState.signUp with url = s}
        { model with pageState={ model.pageState with signUp=Some signUp'}}, Cmd.none
    | SetSignUpPassword s ->
        let signUp' = {signUpOrNew model.pageState.signUp with password = s}
        { model with pageState={ model.pageState with signUp=Some signUp'}}, Cmd.none
    | SetSignUpPassword2 s ->
        let signUp' = {signUpOrNew model.pageState.signUp with password2 = s}
        { model with pageState={ model.pageState with signUp=Some signUp'}}, Cmd.none

    | GetSignedInAs ->
        model, Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with pageState={ model.pageState with signIn={ model.pageState.signIn with signedInAs = username }}}, onSignIn username
    | SendSignIn ->
        model, Cmd.OfAsync.either remote.signIn (model.pageState.signIn.username, model.pageState.signIn.password) RecvSignIn Error
    | RecvSignIn username ->
        { model with pageState={ model.pageState with signIn={ model.pageState.signIn with signedInAs = username; signInFailed = Option.isNone username }}}, onSignIn username
    | SendSignOut ->
        model, Cmd.OfAsync.either remote.signOut () (fun () -> RecvSignOut) Error
    | RecvSignOut ->
        { model with pageState={ model.pageState with signIn={ model.pageState.signIn with signedInAs = None; signInFailed = false }}}, Cmd.none
    | GetUrls -> model,Cmd.none

    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; pageState={ model.pageState with signIn={ model.pageState.signIn with signedInAs = None }}}, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// TODO: extract router
let router = Router.infer SetPage (fun model -> model.page)

// TODO: extract view
let errorNotification err clear =
    div [attr.classes ["notification"; "is-warning"]] [
        button [attr.classes ["delete"]; on.click clear ] []
        span [] [text err]
    ]

let homePage model dispatch =
    div[attr.classes ["homepage"]] [
        h3 [] [text "why to use this app?"]
        ul[attr.classes ["like-list"] ] [
            span[][text "good question? <br> should we have a markdown parser here?"]
        ]
        a [on.click (fun _ -> dispatch <| SetPage Dashboard)] [text "Sign in"]
        br[]
        span[][text "not a member yet?"]
        a [on.click (fun _ -> dispatch <| SetPage SignUp)] [text "Sign up"]
    ]

let dashboardPage model dispatch =
    div[attr.classes ["likes-list"]] [
        h3 [] [text <| "Likes <| " + model.pageState.signIn.username]
        //button [on.click (fun _ -> dispatch <| GetLikes model.signedInAs.Value)] [text "Reload"]
        button [on.click (fun _ -> dispatch SendSignOut)] [text "Sign out"]
        span[][text model.pageState.dashboard.website.url]
        span[][text model.pageState.dashboard.website.title]
        //ul[attr.classes ["like-list"] ] [
        //    forEach model.pageState.dashboard.website <| fun l ->
        //    li [][
        //        span[][text <| l]
        //        span[][text " - "]
        //        a[attr.href l.url][text l.url]
        //    ]
        //]
    ]

let signInPage model dispatch =
    div[][
        h1 [attr.classes ["title"]] [text "Sign in"]
        form [on.submit (fun _ -> dispatch SendSignIn)][
            div [attr.classes ["field"]][
                label [attr.classes ["label"]] [text "Username"]
                input [attr.classes ["input"]; bind.input.string model.pageState.signIn.username (dispatch << SetUsername)]
            ]
            div [attr.classes ["field"]][
                label [attr.classes ["label"]] [text "Password"]
                input [attr.classes ["input"]; attr.``type`` "password"; bind.input.string model.pageState.signIn.password (dispatch << SetPassword)]
            ]
            div [attr.classes ["field"]][
                input [attr.classes ["input"]; attr.``type`` "submit"; attr.value "Sign in"]
            ]
        ]
    ]

let signUpPage model (signup:SignupState) dispatch =
    div[][
        h1 [attr.classes ["title"]] [text "Sign in"]
        form [on.submit (fun _ -> dispatch SendSignIn)][
            div [attr.classes ["field"]][
                label [attr.classes ["label"]] [text "Website title"]
                input [attr.classes ["input"]; bind.input.string signup.title (dispatch << SetSignUpTitle)]
            ]
            div [attr.classes ["field"]][
                label [attr.classes ["label"]] [text "Website Url"]
                input [attr.classes ["input"]; bind.input.string signup.url (dispatch << SetSignUpUrl)]
            ]
            div [attr.classes ["field"]][
                label [attr.classes ["label"]] [text "Password"]
                input [attr.classes ["input"]; attr.``type`` "password"; bind.input.string signup.password (dispatch << SetSignUpPassword)]
            ]
            div [attr.classes ["field"]][
                label [attr.classes ["label"]] [text "Repeat Password"]
                input [attr.classes ["input"]; attr.``type`` "password"; bind.input.string signup.password (dispatch << SetSignUpPassword2)]
            ]
            div [attr.classes ["field"]][
                input [attr.classes ["input"]; attr.``type`` "submit"; attr.value "Sign up"]
            ]
        ]
    ]

let view model dispatch =
    section [] [
        // BODY
        cond model.page <| function
        | Dashboard -> 
            cond model.pageState.signIn.signedInAs <| function
                | Some _ -> dashboardPage model dispatch
                | None -> signInPage model dispatch
                
        | Homepage ->
            cond model.pageState.signIn.signedInAs <| function
                | Some _ -> dashboardPage model dispatch
                | None -> homePage model dispatch
        
        | SignUp ->
            let signup = signUpOrNew model.pageState.signUp
            signUpPage model signup dispatch
        
        //notification
        div [attr.id "notification-area"] [
            match model.error with
            | None -> empty
            | Some err -> errorNotification err (fun _ -> dispatch ClearError)

            match model.pageState.signIn.signInFailed with
            | false -> empty
            | true -> errorNotification "Sign in failed. Use any username and the password \"password\"."  (fun _ -> dispatch ClearError)
        ]
    ]

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let bookService = this.Remote<DashboardService>()
        let update = update bookService
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
