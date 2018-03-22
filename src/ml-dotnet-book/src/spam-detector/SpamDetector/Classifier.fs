namespace MachineLearningBook.SpamDetector
    module Classifier =

        open MachineLearningBook.SpamDetector.Tokenizer

        /// Data that has been grouped and measured by token 
        type TokenGrouping = {
            ProportionOfData : float;
            TokenFrequencies : Map<Token,float>
        }

        /// Constructs assoiated wtih a naive Bayes classification
        module NaiveBayes =

            let private calculateProportion count total = 
                match total with
                | _ when total > 0 -> ((float count) / (float total))
                | _                -> 0.0

            let private laplace count total =
                (float (count + 1)) / (float (total + 1))

            let private countTokensIn group token =
                group
                |> Seq.filter (Set.contains token)
                |> Seq.length

            let private tokenScore (group : TokenGrouping) (token : Token) =
                match group.TokenFrequencies.TryFind(token) with
                | Some frequency -> log frequency 
                | None           -> 0.0

            
            /// Analyzes a set of tokenized documents to gain an understandng of the proportions to which tokens appear in them
            let analyzeDataSet (tokenizedDataSet      : seq<TokenizedData>) 
                               (totalDataElementCount : int) 
                               (classificationTokens  : Set<Token>) =
                
                let dataElementWithTokensCount = tokenizedDataSet |> Seq.length
                                
                let calculateScore token = 
                    match dataElementWithTokensCount with
                    | count when count > 0 -> laplace (countTokensIn tokenizedDataSet token) dataElementWithTokensCount
                    | _                    -> 0.0

                let scoredTokens =
                    classificationTokens
                    |> Set.map (fun token -> token, (calculateScore token))
                    |> Map.ofSeq                    

                // TokenGrouping
                { 
                    ProportionOfData = (calculateProportion dataElementWithTokensCount totalDataElementCount) ; 
                    TokenFrequencies = scoredTokens 
                }

            
            /// Transforms raw data into a set of token groupings, organized grouped by the data label
            let transformData (data                 : seq<_ * 'TData>) 
                              (tokenizer            : Tokenizer<'TData>) 
                              (classificationTokens : Set<Token>) =            
                
                let dataList  = data |> Seq.toList
                let dataCount = dataList.Length

                dataList
                |> List.map (fun (label, content) -> (label, (tokenizer content)))
                |> Seq.groupBy fst
                |> Seq.map (fun (label, group) -> (label, (group |> Seq.map snd)))
                |> Seq.map (fun (label, group) -> (label, (analyzeDataSet group dataCount classificationTokens)))

            
            /// Classifies a set of data by considering it against a grouping of measured tokens
            let classify<'TData, 'TGroupBy> (groups    : seq<'TGroupBy * TokenGrouping>) 
                                            (tokenizer : Tokenizer<'TData>) 
                                            (data      : 'TData) =
                let tokenized = tokenizer data
                
                let calculateScore (data : TokenizedData) (group : TokenGrouping) =
                    log (group.ProportionOfData + (data |> Seq.sumBy (tokenScore group)))

                match groups with
                | empty when Seq.isEmpty empty -> 
                    None

                | groups ->                        
                    Some (
                        groups
                        |> Seq.maxBy (fun (_, group) -> calculateScore tokenized group)
                        |> fst
                    )

            
