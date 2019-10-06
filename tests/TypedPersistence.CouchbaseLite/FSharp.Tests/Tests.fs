namespace TypedPersistence.CouchbaseLite.FSharp.Tests

open Couchbase.Lite
open FsCheck
open FsCheck.Xunit
open TypedPersistence.CouchbaseLite.FSharp

module ``Saving and loading tests`` =
    type GenericRecord<'a> = { value: 'a }

    let documentName = "testdoc"

    let openDatabase () =
        new Database("testdb")

    let cleanUpDatabase () =
        use db = openDatabase ()
        db.Delete()

    let genericTest<'a when 'a : equality> (data: 'a) (compare: 'a -> bool) =
        let dataRecord = { GenericRecord.value = data }

        cleanUpDatabase ()

        use db = openDatabase ()
        saveDocument db documentName dataRecord
        db.Close ()

        use db = openDatabase ()
        let result = loadDocument<GenericRecord<'a>> db documentName
        db.Close()

        match result with
        | Ok record ->
            record.value = data
        | Error error ->
            printfn "Error: %A" error
            false

    [<Property>]
    let ``Handles strings correctly`` (text: NonNull<string>) =
        let text = text.Get

        genericTest<string> text

    [<Property>]
    let ``Handles ints correctly`` (number: int) =
        genericTest<int> number

    [<Property>]
    let ``Handles option strings correctly`` (text: Option<NonNull<string>>) =
        let text =
            match text with
            | Some text -> Some text.Get
            | None -> None

        genericTest<string option> text

    [<Property>]
    let ``Handles option ints correctly`` (number: Option<int>) =
        genericTest<int option> number

    [<Property>]
    let ``Handles int record correctly`` (record: GenericRecord<int>) =
        genericTest<GenericRecord<int>> record
