namespace Pitaco.Shared

module Model =

    type Website = {
        url: string
        title: string
    }

    and Comment = {
        url: string
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