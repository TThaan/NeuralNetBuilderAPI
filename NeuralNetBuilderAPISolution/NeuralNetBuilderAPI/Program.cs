using ConsoleWindowChanger;
using DeepLearningDataProvider;
using NeuralNetBuilder;
using NeuralNetBuilder.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NeuralNetBuilderAPI
{
    public class Program
    {
        #region fields

        // Later: use DI to let ICommandables access those fields instead of making them internal static?
        internal static Initializer initializer;
        internal static PathBuilder pathBuilder;
        internal static ParameterBuilder paramBuilder;

        //private static DeepLearningDataProvider.Builders.PathBuilder samplesPathBuilder;

        private static Stopwatch stopwatch = new Stopwatch();
        //private static CommandNames commands;
        private static string commandsPath = AppDomain.CurrentDomain.BaseDirectory + @"\CommandNames.txt";
        private static bool isDataProviderChangedEventActive = true;
        private static object input;

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
            ShowHelp();
            ShowSettings();

            await ExecuteConsoleCommands();
        }

        private static async Task ExecuteConsoleCommands()
        {
            Console.WriteLine();
            string consoleInput = Console.ReadLine();

            try
            {
                var splitInput = consoleInput.Split(' ');
                var mainCommand_String = splitInput.First();
                var mainCommand = mainCommand_String.ToEnum<MainCommand>();
                var parameters = splitInput.Skip(1);

                var iComm = mainCommand.ToICommandable();
                await iComm.Execute(parameters);

                string singleParameter = parameters.Count() == 1 ? parameters.First() : null;
                int layerId = GetLayerId(parameters);
                string[] paramsWithoutLayerId = parameters.Where(x => !Equals(x.Split(':').First(), ParameterName.L.ToString())).ToArray();

                // CheckForAndExecutePotentialCommand() -> Refactor main commands into distinct classes!

                #region Path

                else if (mainCommand == MainCommand.path)
                {
                    PathCommand pathCommand = subCommand_String.ToEnum<PathCommand>();

                    switch (pathCommand)
                    {
                        case PathCommand.prefix:
                            pathBuilder.SetFileNamePrefix(singleParameter);
                            break;
                        case PathCommand.suffix:
                            pathBuilder.SetFileNameSuffix(singleParameter);
                            break;
                        case PathCommand.reset:
                            pathBuilder.ResetPaths();
                            break;
                        case PathCommand.general:
                            pathBuilder.SetGeneralPath(singleParameter);
                            break;
                        case PathCommand.net0:
                            pathBuilder.SetInitializedNetPath(singleParameter);
                            break;
                        case PathCommand.net1:
                            pathBuilder.SetTrainedNetPath(singleParameter);
                            break;
                        case PathCommand.samples:
                            pathBuilder.SetSampleSetPath(singleParameter);
                            break;
                        case PathCommand.netpar:
                            pathBuilder.SetNetParametersPath(singleParameter);
                            break;
                        case PathCommand.trainerpar:
                            pathBuilder.SetTrainerParametersPath(singleParameter);
                            break;
                        case PathCommand.log:
                            pathBuilder.SetLogPath(singleParameter);
                            break;
                        default:
                            break;
                    }
                }
            
                #endregion
            
                #region Create

                else if (mainCommand == MainCommand.create)
                {
                    CreateCommand createCommand = subCommand_String.ToEnum<CreateCommand>();
                
                    switch (createCommand)
                    {
                        case CreateCommand.all:
                            await CreateNetAndTrainerAsync();
                            break;
                        case CreateCommand.net:
                            await initializer.CreateNetAsync(singleParameter.ToEnum<PresetValue>());
                            break;
                        case CreateCommand.trainer:
                            if (await initializer.CreateTrainerAsync(initializer.SampleSet))
                                initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;
                            break;
                        case CreateCommand.par:
                            CreateAllParameters();
                            break;
                        case CreateCommand.netpar:
                            paramBuilder.CreateNetParameters();
                            break;
                        case CreateCommand.trainerpar:
                            paramBuilder.CreateTrainerParameters();
                            break;
                        default:
                            break;
                    }
                }

                #endregion

                #region Load

                else if (mainCommand == MainCommand.load)
                {
                    LoadAndSaveCommand loadCommand = subCommand_String.ToEnum<LoadAndSaveCommand>();

                    switch (loadCommand)
                    {
                        case LoadAndSaveCommand.all:
                            await LoadSamplesNetAndTrainerAsync();
                            break;
                        case LoadAndSaveCommand.net0:
                            await initializer.LoadNetAsync();
                            break;
                        case LoadAndSaveCommand.net1:
                            await initializer.LoadTrainedNetAsync();
                            break;
                        case LoadAndSaveCommand.samples:
                            await LoadSampleSetAsync(pathBuilder.SampleSet, parameters);
                            break;
                        case LoadAndSaveCommand.par:
                            await LoadAllParametersAsync();
                            break;
                        case LoadAndSaveCommand.netpar:
                            await paramBuilder.LoadNetParametersAsync();
                            break;
                        case LoadAndSaveCommand.trainerpar:
                            await paramBuilder.LoadTrainerParametersAsync();
                            break;
                        default:
                            break;
                    }
                }

                #endregion

                #region Save

                else if (mainCommand == MainCommand.save)
                {
                    LoadAndSaveCommand saveCommand = subCommand_String.ToEnum<LoadAndSaveCommand>();

                    switch (saveCommand)
                    {
                        case LoadAndSaveCommand.all:
                            await SaveSamplesNetAndTrainerAsync();
                            break;
                        case LoadAndSaveCommand.net0:
                            await initializer.SaveInitializedNetAsync();
                            break;
                        case LoadAndSaveCommand.net1:
                            await initializer.SaveTrainedNetAsync();
                            break;
                        case LoadAndSaveCommand.samples:
                            await initializer.SampleSet.SaveSampleSetAsync(pathBuilder.SampleSet);  // Task: dynamize testSamplesFaction!
                            break;
                        case LoadAndSaveCommand.par:
                            await SaveAllParametersAsync(singleParameter.ToEnum<PresetValue>());
                            break;
                        case LoadAndSaveCommand.netpar:
                            await paramBuilder.SaveNetParametersAsync(singleParameter.ToEnum<PresetValue>());
                            break;
                        case LoadAndSaveCommand.trainerpar:
                            await paramBuilder.SaveTrainerParametersAsync(singleParameter.ToEnum<PresetValue>());
                            break;
                        default:
                            break;
                    }
                }

                #endregion

                #region Change parameter

                else if (mainCommand == MainCommand.param)
                {
                    ParameterCommand parameterCommand = subCommand_String.ToEnum<ParameterCommand>();

                    switch (parameterCommand)
                    {
                        case ParameterCommand.set:
                            paramBuilder.ChangeParameter(parameters, layerId);
                            break;
                        //case ParameterCommand.add:
                        //    paramBuilder.AddLayerAfter(layerId);
                        //    break;
                        default:
                            break;
                    }
                }

                #endregion

                #region Change layer

                else if (mainCommand == MainCommand.layer)
                {
                    LayerCommand layerCommand = subCommand_String.ToEnum<LayerCommand>();

                    switch (layerCommand)
                    {
                        //case LayerCommand.set:
                        //    paramBuilder.ChangeParameter(parameter, layerId);
                        //    break;
                        case LayerCommand.add:
                            paramBuilder.AddLayerAfter(layerId);
                            break;
                        case LayerCommand.del:
                            paramBuilder.DeleteLayer(layerId);
                            break;
                        case LayerCommand.left:
                            paramBuilder.MoveLayerLeft(layerId);
                            break;
                        case LayerCommand.right:
                            paramBuilder.MoveLayerRight(layerId);
                            break;
                        default:
                            break;
                    }
                }

                #endregion

                #region Log

                else if (mainCommand == MainCommand.log)
                    switch (subCommand_String.ToEnum<LogCommand>())
                    {
                        case LogCommand.on:
                            LogOn();
                            break;
                        case LogCommand.off:
                            LogOff();
                            break;
                        default:
                            break;
                    }

                #endregion

                #region Misc

                else if (mainCommand == MainCommand.test)
                    await TestTraining();
                else if (mainCommand == MainCommand.train)
                    await TrainAsync(singleParameter.ToEnum<PresetValue>());

                #endregion

                else
                    throw new ArgumentException("Unkown Command.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            await ExecuteConsoleCommands();
        }

        #region Misc Methods

        /// <summary>
        /// Valid parameters: Undefined, Shuffle
        /// </summary>
        private static async Task TrainAsync(PresetValue parameter = PresetValue.undefined)
        {
            if (parameter != PresetValue.undefined && parameter != PresetValue.shuffle)
                throw new ArgumentException($"Parameter {parameter} is not valid here. Use {PresetValue.shuffle} or no parameter.");

            stopwatch.Reset();
            stopwatch.Start();
            await initializer.TrainAsync(initializer.SampleSet, parameter);
            stopwatch.Stop();

            await initializer.SaveTrainedNetAsync();
        }
        private static void LogOn()
        {
            initializer.IsLogged = true;
            Console.WriteLine("Logging activated.");
        }
        private static void LogOff()
        {
            initializer.IsLogged = false;
            Console.WriteLine("Logging deactivated.");
        }
        private async static Task TestTraining()
        {
            pathBuilder.ResetPaths();
            //pathBuilder.FileName_Prefix = @"";
            //pathBuilder.FileName_Suffix = "_test.txt";
            pathBuilder.General = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\Test2\";

            // Get samples

            if (!await initializer.SampleSet.LoadSampleSetAsync(pathBuilder.SampleSet, .1f, 0))
                return;             // Always check if the loaded sample set suits the ... parameters!

            // Get net

            if (!await paramBuilder.LoadNetParametersAsync())
                return;
            if (!await initializer.CreateNetAsync())
                return;
            // if (!await initializer.LoadNetAsync())
            //     return;        // Always check if the loaded initialized net suits loaded parameters!

            // Get trainer

            if (!await paramBuilder.LoadTrainerParametersAsync())
                return;
            if (!await initializer.CreateTrainerAsync(initializer.SampleSet))
                return;
            initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;
            
            // await initializer.Trainer.SampleSet.TrainSet.ShuffleAsync();

            // Show Initialized Net

            ShowNet();

            // Activate logging
            LogOn();

            // Train

            await TrainAsync();
        }

        #endregion

        /// <summary>
        /// Default values if you parameters miss an input helper:
        /// testSamplesInPercent = 10, columnIndex_Label = 0.
        /// </summary>
        public static async Task<bool> LoadSampleSetAsync(string samplesFileName, IEnumerable<string> parameters)
        {
            // default values
            int testSamplesInPercent = 10, columnIndex_Label = 0;

            var testParam = parameters.SingleOrDefault(x => x.Contains(ParameterName.test.ToString()));
            if(testParam != null)
                if (!int.TryParse(testParam.Split(':').Last(), out testSamplesInPercent))
                    throw new ArgumentException($"Parameter value {testParam.Split(':').Last()} is not valid." +
                        "Parameter value for 'test' must be an integer between 1 and 99 (inclusive) defining how much percent of the samples will be used as test samples.");

            var labelParam = parameters.SingleOrDefault(x => x.Contains(ParameterName.label.ToString()));
            if (labelParam != null)
                if (!int.TryParse(labelParam.Split(':').Last(), out columnIndex_Label))
                    throw new ArgumentException($"Parameter value {labelParam.Split(':').Last()} is not valid." +
                        "Parameter value for 'label' must be a positive integer defining the index of the column holding the label values (First column index = 0!).");

            return await initializer.SampleSet.LoadSampleSetAsync(samplesFileName, (float)testSamplesInPercent / 100, columnIndex_Label);
        }

        #region Combining Methods

        public static async Task<bool> CreateNetAndTrainerAsync()
        {
            if (await initializer.CreateNetAsync())
                return false;
            if (await initializer.CreateTrainerAsync(initializer.SampleSet) == false)
                return false;

            return true;
        }
        public static bool CreateAllParameters()
        {
            paramBuilder.CreateNetParameters();
            paramBuilder.CreateTrainerParameters();

            return true;
        }
        public static async Task<bool> LoadAllParametersAsync()
        {
            bool result = true;

            if (await paramBuilder.LoadNetParametersAsync() == false) result = false;
            if (await paramBuilder.LoadTrainerParametersAsync() == false) result = false;

            return result;
        }
        public static async Task<bool> SaveAllParametersAsync(PresetValue indented = PresetValue.indented)
        {
            if (await paramBuilder.SaveNetParametersAsync(indented) == false) 
                return false;
            return await paramBuilder.SaveTrainerParametersAsync(indented) == false;
        }
        public static async Task<bool> SaveSamplesNetAndTrainerAsync() // incl trained net but no trainer
        {
            if (await initializer.SampleSet.SaveSampleSetAsync(pathBuilder.SampleSet) == false)
                return false;
            if (await initializer.SaveInitializedNetAsync() == false)
                return false;
            return await initializer.SaveTrainedNetAsync();
        }
        public static async Task<bool> LoadSamplesNetAndTrainerAsync() // incl trained net but no trainer
        {
            if (await initializer.SampleSet.LoadSampleSetAsync(pathBuilder.SampleSet, .1f, 0) == false)
                return false;
            if (await initializer.LoadNetAsync() == false)
                return false;

            return true;
        }

        #endregion

        #region Analyzing Methods
        
        private static void AnalyzeInput(string consoleInput, out MainCommand mainCommand, out string subCommand_String, out IEnumerable<string> parameters)
        {
            var splitInput = consoleInput.Split(' ');
            parameters = splitInput.Skip(2);
            CheckCommandsStructure(splitInput);
            CheckParameters(parameters);
            mainCommand = GetMainCommand(splitInput);
            subCommand_String = splitInput[1];

            switch (mainCommand)
            {
                case MainCommand.Undefined:
                    break;
                case MainCommand.path:
                    CheckSubCommand<PathCommand>(subCommand_String);
                    break;
                case MainCommand.show:
                    CheckSubCommand<ShowCommand>(subCommand_String);
                    break;
                case MainCommand.create:
                    CheckSubCommand<CreateCommand>(subCommand_String);
                    break;
                case MainCommand.load:
                    CheckSubCommand<LoadAndSaveCommand>(subCommand_String);
                    //singleParameter = currentParamNames.ToStringFromCollection(" ");
                    break;
                case MainCommand.save:
                    CheckSubCommand<LoadAndSaveCommand>(subCommand_String);
                    break;
                case MainCommand.log:
                    CheckSubCommand<LogCommand>(subCommand_String);
                    break;
                case MainCommand.train:
                    break;
                case MainCommand.test:
                    break;
                case MainCommand.param:
                    CheckSubCommand<ParameterCommand>(subCommand_String);
                    break;
                case MainCommand.layer:
                    CheckSubCommand<LayerCommand>(subCommand_String);
                    break;
                default:
                    break;
            }

            if (subCommand_String == null)
                throw new ArgumentException($"A valid sub command is missing.");
        }
        /// <summary>
        /// Check if there are two commands (main and sub) or if it is a special case (like MainCommand.Train or MainCommand.Test).
        /// </summary>
        private static void CheckCommandsStructure(string[] splitInput)
        {
            if (splitInput.Length <= 1 &&
                            splitInput[0] != MainCommand.train.ToString() &&
                            splitInput[0] != MainCommand.test.ToString())
                throw new ArgumentException("A console input must consist of at least two units: a main command followed by a sub command \n" +
                    $"excepting the potential stand-alone commands '{MainCommand.train}' and '{MainCommand.test}'.");
        }
        private static void CheckSubCommand<TEnum>(string subCommand)
        {
            if (!Enum.GetNames(typeof(TEnum)).Contains(subCommand))
                throw new ArgumentException($"SubCommand '{subCommand}' does not exist.");
        }
        private static MainCommand GetMainCommand(string[] splitInput)
        {
            return splitInput[0].ToEnum<MainCommand>();
        }
        private static void CheckParameters(IEnumerable<string> parameters)
        {
            //if (uncheckedParameters.Count() == 0)
            //    return new List<string>();

            // Check if there are any parameters at all.

            if (parameters.Count() > 0)
            {
                // Check if all parameter names exist as an enum 'ParameterName'.

                if (parameters.All(x => Enum.GetNames(typeof(ParameterName)).Contains(x.Split(':').First())))  //.ToString()
                {
                    // Check if all parameter values are of type int.

                    if (!parameters.Any(x => int.Parse(x.Split(':').Last()).GetType() == typeof(int)))
                        throw new ArgumentException("");
                }
                else
                    throw new ArgumentException("...");
            }
        }
        private static int GetLayerId(IEnumerable<string> parameters)
        {
            string layerId_String = parameters.SingleOrDefault(x => Equals(x.Split(':').First(), ParameterName.L.ToString()));

            if (layerId_String == null)
                return -1;
                // throw new ArgumentException($"Cannot find a parameter for the layer index. (Expected: {ParameterName.L}:[index (positive integer)]).");

            if (!int.TryParse(layerId_String, out int result))
                throw new ArgumentException($"Cannot transform {layerId_String} into a layer index (positive integer).");

            return result;
        }

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
        private static void Trainer_StatusChanged_EventHandlingMethod(object trainer, TrainerStatusChangedEventArgs e)
        {
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds,10}: {e.Info}");
        }

        #endregion
    }
}