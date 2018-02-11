// Learn more about F# at http://fsharp.org
namespace MachineLearningBook.SpamDetector
    
    open System
    open System.Diagnostics
    open System.Reflection
    open Humanizer
    open System.IO

    module EntryPoint =


        [<EntryPoint>]
        let Main argv =
            let dataSetFile    = "sms-spam.txt"
            let rootPath       = sprintf "%s/../../.././../../../../data" (Assembly.GetEntryAssembly().Location)
            let trainingPath   = Path.GetFullPath(Path.Combine(rootPath, "training" + dataSetFile))
            let validationPath = Path.GetFullPath(Path.Combine(rootPath, "validation" + dataSetFile))
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
        
            watch.Stop();
            printfn " complete.  Elapsed: %s" (watch.Elapsed.Humanize())
        
            //watch.Reset()
            //watch.Start()
        
            printfn "%d classifications were correct." 0

            runWatch.Stop()
            printfn ""
            printfn "Run complete.  Total time: %s" (runWatch.Elapsed.Humanize())            
            
            printfn ""
            printfn ""
            printf "Press any key"
            Console.ReadKey true |> ignore
            0 
