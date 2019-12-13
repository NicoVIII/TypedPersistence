namespace TypedPersistence.FSharp.Tests

open FsCheck
open FsCheck.Xunit
open FsUnit
open TypedPersistence.FSharp
open Xunit

module ``Saving and loading tests`` =
    [<Fact>]
    let ``Fails to load unexisting document`` () =
        cleanUpDatabase ()

        loadFromDatabase<int> ()
        |> should equal DocumentNotExisting

    [<Property>]
    let ``Handles ints correctly`` (number: int) = simplePropertyTest<int> number

    [<Property>]
    let ``Handles double saving correctly`` (number: int) = genericPropertyTest<int> (saveToDatabase<GenericRecord<int>> >> ignore) number

    [<Property>]
    let ``Handles strings correctly`` (text: NonNull<string>) =
        let text = text.Get

        simplePropertyTest<string> text

    [<Property>]
    let ``Handles floats correctly`` (number: NormalFloat) =
        number.Get
        |> simplePropertyTest<float>

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

    [<Property>]
    let ``Handles saving with mapping with id correctly`` (number: int) =
        let numberRecord = wrapInRecord number

        cleanUpDatabase()

        saveToDatabaseWithMapping<GenericRecord<int>, GenericRecord<int>> id numberRecord |> ignore

        loadFromDatabase<GenericRecord<int>> ()
        |> checkResultSuccess number

    [<Property>]
    let ``Handles loading with mapping with id correctly`` (number: int) =
        let numberRecord = wrapInRecord number

        cleanUpDatabase()

        saveToDatabase<GenericRecord<int>> numberRecord |> ignore

        loadFromDatabaseWithMapping<GenericRecord<int>, GenericRecord<int>> id
        |> checkResultSuccess number

    [<Property>]
    let ``Handles saving and loading with mapping with id correctly`` (number: int) =
        let numberRecord = wrapInRecord number

        cleanUpDatabase()

        saveToDatabaseWithMapping<GenericRecord<int>, GenericRecord<int>> id numberRecord |> ignore

        loadFromDatabaseWithMapping<GenericRecord<int>, GenericRecord<int>> id
        |> checkResultSuccess number

    [<Property>]
    let ``Handles saving and loading with mapping with wrapping correctly`` (number: int) =
        cleanUpDatabase()

        saveToDatabaseWithMapping<GenericRecord<int>, int> wrapInRecord number |> ignore

        loadFromDatabaseWithMapping<GenericRecord<int>, int> (fun x -> x.value)
        |> function
            | Ok read ->
                if read = number then
                    true
                else
                    printfn "Expected: %A - Got: %A" number read
                    false
            | Error error ->
                printfn "Error: %A" error
                false

    [<Property>]
    let ``Handles saving and loading with mapping with +-1 correctly`` (number: int) =
        cleanUpDatabase()

        saveToDatabaseWithMapping<GenericRecord<int>, int> ((+) 1 >> wrapInRecord) number |> ignore

        loadFromDatabaseWithMapping<GenericRecord<int>, int> ((fun x -> x.value) >> (+) -1)
        |> function
            | Ok read ->
                if read = number then
                    true
                else
                    printfn "Expected: %A - Got: %A" number read
                    false
            | Error error ->
                printfn "Error: %A" error
                false
