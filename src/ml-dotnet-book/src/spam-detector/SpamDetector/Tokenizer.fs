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
        let wordBreakTokenizer (data : string) =
            match data with
            | data when String.IsNullOrEmpty(data) -> 
                Set.empty<Token>
                
            | _ ->
                data.ToLowerInvariant()
                |> wordMatcherEx.Matches
                |> Seq.cast<Match>
                |> Seq.map (fun wordMatch -> wordMatch.Value)
                |> Set.ofSeq

        
        (*
            Note: Chapter 2 of the book continues on to refine the tokenizer into one that extracts words that appear more often
                  in only one document type (Ham or Spam) and use those as the relevant tokens, rather than including every token
                  found.  

                  It also then does some basic massaging of those tokens to normalize patterns that appear to be phone numbers into a
                  placeholder slug so that they can be more easily used for recognition of a specific pattern appearing in spam messages
                  where callback or textback numbers are used.

                  These enhancements are not yet included in this module.
        *)
