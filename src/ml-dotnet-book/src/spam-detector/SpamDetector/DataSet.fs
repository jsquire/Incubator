namespace MachineLearningBook.SpamDetector
    module DataSet =

        open System
        open FSharp.Reflection

        /// The type of document, based on its content.
        type DocType = 
            | Spam
            | Ham
            
        
        /// Attempts to parses a string representation of the DocType
        let parseDocType (stringValue:string) = 
                match FSharpType.GetUnionCases typeof<DocType> |> Array.filter (fun case -> String.Equals(case.Name, stringValue, StringComparison.InvariantCultureIgnoreCase)) with
                | [| case |] -> Some(FSharpValue.MakeUnion(case, Array.empty) :?> DocType)
                | _          -> None


        /// Parses a line in the document, separating the document type identifier from its content.
        let parseLine (line:string) =            
            
            let splitPos = 
                match line.IndexOf('\t') with
                    | pos when pos >= 0 -> Some pos
                    | _                 -> None

            let docType = 
                match splitPos with
                    | Some pos -> parseDocType (line.Substring(0, pos))
                    | _        -> None

            let content =                
                match splitPos with
                    | Some pos -> 
                          match line.Substring(pos + 1) with
                              | text when not (String.IsNullOrEmpty(text)) -> Some text
                              | _                                          -> None
                    
                    | _ -> None

            match (docType, content) with
                | (Some lineType, Some lineContent) -> Some (lineType, lineContent)
                | _                                 -> None

            
        /// Parses a set of data, ignoring lines that are not valid.
        let parseData (dataSet:seq<string>) =
            dataSet 
            |> Seq.map parseLine
            |> Seq.filter (fun line -> line.IsSome)
            |> Seq.map (fun data -> data.Value)
                

        
