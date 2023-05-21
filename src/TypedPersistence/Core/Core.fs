namespace TypedPersistence.Core

open System
open System.Threading.Tasks

type Version = uint32

type IPersistenceProvider<'context> =
    abstract getVersion: 'context -> Async<Version option>
    abstract load<'T> : 'context -> Async<'T option>
    abstract save<'T> : 'context -> 'T -> Async<unit>
    abstract saveVersion<'T> : 'context -> Version -> 'T -> Async<unit>

type IPersistenceProviderCSharp<'context> =
    abstract GetVersion: 'context -> Task<Nullable<Version>>

    abstract Load<'T when 'T: struct and 'T: (new: unit -> 'T) and 'T :> System.ValueType> :
        'context -> Task<Nullable<'T>>

    abstract Save<'T> : 'context * 'T -> Task<unit>
    abstract SaveVersion<'T> : 'context * Version * 'T -> Task<unit>

module Provider =
    /// Method, which transforms a FSharp Provider to an CSharp one
    let fsharpToCsharp<'context> (provider: IPersistenceProvider<'context>) =
        { new IPersistenceProviderCSharp<'context> with
            member _.GetVersion context =
                task {
                    let! version = provider.getVersion context
                    return Option.toNullable version
                }

            member _.Load<'T
                when 'T: struct and 'T: (new: unit -> 'T) and 'T :> System.ValueType>
                context
                =
                task {
                    let! content = provider.load<'T> context
                    return Option.toNullable content
                }

            member _.Save<'T>(context, object) =
                task { do! provider.save<'T> context object }

            member _.SaveVersion<'T>(context, version, object) =
                task { do! provider.saveVersion<'T> context version object } }
