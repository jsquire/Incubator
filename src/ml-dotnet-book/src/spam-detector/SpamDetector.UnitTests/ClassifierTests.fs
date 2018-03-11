namespace MachineLearningBook.SpamDetector.UnitTests
    module ClassifierTests =

        open MachineLearningBook.SpamDetector
        open Tokenizer
        open Classifier
        open Xunit
        open FsUnit.Xunit  

        //
        // Naive Bayes Analyze tests
        //
        type ``The Naive Bays analysis should produce the expected results`` () =
            
            static member DataSetProportionMemberData 
                with get() = 
                    let singleSet = [| Set.empty.Add("blah") |] |> Seq.ofArray
                    let doubleSet = [| Set.empty.Add("blah"); Set.empty.Add("blah") |] |> Seq.ofArray
                    
                    seq<obj[]> {
                        yield [| singleSet; 1;  1.0        |]
                        yield [| doubleSet; 2;  1.0        |]
                        yield [| singleSet; 2;  0.5        |]
                        yield [| doubleSet; 4;  0.5        |]
                        yield [| singleSet; 3; (1.0 / 3.0) |]
                        yield [| doubleSet; 3; (2.0 / 3.0) |]
                    }


            [<Fact>]
            member verify.``If there were no total elements in the data set, then it should have no proportion`` () =               
                let tokenizedDataSet     = [| Set.empty.Add("blah") |] |> Seq.ofArray
                let totalDataSetElements = 0
                let tokens               = [| "test"; "data" |] |> Set.ofArray
                let result               = NaiveBayes.analyzeDataSet tokenizedDataSet totalDataSetElements tokens

                result.ProportionOfData |> should equal 0.0
                result.TokenFrequencies |> should haveCount tokens.Count

            
            [<Fact>]
            member verify.``An empty data set should have no proportion`` () =               
                let tokenizedDataSet     = Set.empty
                let totalDataSetElements = 100
                let tokens               = [| "test"; "data" |] |> Set.ofArray
                let result               = NaiveBayes.analyzeDataSet tokenizedDataSet totalDataSetElements tokens

                result.ProportionOfData |> should equal 0.0
                result.TokenFrequencies |> should haveCount tokens.Count


            [<Theory>]
            [<MemberData("DataSetProportionMemberData")>]
            member verify.``The proportion represents the set elements against all elements`` (dataSet            : seq<TokenizedData>) 
                                                                                              (totalDataElements  : int) 
                                                                                              (expectedProportion : float) =               
                
                let tokens = [| "test"; "data" |] |> Set.ofArray
                let result = NaiveBayes.analyzeDataSet dataSet totalDataElements tokens

                result.ProportionOfData |> should be (equalWithin 0.05 expectedProportion) 
                result.TokenFrequencies |> should haveCount tokens.Count


            [<Fact>]
            member verify.``An empty data set returns minimum weighted tokens`` () =               
                let tokenizedDataSet     = Set.empty
                let totalDataSetElements = 100
                let tokens               = [| "test"; "data" |] |> Set.ofArray
                let result               = NaiveBayes.analyzeDataSet tokenizedDataSet totalDataSetElements tokens

                result.TokenFrequencies |> should haveCount tokens.Count
                
                // Token frequencies are smoothed, which will eliminate the chance of a 0% frequency.  Instead, they will
                // apppear in proportion to the number of tokens in the classification set.  
                //
                // Calculate the minumum frequency for the single token that appears in no elements.

                let minFrequency = (1.0 / (float (tokens |> Set.count)))

                result.TokenFrequencies 
                |> Seq.filter (fun pair -> pair.Value > minFrequency)
                |> should be Empty

         
            [<Fact>]
            member verify.``Tokens that appear in no data set should have minimum frequency`` () =               
                let noSetToken           = "never"
                let tokenizedDataSet     = [| Set.empty.Add("blah") |] |> Seq.ofArray
                let totalDataSetElements = tokenizedDataSet |> Seq.length
                let tokens               = Set.empty.Add(noSetToken);
                let result               = NaiveBayes.analyzeDataSet tokenizedDataSet totalDataSetElements tokens

                result.TokenFrequencies |> should haveCount tokens.Count
                result.TokenFrequencies.[noSetToken] |> should be (lessThanOrEqualTo 0.5)
            

            [<Fact>]
            member verify.``Token frequencies for a token in all elements and a token in no elements should reflect the minimum and maximum frequencies, respectively `` () =               
                let expectedToken        = "test"    
                let noSetToken           = "never"
                let tokenizedDataSet     = [| Set.empty.Add(expectedToken) |] |> Seq.ofArray                
                let totalDataSetElements = 10
                let tokens               = [| expectedToken; noSetToken |] |> Set.ofArray
                let result               = NaiveBayes.analyzeDataSet tokenizedDataSet totalDataSetElements tokens

                result.TokenFrequencies |> should haveCount tokens.Count

                // Token frequencies are smoothed, which will eliminate the chance of a 0% frequency.  Instead, they will
                // apppear in proportion to the number of tokens in the classification set.  
                //
                // Calculate the minumum frequency for the single token that appears in no elements.

                let minFrequency = (1.0 / (float (tokens |> Set.count)))

                result.TokenFrequencies 
                |> Seq.filter (fun pair -> pair.Value > minFrequency)
                |> Seq.length 
                |> should equal 1                

                // The expected token appears in all elements and should have a frequency of 100%

                result.TokenFrequencies 
                |> Seq.filter (fun pair -> pair.Value = 1.0)
                |> Seq.length
                |> should equal 1


            [<Fact>]
            member verify.``Token frequencies should reflect the number of data elements they appear in`` () =               
                let everySetToken        = "test"
                let oneSetToken          = "other"
                let noSetToken           = "never"
                let baseTokenSet         = Set.empty.Add(everySetToken)
                let tokenizedDataSet     = [| baseTokenSet; baseTokenSet.Add(oneSetToken) |] |> Seq.ofArray                
                let totalDataSetElements = tokenizedDataSet |> Seq.length
                let tokens               = [| everySetToken; oneSetToken; noSetToken |] |> Set.ofArray
                let result               = NaiveBayes.analyzeDataSet tokenizedDataSet totalDataSetElements tokens
                
                result.TokenFrequencies |> should haveCount tokens.Count

                // Token frequencies are smoothed, which will eliminate the chance of a 0% frequency.  Instead, they will
                // apppear in proportion to the number of tokens in the classification set.  
                //
                // Calculate the minumum frequency for the single token that appears in no elements.

                let minFrequency = (1.0 / (float (tokens |> Set.count)))

                // Each token in the set should have at least the minimum frequency.
                
                result.TokenFrequencies 
                |> Seq.filter (fun pair -> pair.Value >= minFrequency)
                |> Seq.length 
                |> should equal 3

                // Because two tokens appear in at least one element, they should have at least 50% frequency.

                result.TokenFrequencies 
                |> Seq.filter (fun pair -> pair.Value > 0.50)
                |> Seq.length 
                |> should equal 2               

                // One token appears in all elements and should have a frequency of 100%

                result.TokenFrequencies 
                |> Seq.filter (fun pair -> pair.Value = 1.0)
                |> Seq.length
                |> should equal 1

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

