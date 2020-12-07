namespace TypedPersistence.Core

open System

// C# friendly single case union: http://fssnip.net/7Vd
/// Type for version of saves
[<Struct>]
type Version =
    | Version of uint32
    override this.ToString() = let (Version v) = this in string v
    static member op_Equality(a, b: Version) = a = b
    static member op_Inequality(a, b: Version) = a <> b

type IPersistenceProvider<'context> =
    abstract getVersion: 'context -> Version option
    abstract load<'T> : 'context -> 'T option
    abstract save<'T> : 'context -> 'T -> unit
    abstract saveVersion<'T> : 'context -> Version -> 'T -> unit

type IPersistenceProviderCSharp<'context> =
    abstract GetVersion: 'context -> Nullable<Version>

    abstract Load<'T when 'T: struct and 'T: (new: unit -> 'T) and 'T :> System.ValueType> : 'context
     -> Nullable<'T>

    abstract Save<'T> : 'context * 'T -> unit
    abstract SaveVersion<'T> : 'context * Version * 'T -> unit

module Provider =
    /// Method, which transforms a FSharp Provider to an CSharp one
    let fsharpToCsharp<'context> (provider: IPersistenceProvider<'context>) =
        { new IPersistenceProviderCSharp<'context> with
            member _.GetVersion context =
                provider.getVersion context |> Option.toNullable

            member _.Load<'T when 'T: struct and 'T: (new: unit -> 'T) and 'T :> System.ValueType> context
                                                                                                   =
                provider.load<'T> context |> Option.toNullable

            member _.Save<'T>(context, object) = provider.save<'T> context object

            member _.SaveVersion<'T>(context, version, object) =
                provider.saveVersion<'T> context version object }
