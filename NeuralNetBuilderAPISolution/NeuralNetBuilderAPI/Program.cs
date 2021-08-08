using ConsoleWindowChanger;
using DeepLearningDataProvider;
using NeuralNetBuilder;
using NeuralNetBuilder.Builders;
using NeuralNetBuilderAPI.Commandables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.GlobalConstants;

namespace NeuralNetBuilderAPI
{
    public class Program
    {
        #region fields

        // Later: use DI to let ICommandables access those fields instead of making them internal static?
        internal static Initializer initializer;
        internal static PathBuilder pathBuilder;
        internal static ParameterBuilder paramBuilder;
        internal static Stopwatch stopwatch = new Stopwatch();
        internal static bool isInitializerStatusChangedEventActive = true;
        internal static bool isDataProviderChangedEventActive = true;

        #endregion

        #region methods

        static async Task Main(string[] args)
        {
            #region Fit Console Window

            Change.BorderPositions(.5m, 0, 1, 1);

            #endregion

            initializer = new Initializer();
            initializer.InitializerStatusChanged += NetbuilderChanged_EventHandlingMethod;
            pathBuilder = initializer.Paths;            
            paramBuilder = initializer.ParameterBuilder;

            initializer.SampleSet = new SampleSet();
            initializer.SampleSet.DataProviderChanged += DataProviderChanged_EventHandlingMethod;
            //samplesPathBuilder = initializer.SampleSet.PathBuilder; // redundant?
            //pathBuilder.SetSampleSetPath(Path.Combine(@"C:\Users\Jan_PC\Desktop\FourPixCam\", "Samples.csv")); //@"C:\Users\Jan_PC\Desktop\FourPixCam\", "Samples.csv")

            //commands = CommandNames.GetDefaultCommandNames();

            // pathBuilder.ResetPaths();
            Show.ShowHelp();
            Show.ShowSettings();

            await ExecuteConsoleCommands();
        }

        private static async Task ExecuteConsoleCommands()
        {
            Console.WriteLine();
            string consoleInput = Console.ReadLine();

            try
            {
                var splitInput = consoleInput.Split(Separator_ConsoleInput, StringSplitOptions.RemoveEmptyEntries);
                var mainCommand_String = splitInput.First();
                var mainCommand = mainCommand_String.ToEnum<MainCommand>();
                var parameters = splitInput.Skip(1);

                var comm = mainCommand.ToCommandableBase();
                await comm.Execute(parameters);
            }
            catch (Exception e) { Console.WriteLine(e.Message); }

            await ExecuteConsoleCommands();
        }

        #region Misc Methods

        #endregion

        #region Analyzing Methods
        
        //private static void AnalyzeInput(string consoleInput, out MainCommand mainCommand, out string subCommand_String, out IEnumerable<string> parameters)
        //{
        //    var splitInput = consoleInput.Split(' ');
        //    parameters = splitInput.Skip(2);
        //    CheckCommandsStructure(splitInput);
        //    CheckParameters(parameters);
        //    mainCommand = GetMainCommand(splitInput);
        //    subCommand_String = splitInput[1];

        //    switch (mainCommand)
        //    {
        //        case MainCommand.Undefined:
        //            break;
        //        case MainCommand.path:
        //            CheckSubCommand<PathCommand>(subCommand_String);
        //            break;
        //        case MainCommand.show:
        //            CheckSubCommand<ShowCommand>(subCommand_String);
        //            break;
        //        case MainCommand.create:
        //            CheckSubCommand<CreateCommand>(subCommand_String);
        //            break;
        //        case MainCommand.load:
        //            CheckSubCommand<LoadAndSaveCommand>(subCommand_String);
        //            //singleParameter = currentParamNames.ToStringFromCollection(" ");
        //            break;
        //        case MainCommand.save:
        //            CheckSubCommand<LoadAndSaveCommand>(subCommand_String);
        //            break;
        //        case MainCommand.log:
        //            CheckSubCommand<LogCommand>(subCommand_String);
        //            break;
        //        case MainCommand.train:
        //            break;
        //        case MainCommand.test:
        //            break;
        //        case MainCommand.param:
        //            CheckSubCommand<ParameterCommand>(subCommand_String);
        //            break;
        //        case MainCommand.layer:
        //            CheckSubCommand<LayerCommand>(subCommand_String);
        //            break;
        //        default:
        //            break;
        //    }

        //    if (subCommand_String == null)
        //        throw new ArgumentException($"A valid sub command is missing.");
        //}
        ///// <summary>
        ///// Check if there are two commands (main and sub) or if it is a special case (like MainCommand.Train or MainCommand.Test).
        ///// </summary>
        //private static void CheckCommandsStructure(string[] splitInput)
        //{
        //    if (splitInput.Length <= 1 &&
        //                    splitInput[0] != MainCommand.train.ToString() &&
        //                    splitInput[0] != MainCommand.test.ToString())
        //        throw new ArgumentException("A console input must consist of at least two units: a main command followed by a sub command \n" +
        //            $"excepting the potential stand-alone commands '{MainCommand.train}' and '{MainCommand.test}'.");
        //}
        //private static void CheckSubCommand<TEnum>(string subCommand)
        //{
        //    if (!Enum.GetNames(typeof(TEnum)).Contains(subCommand))
        //        throw new ArgumentException($"SubCommand '{subCommand}' does not exist.");
        //}
        //private static MainCommand GetMainCommand(string[] splitInput)
        //{
        //    return splitInput[0].ToEnum<MainCommand>();
        //}
        //private static void CheckParameters(IEnumerable<string> parameters)
        //{
        //    //if (uncheckedParameters.Count() == 0)
        //    //    return new List<string>();

        //    // Check if there are any parameters at all.

        //    if (parameters.Count() > 0)
        //    {
        //        // Check if all parameter names exist as an enum 'ParameterName'.

        //        if (parameters.All(x => Enum.GetNames(typeof(ParameterName)).Contains(x.Split(':').First())))  //.ToString()
        //        {
        //            // Check if all parameter values are of type int.

        //            if (!parameters.Any(x => int.Parse(x.Split(':').Last()).GetType() == typeof(int)))
        //                throw new ArgumentException("");
        //        }
        //        else
        //            throw new ArgumentException("...");
        //    }
        //}

        #endregion

        #endregion

        #region event handling methods

        private static void NetbuilderChanged_EventHandlingMethod(object initializer, InitializerStatusChangedEventArgs e)
        {
            if(isInitializerStatusChangedEventActive)
                Console.WriteLine($"{e.Info}");
        }
        private static void DataProviderChanged_EventHandlingMethod(object initializer, DataProviderChangedEventArgs e)
        {
            if(isDataProviderChangedEventActive)
                Console.WriteLine($"{e.Info}");
        }
        // public to give access to Create class. (Later: Use DI.)
        public static void Trainer_StatusChanged_EventHandlingMethod(object trainer, TrainerStatusChangedEventArgs e)
        {
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds,10}: {e.Info}");
        }

        #endregion
    }
}