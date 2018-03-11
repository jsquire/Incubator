namespace MachineLearningBook.SpamDetector.UnitTests
    module ClassifierTests =

        open MachineLearningBook.SpamDetector
        open Classifier
        open Xunit
        open FsUnit.Xunit  

        //
        // Naive Bayes Analyze tests
        //
        type ``The Naive Bays analysis should produce the expected results`` () =

            [<Fact>]
            member verify.``An empty data set returns minimum weighted tokens`` () =               
                let tokenizedDataSet     = Set.empty
                let totalDataSetElements = 100
                let tokens               = [| "test"; "data" |] |> Set.ofArray
                let result               = NaiveBayes.analyzeDataSet tokenizedDataSet totalDataSetElements tokens

                result.ProportionOfData |> should equal 0.0
                result.TokenFrequencies |> should haveCount tokens.Count
                
                result.TokenFrequencies 
                |> Seq.filter (fun pair -> pair.Value > 1.0)
                |> should be Empty

            
            [<Fact>]
            member verify.``A data set with a single element containing the token weights the token appropriately`` () =               
                let totalDataSetElements = 10
                let expectedToken        = "test"                
                let tokenizedDataSet     = [| Set.empty.Add(expectedToken) |] |> Seq.ofArray                
                let tokens               = [| expectedToken; "data" |] |> Set.ofArray
                let expectedProportion   = ((float (tokenizedDataSet |> Seq.length)) / (float totalDataSetElements))
                let result               = NaiveBayes.analyzeDataSet tokenizedDataSet totalDataSetElements tokens
                
                result.ProportionOfData |> should be (equalWithin 0.05 expectedProportion) 
                result.TokenFrequencies |> should haveCount tokens.Count

                // Verify the frequencies that the tokens are found; because the results are smoothed and not 
                // raw and the number of tokens is limited, the token that isn't found will occur in roughly half
                // as many sets as the token that is found - since there are exactly two tokens.  
                //
                // Apply that knowledge when filtering to ensure that only one token appears in > 1/2 the data set elements.

                result.TokenFrequencies 
                |> Seq.filter (fun pair -> pair.Value >= 0.51)
                |> Seq.length 
                |> should equal ((tokens |> Set.count) - 1)                

                // Only the expected token should appear in 100% of the data set elements.

                result.TokenFrequencies 
                |> Seq.filter (fun pair -> pair.Value = 1.0)
                |> Seq.length
                |> should equal  1

        //
        // Naive Bayes Classification tests
        //
        type ``The Naive Bays classification should produce the expected results`` () =

            [<Fact>]
            member verify.``An empty group returns no result`` () =               
                let groups    = Seq.empty<_ * TokenGrouping>        
                let data      = "something"
                let tokenizer = (fun target -> Set.empty<Tokenizer.Token>.Add data)
                let result    = NaiveBayes.classify groups tokenizer data

                result.IsNone |> should be True


            [<Fact>]
            member verify.``An single group returns the only element`` () =
                let group = {
                    ProportionOfData = 0.0;
                    TokenFrequencies = Map.empty<Tokenizer.Token,float>
                }

                let expected  = DataSet.DocType.Ham
                let groups    = [| (expected, group) |]                
                let data      = "something"
                let tokenizer = (fun target -> Set.empty<Tokenizer.Token>.Add data)
                let result    = NaiveBayes.classify groups tokenizer data

                result.IsSome |> should be True
                result.Value  |> should equal expected


            [<Fact>]
            member verify.``The item with the highest frequency group is returned`` () =
                let data = "something"

                let lowGroup = {
                    ProportionOfData = 50.0;
                    TokenFrequencies = [| (data, 20.0) |] |> Map.ofSeq
                }


                let highGroup = {
                    ProportionOfData = 50.0;
                    TokenFrequencies = [| (data, 80.0) |] |> Map.ofSeq
                }


                let expected  = 65
                let groups    = [| (99, lowGroup); (expected, highGroup) |]                                
                let tokenizer = (fun target -> Set.empty<Tokenizer.Token>.Add data)
                let result    = NaiveBayes.classify groups tokenizer data

                result.IsSome |> should be True
                result.Value  |> should equal expected


            [<Fact>]
            member verify.``Unknown tokens do not influence the classification`` () =
                let data    = "something"
                let unknown = "unknown"

                let lowGroup = {
                    ProportionOfData = 50.0;
                    TokenFrequencies = [| (data, 20.0); (unknown, 50.0) |] |> Map.ofSeq
                }


                let highGroup = {
                    ProportionOfData = 50.0;
                    TokenFrequencies = [| (data, 80.0); (unknown, 90.0) |] |> Map.ofSeq
                }


                let expected  = DataSet.DocType.Ham
                let groups    = [| (DataSet.DocType.Spam, lowGroup); (expected, highGroup) |]                                
                let tokenizer = (fun target -> Set.empty<Tokenizer.Token>.Add data)
                let result    = NaiveBayes.classify groups tokenizer data

                result.IsSome |> should be True
                result.Value  |> should equal expected

