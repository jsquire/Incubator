namespace MachineLearningBook.SpamDetector
    module Tokenizer =

        open System
        open System.Text.RegularExpressions

        /// An element that identifies a particular part of a document
        type Token = string

        /// Data that has been processed and tokenized
        type TokenizedData = Set<Token>
                
        /// A function that is responsible for inspecting data and breaking it into its composite set of tokens
        type Tokenizer<'T> = 'T -> TokenizedData
        
        let private regexOptions  = (RegexOptions.Multiline ||| RegexOptions.Compiled)
        let private wordMatcherEx = Regex(@"\w+", regexOptions)

        /// Tokenizes a set of data using traditional word breaks.
        let wordBreakTokenizer (data:string) =
            match data with
            | data when String.IsNullOrEmpty(data) -> 
                Set.empty<Token>
                
            | _ ->
                data.ToLowerInvariant()
                |> wordMatcherEx.Matches
                |> Seq.cast<Match>
                |> Seq.map (fun wordMatch -> wordMatch.Value)
                |> Set.ofSeq
