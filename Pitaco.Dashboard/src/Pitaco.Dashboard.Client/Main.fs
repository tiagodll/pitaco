module Pitaco.Dashboard.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client

open Pitaco.Shared.Model
open Pitaco.Dashboard.Client.DashboardService

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Homepage
    | [<EndPoint "/dashboard">] Dashboard
    | [<EndPoint "/signup">] SignUp

/// The Elmish application's model.
type Model =
    {
        page: Page
        error: string option
        dashboard: DashboardState
        auth: Auth.Model
    }
and DashboardState = 
    {
        website: Website
    }


let initModel =
    {
        page = Homepage
        error = None
        dashboard = {
            website = { url=""; title="" }
        }
        auth = Auth.init()
    }

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | GetUrls
    | Error of exn
    | ClearError
    | AuthMsg of Auth.Msg



let update remote message model =
    let onSignIn = function
        | Some _ -> Cmd.ofMsg GetUrls
        | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | GetUrls -> model,Cmd.none


    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; auth={ model.auth with signIn={ model.auth.signIn with signedInAs = None }}}, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

    | AuthMsg msg' ->
        match msg' with
        | Auth.Msg.RecvSignUp x ->
            match x with
            | None -> model, Cmd.ofMsg (SetPage Dashboard)
            | Some e -> 
                let res', cmd' = Auth.update remote msg' model.auth
                { model with auth = res' }, Cmd.map AuthMsg cmd'
                //model, Cmd.ofMsg (SetPage Dashboard) // todo: not redirect when return error
        | _ -> 
            let res', cmd' = Auth.update remote msg' model.auth
            { model with auth = res' }, Cmd.map AuthMsg cmd'

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
        h3 [] [ text <| "Likes <| " + model.auth.signIn.username]
        //button [on.click (fun _ -> dispatch <| GetLikes model.signedInAs.Value)] [text "Reload"]
        button [on.click (fun _ -> dispatch (AuthMsg (Auth.SendSignOut)))] [text "Sign out"]
        span[] [text model.dashboard.website.url]
        span[] [text model.dashboard.website.title]
        //ul[attr.classes ["like-list"] ] [
        //    forEach model.pageState.dashboard.website <| fun l ->
        //    li [][
        //        span[][text <| l]
        //        span[][text " - "]
        //        a[attr.href l.url][text l.url]
        //    ]
        //]
    ]


let view model dispatch =
    section [] [
        // BODY
        cond model.page <| function
        | Dashboard -> 
            cond model.auth.signIn.signedInAs <| function
                | Some _ -> dashboardPage model dispatch
                | None -> Auth.signInPage model.auth (fun x -> dispatch (AuthMsg x))
                
        | Homepage ->
            cond model.auth.signIn.signedInAs <| function
                | Some _ -> dashboardPage model dispatch
                | None -> homePage model dispatch
        
        | SignUp ->
            Auth.signUpPage model.auth (fun x -> dispatch (AuthMsg x))
        
        //notification
        div [attr.id "notification-area"] [
            match model.error with
            | None -> empty
            | Some err -> errorNotification err (fun _ -> dispatch ClearError)

            match model.auth.error with
            | None -> empty
            | Some err -> errorNotification err (fun _ -> dispatch (AuthMsg Auth.ClearError))

            match model.auth.signIn.signInFailed with
            | false -> empty
            | true -> errorNotification "Sign in failed. Use any username and the password \"password\"."  (fun _ -> dispatch (AuthMsg Auth.ClearError))
        ]
    ]

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let dashboardService = this.Remote<DashboardService>()
        let update = update dashboardService
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg (AuthMsg Auth.GetSignedInAs)) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
