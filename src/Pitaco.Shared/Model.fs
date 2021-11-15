namespace Pitaco.Shared

module Model =

    type Website = {
        key: string
        url: string
        title: string
    }

    and Comment = {
        wskey: string
        url: string
        text: string
        author: string
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
   
   