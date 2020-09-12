module Tests

open Expecto
open TypedPersistence

let tests =
    testList
        "Group of tests"
        [ test "Simple saving and loading" {
              let test = "Hello World"
              save "test.json" test
              let test2 = load<string> "test.json"
              Expect.equal test2.Value test "The strings should equal"
          }

          testProperty "Reverse of reverse of a list is the original list" (fun (xs: list<int>) ->
              List.rev (List.rev xs) = xs) ]
