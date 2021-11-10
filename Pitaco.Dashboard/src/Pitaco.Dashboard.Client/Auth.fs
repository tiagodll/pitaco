﻿module Pitaco.Dashboard.Client.Auth

open Elmish
open Bolero.Remoting
open Bolero.Remoting.Client
open Pitaco.Dashboard.Client.DashboardService
open Bolero
open Bolero.Html
open Pitaco.Shared.Model

type Model = {
    signIn: SignInModel
    signUp: SignUpRequest
    error: string option
}
and SignInModel = {
    username: string
    password: string
    signedInAs: option<string>
    signInFailed: bool
}

let blankSignIn = {
    username = ""
    password = ""
    signedInAs = None
    signInFailed = false
}
let blankSignUp = {
    title = ""
    url = ""
    password = ""
    password2 = ""
}

let init() = 
  {
    signIn = blankSignIn
    signUp = blankSignUp
    error = None
  }

type Msg =
    | SetUsername of string
    | SetPassword of string
    | SetSignUpTitle of string
    | SetSignUpUrl of string
    | SetSignUpPassword of string
    | SetSignUpPassword2 of string
    | GetSignedInAs
    | RecvSignedInAs of string option
    | SendSignIn
    | RecvSignIn of string option
    | SendSignUp
    | RecvSignUp of string option
    | SendSignOut
    | RecvSignOut
    | Error of exn
    | ClearError

let CreateId (n: string) =
  let rand = System.Random()
  let r = rand.NextDouble().ToString().[2..4]
  n.Split(" ")
  |> Array.map (fun s -> s.[0..1])
  |> (fun a -> Array.append a [| r |])
  |> String.concat ""

let update remote message model =
    match message with
    | SetUsername s ->
        { model with signIn={ model.signIn with username = s }}, Cmd.none
    | SetPassword s ->
        { model with signIn={ model.signIn with password = s }}, Cmd.none
    
    | SetSignUpTitle s ->
        { model with signUp={ model.signUp with title=s}}, Cmd.none
    | SetSignUpUrl s ->
        { model with signUp={ model.signUp with url = s}}, Cmd.none
    | SetSignUpPassword s ->
        { model with signUp={ model.signUp with password = s}}, Cmd.none
    | SetSignUpPassword2 s ->
        { model with signUp={ model.signUp with password2 = s}}, Cmd.none

    | GetSignedInAs ->
        model, Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with signIn={ model.signIn with signedInAs = username }}, Cmd.none // onSignIn username
    
    | SendSignIn ->
        model, Cmd.OfAsync.either remote.signIn (model.signIn.username, model.signIn.password) RecvSignIn Error
    | RecvSignIn username ->
        { model with signIn={ model.signIn with signedInAs = username; signInFailed = Option.isNone username }}, Cmd.none // onSignIn username
    | SendSignOut ->
        model, Cmd.OfAsync.either remote.signOut () (fun () -> RecvSignOut) Error
    | RecvSignOut ->
        { model with signIn={ model.signIn with signedInAs = None; signInFailed = false }}, Cmd.none

    | SendSignUp ->
        model, Cmd.OfAsync.either remote.signUp model.signUp RecvSignUp Error
    | RecvSignUp error ->
        { model with error = error }, Cmd.none // onSignIn username

    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; signIn={ model.signIn with signedInAs = None }}, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none




let signInPage model dispatch =
    div[][
        h1 [attr.classes ["title"]] [text "Sign in"]
        form [on.submit (fun _ -> dispatch SendSignIn)] [
            div [attr.classes ["field"]] [
                label [attr.classes ["label"]] [text "URL"]
                input [attr.classes ["input"]; bind.input.string model.signIn.username (fun s -> dispatch (SetUsername s))]
            ]
            div [attr.classes ["field"]] [
                label [attr.classes ["label"]] [text "Password"]
                input [attr.classes ["input"]; attr.``type`` "password"; bind.input.string model.signIn.password (fun s -> dispatch (SetPassword s))]
            ]
            div [attr.classes ["field"]] [
                input [attr.classes ["input"]; attr.``type`` "submit"; attr.value "Sign in"]
            ]
        ]
    ]


let signUpPage model dispatch =
    div[][
        h1 [attr.classes ["title"]] [text "Sign in"]
        form [on.submit (fun _ -> dispatch SendSignUp)] [
            div [attr.classes ["field"]] [
                label [attr.classes ["label"]] [text "Website title"]
                input [attr.classes ["input"]; bind.input.string model.signUp.title (dispatch << SetSignUpTitle)]
            ]
            div [attr.classes ["field"]] [
                label [attr.classes ["label"]] [text "Website Url"]
                input [attr.classes ["input"]; bind.input.string model.signUp.url (dispatch << SetSignUpUrl)]
            ]
            div [attr.classes ["field"]] [
                label [attr.classes ["label"]] [text "Password"]
                input [attr.classes ["input"]; attr.``type`` "password"; bind.input.string model.signUp.password (dispatch << SetSignUpPassword)]
            ]
            div [attr.classes ["field"]] [
                label [attr.classes ["label"]] [text "Repeat Password"]
                input [attr.classes ["input"]; attr.``type`` "password"; bind.input.string model.signUp.password2 (dispatch << SetSignUpPassword2)]
            ]
            div [attr.classes ["field"]] [
                input [attr.classes ["input"]; attr.``type`` "submit"; attr.value "Sign up"]
            ]
        ]
    ]