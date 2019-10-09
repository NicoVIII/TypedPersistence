namespace TypedPersistence.CouchbaseLite.FSharp.Tests

open Couchbase.Lite
open FsCheck
open FsCheck.Xunit
open FsUnit
open TypedPersistence.CouchbaseLite.FSharp
open Xunit

module ``Saving and loading tests`` =
    type GenericRecord<'a> = { value: 'a }
    type GenericRecord2<'a, 'b> = { value1: 'a; value2: 'b }

    let documentName = "testdoc"

    let openDatabase () =
        new Database("testdb")

    let cleanUpDatabase () =
        use db = openDatabase ()
        db.Delete()

    let saveToDatabase data =
        use db = openDatabase ()
        saveDocument db documentName data
        db.Close ()

    let loadFromDatabase<'a> () =
        use db = openDatabase ()
        let result = loadDocument<'a> db documentName
        db.Close()
        result

    let genericPropertyTest<'a when 'a : equality> setUp (data: 'a) =
        let dataRecord = { GenericRecord.value = data }

        cleanUpDatabase ()

        setUp dataRecord

        saveToDatabase dataRecord

        let result = loadFromDatabase<GenericRecord<'a>> ()

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

    let simplePropertyTest<'a when 'a : equality> (data: 'a) =
        genericPropertyTest ignore data

    (*[<Fact>]
    let ``Fails to load unexisting document`` () =
        cleanUpDatabase ()

        use db = openDatabase ()
        let result = loadDocument<int> db documentName
        db.Close()

        result |> should equal (Error DocumentNotExisting)

    [<Fact>]
    let ``Fails to load unexisting property`` () =
        cleanUpDatabase ()

        use db = openDatabase ()
        saveDocument db documentName { value = 0 }
        db.Close ()

        use db = openDatabase ()
        let result = loadDocument<GenericRecord<int>> db documentName
        db.Close()

        let error = ValueNotExisting "value" |> Error
        result |> should equal error*)

    [<Property>]
    let ``Handles ints correctly`` (number: int) =
        simplePropertyTest<int> number

    [<Property>]
    let ``Handles double saving correctly`` (number: int) =
        genericPropertyTest<int> saveToDatabase number

    [<Property>]
    let ``Handles strings correctly`` (text: NonNull<string>) =
        let text = text.Get

        simplePropertyTest<string> text

    [<Property>]
    let ``Handles option strings correctly`` (textOption: Option<NonNull<string>>) =
        let text =
            match textOption with
            | Some text -> Some text.Get
            | None -> None

        simplePropertyTest<string option> text

    [<Property>]
    let ``Handles option ints correctly`` (numberOption: Option<int>) =
        simplePropertyTest<int option> numberOption

    [<Property>]
    let ``Handles int list correctly`` (numberList: int list) =
        simplePropertyTest<int list> numberList

    [<Property>]
    let ``Handles string list correctly`` (textList: NonNull<string> list) =
        textList
        |> List.map (fun x -> x.Get)
        |> simplePropertyTest<string list>

    [<Property>]
    let ``Handles int record correctly`` (numberRecord: GenericRecord<int>) =
        simplePropertyTest<GenericRecord<int>> numberRecord

    [<Property>]
    let ``Handles string record correctly`` (textRecord: GenericRecord<NonNull<string>>) =
        { value = textRecord.value.Get }
        |> simplePropertyTest<GenericRecord<string>>

    [<Property>]
    let ``Handles int option, string record2 correctly`` (record: GenericRecord2<int option, NonNull<string>>) =
        let record = { value1 = record.value1; value2 = record.value2.Get }

        simplePropertyTest<GenericRecord2<int option, string>> record

    [<Property>]
    let ``Handles int record list correctly`` (numberRecordList: GenericRecord<int> list) =
        simplePropertyTest<GenericRecord<int> list> numberRecordList
