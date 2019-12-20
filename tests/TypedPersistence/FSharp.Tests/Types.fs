namespace TypedPersistence.FSharp.Tests

[<AutoOpen>]
module Types =
    type SingleCaseUnion = OnlyCase

    type SingleCaseIntUnion = OnlyIntCase of int

    type GenericRecord<'a> =
        { value: 'a }

    type GenericRecordAlt<'a> =
        { value1: 'a }

    type GenericRecord2<'a, 'b> =
        { value1: 'a
          value2: 'b }

    type LoadErrorCategories =
        | OkCase
        | DocumentNotExistingErrorCase
        | ValueNotExistingErrorCase
