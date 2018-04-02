namespace MachineLearningBook.SpamDetector.UnitTests
    module TokenizerTests =

        open MachineLearningBook.SpamDetector
        open Xunit
        open FsUnit.Xunit  

        //
        // Work Break Tokenizing
        //
        type ``String data should be able to be tokenized using word breaks`` () =

            [<Theory>]
            [<InlineData(null)>]
            [<InlineData("")>]
            member verify.``A string with no content produces no tokens`` stringData =
                let result = Tokenizer.wordBreakTokenizer stringData
                
                result |> should not' (be Null)
                result |> should haveCount 0

            
            [<Fact>]
            member verify.``A string a single word produces a single tokens`` () =
                let data   = "single"
                let result = Tokenizer.wordBreakTokenizer data
                
                result |> should not' (be Null)
                result |> should haveCount 1
                result.Contains(data) |> should be True


            [<Fact>]
            member verify.``A string with words seaparated by a space produces the expected tokens`` () =
                let first  = "single"
                let second = "notsingle"
                let data   = sprintf "%s %s" first second
                let result = Tokenizer.wordBreakTokenizer data
                
                result |> should not' (be Null)
                result |> should haveCount 2

                result.Contains(first)  |> should be True
                result.Contains(second) |> should be True

            
            [<Fact>]
            member verify.``A string with words seaparated by non-word characters produces the expected tokens`` () =
                let first  = "single"
                let second = "notsingle"
                let third  = "triple"
                let data   = sprintf "%s\t%s::%s" first second third
                let result = Tokenizer.wordBreakTokenizer data
                
                result |> should not' (be Null)
                result |> should haveCount 3

                result.Contains(first)  |> should be True
                result.Contains(second) |> should be True
                result.Contains(third)  |> should be True


            [<Fact>]
            member verify.``Tokenization ignores leading and trailing punctuation`` () =
                let first  = "single"
                let second = "notsingle"
                let third  = "triple"
                let data   = sprintf "???%s\t%s %s!!!!" first second third
                let result = Tokenizer.wordBreakTokenizer data
                
                result |> should not' (be Null)
                result |> should haveCount 3

                result.Contains(first)  |> should be True
                result.Contains(second) |> should be True
                result.Contains(third)  |> should be True