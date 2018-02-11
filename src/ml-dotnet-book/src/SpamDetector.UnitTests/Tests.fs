namespace MachineLearningBook.SpamDetector.UnitTests

    open System
    open Xunit
    open FsUnit.Xunit


    // http://fsprojects.github.io/FsUnit/index.html
    // http://fsprojects.github.io/FsUnit/xUnit.html

    module SampleTests =


        [<Fact>]
        let ``My test`` () =
           true |> should be True


        type ``Describe a scenario here`` () =
            let value = true

            [<Fact>] member x.
                ``when I do something, something is in then right state`` () =
                    true |> should equal value

            [<Fact>]
            member verify.``that this thing works because it's cool`` () =
                "bob" |> should equal "bob"

