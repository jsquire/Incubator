namespace MachineLearningBook.SpamDetector.UnitTests
    module ClassifierTests =

        open MachineLearningBook.SpamDetector
        open DataSet
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
        // Naive Bayes Transform tests
        //
        type ``The Naive Bays data transformation should produce the expected results`` () =

            static member DataTransformationLengthMemberData
                with get() =                                         
                    seq<obj[]> {
                        yield [| [ (DocType.Ham, "this is a test")];                                                                                          1 |]
                        yield [| [ (DocType.Ham, "this is a test"); (DocType.Spam, "Another one") ];                                                          2 |]
                        yield [| [ (DocType.Ham, "this is a test"); (DocType.Spam, "Another one"); (DocType.Spam, "A thired") ];                              2 |]
                        yield [| [ (DocType.Ham, "this is a test"); (DocType.Spam, "Another one"); (DocType.Spam, "A thired"); (DocType.Spam, "A fourth" ) ]; 2 |]
                        yield [| list<DocType * string>.Empty;                                                                                                0 |]
                    }


            static member DataTransformationResultLabelsMemberData
                with get() =                                         
                    seq<obj[]> {
                        yield [| [ (DocType.Ham, "this is a test")];                                                                                          [ DocType.Ham ]               |]
                        yield [| [ (DocType.Ham, "this is a test"); (DocType.Spam, "Another one") ];                                                          [ DocType.Ham; DocType.Spam ] |]
                        yield [| [ (DocType.Ham, "this is a test"); (DocType.Spam, "Another one"); (DocType.Spam, "A thired") ];                              [ DocType.Ham; DocType.Spam ] |]
                        yield [| [ (DocType.Ham, "this is a test"); (DocType.Spam, "Another one"); (DocType.Spam, "A thired"); (DocType.Spam, "A fourth" ) ]; [ DocType.Ham; DocType.Spam ] |]
                        yield [| [ (DocType.Ham, "this is a test"); (DocType.Ham, "Another one"); (DocType.Ham, "A thired"); (DocType.Ham, "A fourth" ) ];    [ DocType.Ham]                |]
                        yield [| List<DocType * string>.Empty;                                                                                                List<DocType>.Empty           |]
                    }


            [<Fact>]
            member verify.``An empty set of data produces an empty result`` () =
                let empty  = List.empty<(DocType * string)>
                let tokens = Set.empty.Add("One").Add("Two") 
                let result = NaiveBayes.transformData empty Tokenizer.wordBreakTokenizer tokens

                result |> should be Empty


            [<Fact>]
            member verify.``An empty set of classification tokens should produce no token results`` () =
                let data   = [ (DocType.Ham, "this is a test"); (DocType.Spam, "Another one") ]
                let tokens = Set.empty 
                let result = NaiveBayes.transformData data Tokenizer.wordBreakTokenizer tokens
                                
                let tokenGroups = 
                    result 
                    |> Seq.map snd
                    |> Seq.filter (fun group -> (not (Map.isEmpty group.TokenFrequencies)))

                tokenGroups |> should be Empty

            [<Theory>]
            [<MemberData("DataTransformationLengthMemberData")>]
            member verify.``A data set produces the expected result length`` (inputSet       : List<DocType * string>) 
                                                                             (expectedLength : int)   =                
                let tokens = Set.empty.Add("One").Add("Two") 
                let result = NaiveBayes.transformData inputSet Tokenizer.wordBreakTokenizer tokens
                
                (result |> Seq.length) |> should equal expectedLength



            [<Theory>]
            [<MemberData("DataTransformationResultLabelsMemberData")>]
            member verify.``A data set produces the expected result labels`` (inputSet       : List<DocType * string>) 
                                                                             (expectedLabels : List<DocType>) =                
                let tokens = Set.empty.Add("One").Add("Two") 
                let result = NaiveBayes.transformData inputSet Tokenizer.wordBreakTokenizer tokens
                
                let resultLabels =
                    result 
                    |> Seq.map (fun item -> (fst item))
                    |> List.ofSeq

                
                resultLabels |> should matchList expectedLabels

            
            [<Fact>]
            member verify.``A data set with single label and no tokens produces the expected result tokens`` () =                
                let docType  = DocType.Ham                
                let tokens   = (Set.empty.Add("One").Add("Two") |> Set.map (fun item -> item.ToLowerInvariant ()))
                let inputSet = [ (docType, "This has no token")]; 
                let result   = NaiveBayes.transformData inputSet Tokenizer.wordBreakTokenizer tokens

                let groupings = 
                    result                         
                    |> Seq.map snd
                    |> Array.ofSeq

                groupings |> should haveLength 1

                let grouping = groupings.[0]

                grouping.ProportionOfData |> should equal 1.0
                grouping.TokenFrequencies |> should haveCount 2
                
                grouping.TokenFrequencies 
                |> Map.toList                
                |> List.map fst
                |> should matchList (tokens |> Set.toList)

                grouping.TokenFrequencies
                |> Map.toList
                |> List.fold (fun acc item -> acc + (snd item)) 0.0
                |> should (equalWithin 0.25) 1.0
                

            [<Fact>]
            member verify.``A data set with single label and token produces the expected result tokens`` () =                
                let docType  = DocType.Ham
                let token    = "One".ToLowerInvariant ()
                let tokens   = Set.empty.Add(token) 
                let inputSet = [ (docType, (sprintf "This has %s" token)) ]; 
                let result   = NaiveBayes.transformData inputSet Tokenizer.wordBreakTokenizer tokens

                let groupings = 
                    result                         
                    |> Seq.map snd
                    |> Array.ofSeq

                groupings |> should haveLength 1

                let grouping = groupings.[0]

                grouping.ProportionOfData |> should equal 1.0
                grouping.TokenFrequencies |> should haveCount 1
                grouping.TokenFrequencies.ContainsKey(token) |> should be True

                grouping.TokenFrequencies
                |> Map.toList
                |> List.fold (fun acc item -> acc + (snd item)) 0.0
                |> should (equalWithin 0.25) 1.0

        
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

