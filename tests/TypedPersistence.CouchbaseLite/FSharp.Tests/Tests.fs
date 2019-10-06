namespace TypedPersistence.CouchbaseLite.FSharp.Tests

open Couchbase.Lite
open FsCheck
open FsCheck.Xunit
open TypedPersistence.CouchbaseLite.FSharp

module ``Saving and loading tests`` =
    type GenericRecord<'a> = { value: 'a }
    type GenericRecord2<'a, 'b> = { value1: 'a; value2: 'b }

    let documentName = "testdoc"

    let openDatabase () =
        new Database("testdb")

    let cleanUpDatabase () =
        use db = openDatabase ()
        db.Delete()

    let genericTest<'a when 'a : equality> (data: 'a) =
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
            if record.value = data then
                true
            else
                printfn "Expected: %A - Got: %A" data record.value
                false
        | Error error ->
            printfn "Error: %A" error
            false

    [<Property>]
    let ``Handles ints correctly`` (number: int) =
        genericTest<int> number

    [<Property>]
    let ``Handles strings correctly`` (text: NonNull<string>) =
        let text = text.Get

        genericTest<string> text

    [<Property>]
    let ``Handles option strings correctly`` (textOption: Option<NonNull<string>>) =
        let text =
            match textOption with
            | Some text -> Some text.Get
            | None -> None

        genericTest<string option> text

    [<Property>]
    let ``Handles option ints correctly`` (numberOption: Option<int>) =
        genericTest<int option> numberOption

    // Does not work for now... :(
    [<Property>]
    let ``Handles int list correctly`` (numberList: int list) =
        genericTest<int list> numberList

    // Does not work for now... :(
    [<Property>]
    let ``Handles string list correctly`` (textList: NonNull<string> list) =
        textList
        |> List.map (fun x -> x.Get)
        |> genericTest<string list>

    [<Property>]
    let ``Handles int record correctly`` (numberRecord: GenericRecord<int>) =
        genericTest<GenericRecord<int>> numberRecord

    [<Property>]
    let ``Handles string record correctly`` (textRecord: GenericRecord<NonNull<string>>) =
        { value = textRecord.value.Get }
        |> genericTest<GenericRecord<string>>

    [<Property>]
    let ``Handles int option, string record2 correctly`` (record: GenericRecord2<int option, NonNull<string>>) =
        let record = { value1 = record.value1; value2 = record.value2.Get }

        genericTest<GenericRecord2<int option, string>> record
