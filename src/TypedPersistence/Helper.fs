namespace TypedPersistence

open FSharp.Json

module Helper =
    type OptionBuilder() =
        member x.Bind(v, f) = Option.bind f v
        member x.Return v = Some v
        member x.ReturnFrom o = o
        member x.Zero() = None

    let opt = OptionBuilder()

    let deserializeJson<'a> content =
        try
            Json.deserialize<'a> content |> Some
        with :? JsonDeserializationError -> None
