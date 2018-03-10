namespace MachineLearningBook.SpamDetector.UnitTests
    module ClassifierTests =

        open MachineLearningBook.SpamDetector
        open Classifier
        open Xunit
        open FsUnit.Xunit  

        //
        // Naive Bayes Classifier tests
        //
        type ``The Naieve Bays classifier should produce the expected results`` () =

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

                let expected  = obj()
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


                let expected  = obj()
                let groups    = [| (obj(), lowGroup); (expected, highGroup) |]                                
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

