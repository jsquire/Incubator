using System;
using System.IO;
using System.Reflection;

namespace MachineLearningBook.DigitsRecognizer.CSharp
{
    public static class EntryPoint
    {        
        public static void Main(string[] args)
        {
            var dataSetFile    = "digits.csv";
            var rootPath       = $" { Assembly.GetEntryAssembly().Location }/../../../data";
            var trainingPath   = Path.GetFullPath(Path.Combine(rootPath, $"training/{ dataSetFile }"));
            var validationPath = Path.GetFullPath(Path.Combine(rootPath, $"validation/{ dataSetFile }"));

            Console.WriteLine(rootPath);
            Console.WriteLine(trainingPath);
            Console.WriteLine(validationPath);
            Console.ReadKey();
        }
    }
}
