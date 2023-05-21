namespace TypedPersistence.Core

open System

type Version = uint32

type IPersistenceProvider<'context> =
    abstract getVersion: 'context -> Version option
    abstract load<'T> : 'context -> 'T option
    abstract save<'T> : 'context -> 'T -> unit
    abstract saveVersion<'T> : 'context -> Version -> 'T -> unit

type IPersistenceProviderCSharp<'context> =
    abstract GetVersion: 'context -> Nullable<Version>

    abstract Load<'T when 'T: struct and 'T: (new: unit -> 'T) and 'T :> System.ValueType> :
        'context -> Nullable<'T>

    abstract Save<'T> : 'context * 'T -> unit
    abstract SaveVersion<'T> : 'context * Version * 'T -> unit

module Provider =
    /// Method, which transforms a FSharp Provider to an CSharp one
    let fsharpToCsharp<'context> (provider: IPersistenceProvider<'context>) =
        { new IPersistenceProviderCSharp<'context> with
            member _.GetVersion context =
                provider.getVersion context |> Option.toNullable

            member _.Load<'T
                when 'T: struct and 'T: (new: unit -> 'T) and 'T :> System.ValueType>
                context
                =
                provider.load<'T> context |> Option.toNullable

            member _.Save<'T>(context, object) = provider.save<'T> context object

            member _.SaveVersion<'T>(context, version, object) =
                provider.saveVersion<'T> context version object }
