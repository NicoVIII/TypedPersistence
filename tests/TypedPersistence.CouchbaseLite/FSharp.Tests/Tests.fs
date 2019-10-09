namespace TypedPersistence.CouchbaseLite.FSharp.Tests

open FsCheck
open FsCheck.Xunit
open FsUnit
open TypedPersistence.CouchbaseLite.FSharp
open Xunit

module ``Saving and loading tests`` =
    let documentName = "testdoc"

    let saveToDatabase<'a> = saveToDatabase<'a> documentName
    let loadFromDatabase<'a> () = loadFromDatabase<'a> documentName
    let simplePropertyTest<'a when 'a : equality> = simplePropertyTest<'a> documentName
    let genericPropertyTest<'a when 'a : equality> = genericPropertyTest<'a> documentName

    [<Fact>]
    let ``Fails to load unexisting document`` () =
        cleanUpDatabase ()

        use db = openDatabase ()
        let result = loadFromDatabase<int> ()
        db.Close()

        result |> categorize
        |> should equal DocumentNotExistingErrorCase

    [<Fact>]
    let ``Fails to load unexisting property`` () =
        cleanUpDatabase ()

        saveToDatabase { value1 = 0 }
        let result = loadFromDatabase<GenericRecord<int>> ()

        result |> categorize
        |> should equal ValueNotExistingErrorCase

    [<Property>]
    let ``Handles ints correctly`` (number: int) = simplePropertyTest<int> number

    [<Property>]
    let ``Handles double saving correctly`` (number: int) = genericPropertyTest<int> saveToDatabase number

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
    let ``Handles option ints correctly`` (numberOption: Option<int>) = simplePropertyTest<int option> numberOption

    [<Property>]
    let ``Handles int list correctly`` (numberList: int list) = simplePropertyTest<int list> numberList

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
        { value = textRecord.value.Get } |> simplePropertyTest<GenericRecord<string>>

    [<Property>]
    let ``Handles int option, string record2 correctly`` (record: GenericRecord2<int option, NonNull<string>>) =
        let record =
            { value1 = record.value1
              value2 = record.value2.Get }

        simplePropertyTest<GenericRecord2<int option, string>> record

    [<Property>]
    let ``Handles int record list correctly`` (numberRecordList: GenericRecord<int> list) =
        simplePropertyTest<GenericRecord<int> list> numberRecordList
