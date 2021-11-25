namespace Pitaco.Database.Types

open System

type DbWebsite = {
    id: string
    url: string
    title: string
    password: string
    timestamp: DateTime
}
    
type DbComment = {
    id: string
    wsId: string
    url: string
    authorId: string
    author: string
    text: string
    timestamp: DateTime
}