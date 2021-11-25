namespace Pitaco.Shared

open System

module Model =

    type Website = {
        key: string
        url: string
        title: string
    }

    and Comment = {
        key:string
        wskey: string
        url: string
        text: string
        author: string
        timestamp: DateTime
    }
    and WsPage = {
        wskey: string
        url: string
        comments: Comment list
    }

    and SignUpRequest = {
        title: string
        url: string
        password: string
        password2: string
        signUpDone: bool
    }
    
    and SignInResponse = {
        website: Website
        comments: Comment list
    }
   
    and addCommentParam = {
        wskey: string
        url: string
        author: string
        text: string
    }
