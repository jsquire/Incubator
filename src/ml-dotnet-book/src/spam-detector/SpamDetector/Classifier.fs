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

            let private tokenScore (group:TokenGrouping) (token:Token) =
                match group.TokenFrequencies.TryFind(token) with
                    | Some frequency -> log frequency 
                    | None           -> 0.0

            let private score (data:TokenizedData) (group:TokenGrouping) =
                log (group.ProportionOfData + (data |> Seq.sumBy (tokenScore group)))


            /// Classifies a set of data by considering it against a grouping of measured tokens
            let classify<'T> (groups:seq<(_ * TokenGrouping)>) (tokenizer:Tokenizer<'T>) (data:'T) =
                let tokenized = tokenizer data

                match groups with
                    | empty when Seq.isEmpty empty -> 
                        None

                    | groups ->                        
                        Some (
                            groups
                            |> Seq.maxBy (fun (_, group) -> score tokenized group)
                            |> fst
                        )