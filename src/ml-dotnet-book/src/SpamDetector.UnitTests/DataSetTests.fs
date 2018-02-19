namespace MachineLearningBook.SpamDetector.UnitTests
    module DataSetTests =

        open System
        open FSharp.Reflection
        open MachineLearningBook.SpamDetector
        open Xunit
        open FsUnit.Xunit                

        type ``Parsing a DocType should produce the expected results`` () =
            
            /// The set of theory data containing Document.DocType names.
            static member DocTypeNameData 
                with get() = FSharpType.GetUnionCases typeof<DataSet.DocType> |> Seq.map (fun case -> [| case.Name |])

            
            [<Theory>]
            [<MemberData("DocTypeNameData")>]            
            member verify.``Exact DocTypes can be parsed`` (docTypeName:string) =
                let result = DataSet.parseDocType docTypeName
                
                result.IsSome |> should be True
                result.Value.ToString() |> should equal docTypeName


            [<Theory>]
            [<MemberData("DocTypeNameData")>]            
            member verify.``DocTypes with mismatched case can be parsed`` (docTypeName:string) =
                let testName =
                    match docTypeName.Substring(0, 1).ToLower() with
                      | first when first < "m" -> docTypeName.ToLower()
                      | _                      -> docTypeName.ToUpper()

                let result = DataSet.parseDocType testName
                
                result.IsSome |> should be True
                result.Value.ToString() |> should equal docTypeName

            
            [<Fact>]
            member verify.``Invalid DocTypes are not parsed`` () =
                let testName = "this is not a valid name"
                let result   = DataSet.parseDocType testName 

                result.IsNone |> should be True

        
        type ``Parsing a document line should produce the expected results`` () =
            [<Fact>]
            member verify.``A line without a separator fails parsing `` () =
                let result = DataSet.parseLine "there is no tab here"
                
                result |> should equal None

            
            [<Fact>]
            member verify.``A line without a valid DocType fails parsing`` () =
                let result = DataSet.parseLine "INVALID\tstuffs"
                
                result |> should equal None

            
            [<Fact>]
            member verify.``A line without content fails parsing`` () =
                let result = DataSet.parseLine "INVALID\t"
                
                result |> should equal None


            [<Fact>]
            member verify.``A valid line can be parsed`` () =
                let expectedDocType = DataSet.DocType.Spam
                let expectedContent = "this is some content"
                let line            = sprintf "%s\t%s" (expectedDocType.ToString()) expectedContent                
                let result          = DataSet.parseLine line

                result.IsSome |> should be True

                let (docType, content) = result.Value

                docType |> should equal expectedDocType
                content |> should equal expectedContent
                
                



