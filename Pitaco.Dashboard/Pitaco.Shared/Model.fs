namespace Pitaco.Shared

module Model =

    type Website = {
        key: string
        url: string
        title: string
    }

    and Comment = {
        wskey: string
        text: string
        author: string
    }

    and SignUpRequest = {
        title: string
        url: string
        password: string
        password2: string
        signUpDone: bool
    }