namespace TypedPersistence.FSharp.Tests

[<AutoOpen>]
module Types =
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
