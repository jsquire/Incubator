using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Humanizer;

namespace MachineLearningBook.DigitsRecognizer
{
    public static class EntryPoint
    {        
        public static void Main(string[] args)
        {
            // Path selection is very brittle due to the assumed structure.  If not just running for
            // personal playtime purposes, these should be passed in or otherwise set in configuration.

            var dataSetFile        = "digits.csv";
            var rootPath           = $"{ Assembly.GetEntryAssembly().Location }/../../.././../../../../data";
            var trainingPath       = Path.GetFullPath(Path.Combine(rootPath, $"training/{ dataSetFile }"));
            var validationPath     = Path.GetFullPath(Path.Combine(rootPath, $"validation/{ dataSetFile }"));
            var watch              = new Stopwatch();
            var distanceCalculator = (ImageDistanceCalculator<int>)ImageDistanceCalculators.Manhattan;
            var classifier         = new BasicImageClassifier(distanceCalculator);  

            Console.WriteLine();
            Console.WriteLine("Run started using:");            
            Console.WriteLine($"\tTraining Data Path: { trainingPath }");
            Console.WriteLine($"\tValidation Data Path: { validationPath }");
            Console.WriteLine($"\tClassifier: { classifier.GetType().Name.Humanize(LetterCasing.Title) } with { distanceCalculator.Method.Name } distance algorithm");
            Console.WriteLine();

            IReadOnlyList<ImageObservation<int>> ReadFileObservations(string path) =>
                File.ReadAllLines(trainingPath)
                    .Skip(1)
                    .Select(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries))                    
                    .Select(tokens => new ImageObservation<int>(tokens[0], tokens.Skip(1).Select(token => Int32.Parse(token))))
                    .ToList();                              

            watch.Start();
            Console.Write("Preparing data...");
                    
            var trainingData   = ReadFileObservations(trainingPath);            
            var validationData = ReadFileObservations(validationPath);   

            watch.Stop();
            Console.WriteLine($" complete.  Elapsed: { watch.Elapsed.Humanize() }");

            watch.Reset();
            watch.Start();
            Console.Write("Training...");            
            
            classifier.Train(trainingData);

            watch.Stop();
            Console.WriteLine($" complete.  Elapsed: { watch.Elapsed.Humanize() }");

            watch.Reset();
            watch.Start();
            Console.Write($"Classifying...");
            
            var correctPercent = ImageClassifierEvaluator.CalculateCorrectPercentage<int>(validationData, classifier);

            watch.Stop();
            Console.WriteLine($" complete.  Elapsed: { watch.Elapsed.Humanize() }");

            Console.WriteLine();
            Console.WriteLine("Run complete.  {0:P2} classifications were correct.", correctPercent);
            Console.WriteLine();

            Console.Write("Press any key");
            Console.ReadKey();
        }
    }
}
