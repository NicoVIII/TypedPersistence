namespace TypedPersistence.Json

open FSharp.Json

module Helper =
    let deserializeJson<'a> content =
        try
            Json.deserialize<'a> content |> Some
        with :? JsonDeserializationError ->
            None
