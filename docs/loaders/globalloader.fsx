#r "../_lib/Fornax.Core.dll"

type UrlRoot =
  | Root of string
  member x.subRoute(route: string) =
    let (Root root) = x
    root.TrimEnd('/') + "/" + route.TrimStart('/')

  member x.subRoutef pattern = Printf.kprintf x.subRoute pattern

type SiteInfo =
  { title: string
    description: string
    theme_variant: string option
    root_url: UrlRoot }

let config =
  { title = "Typed Persistence"
    description = "Description of TypedPersistence project"
    theme_variant = Some "blue"
    root_url =
#if WATCH
      Root "//localhost:8080/"
#else
      Root "{DOCSROOTLINK}"
#endif
  }

let loader (projectRoot: string) (siteContent: SiteContents) =
  siteContent.Add(config)

  siteContent
