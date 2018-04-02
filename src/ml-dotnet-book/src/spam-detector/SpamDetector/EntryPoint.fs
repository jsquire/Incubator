namespace MachineLearningBook.SpamDetector
    module EntryPoint =

        open System
        open System.Diagnostics
        open System.Reflection
        open Humanizer
        open System.IO
        open Classifier


        [<EntryPoint>]
        let Main argv =
            let dataSetFile    = "sms-spam.txt"
            let rootPath       = sprintf "%s/../../.././../../../../data" (Assembly.GetEntryAssembly().Location)
            let trainingPath   = Path.GetFullPath(Path.Combine(rootPath, "training/" + dataSetFile))
            let validationPath = Path.GetFullPath(Path.Combine(rootPath, "validation/" + dataSetFile))
            let runWatch       = new Stopwatch()
            let watch          = new Stopwatch()
        
            printfn ""
            printfn "Run started using:"
            printfn "\tTraining Data Path: %s" trainingPath
            printfn "\tValidaiton Data Path: %s" validationPath 
            printfn ""
        
            runWatch.Start()
            watch.Start()
            printf "Preparing data..."

            let trainingSet = 
                trainingPath 
                |> File.ReadAllLines 
                |> DataSet.parseData

            let validationSet = 
                validationPath 
                |> File.ReadAllLines 
                |> DataSet.parseData
        
            watch.Stop();
            printfn " complete.  Elapsed: %s" (watch.Elapsed.Humanize())
        
            watch.Reset()
            watch.Start()
            printf "Training..."

            let classifier = NaiveBayes.train trainingSet Tokenizer.wordBreakTokenizer (Set.empty.Add "txt")
        
            watch.Stop();
            printfn " complete.  Elapsed: %s" (watch.Elapsed.Humanize())

            watch.Reset()
            watch.Start()
            printf "Classifying..."

            let avgBy (docType, text) =
                match (classifier text) with
                | Some result when docType = result -> 1.0
                | _                                 -> 0.0

            let correct =
                validationSet
                |> Seq.averageBy avgBy
        
            watch.Stop();
            printfn " complete.  Elapsed: %s" (watch.Elapsed.Humanize())

            printfn ""
            printfn "%.3f%% of classifications were correct." correct

            runWatch.Stop()
            printfn "Run complete.  Total time: %s" (runWatch.Elapsed.Humanize())            
            
            printfn ""
            printfn ""
            printf "Press any key"
            Console.ReadKey true |> ignore
            0 
