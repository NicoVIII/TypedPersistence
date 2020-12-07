namespace TypedPersistence.Json

module Types =
    type Context = string

    type OnlyVersion = { version: uint32 }
    type VersionAndData<'a> = { data: 'a; version: uint32 }
