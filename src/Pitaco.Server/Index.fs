module Pitaco.Server.Index

open Bolero
open Bolero.Html
open Bolero.Server.Html
open Pitaco

let page = doctypeHtml [] [
    head [] [
        meta [attr.charset "UTF-8"]
        meta [attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0"]
        title [] [text "Pitaco"]
        ``base`` [attr.href "/"]
        link [attr.rel "stylesheet"; attr.href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.4/css/bulma.min.css"]
        link [attr.rel "stylesheet"; attr.href "https://cdn.jsdelivr.net/npm/@mdi/font@6.5.95/css/materialdesignicons.min.css"]
        link [attr.rel "stylesheet"; attr.href "css/pitaco.css"]
        script [attr.src "js/interop.js"] []
    ]
    body [] [
        div [attr.id "main"] [rootComp<Client.Main.MyApp>]
        boleroScript
    ]
]
