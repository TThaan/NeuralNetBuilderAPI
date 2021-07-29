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

        private static Initializer initializer;
        private static PathBuilder pathBuilder;
        private static ParameterBuilder paramBuilder;

        //private static DeepLearningDataProvider.Builders.PathBuilder samplesPathBuilder;

        private static Stopwatch stopwatch = new Stopwatch();
        //private static CommandNames commands;
        private static string commandsPath = AppDomain.CurrentDomain.BaseDirectory + @"\CommandNames.txt";
        private static bool isInitializerStatusChangedEventActive = true;
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
                // "create -p" as one command or command + param ?
                AnalyzeInput(consoleInput, 
                    out MainCommand mainCommand, 
                    out string subCommand_String, 
                    out IEnumerable<string> parameters);

                string singleParameter = parameters.Count() == 1 ? parameters.First() : null;

                // CheckForAndExecutePotentialCommand() -> Refactor main commands into distinct classes!

                #region Show

                if (mainCommand == MainCommand.show)
                {
                    ShowCommand showCommand = subCommand_String.ToEnum<ShowCommand>();

                    switch (showCommand)
                    {
                        case ShowCommand.help:
                            ShowHelp();
                            break;
                        case ShowCommand.settings:
                            ShowSettings();
                            break;
                        case ShowCommand.par:
                            ShowAllParameters();
                            break;
                        case ShowCommand.netpar:
                            ShowNetParameters();
                            break;
                        case ShowCommand.trainerpar:
                            ShowTrainerParameters();
                            break;
                        case ShowCommand.net:
                            ShowNet();
                            break;
                        case ShowCommand.samples:
                            ShowSampleSet();
                            break;
                        default:
                            break;
                    }

                }

                #endregion

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
                            paramBuilder.ChangeParameter(parameters);
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
                    await TrainAsync(parameters.ToEnum<PresetValue>());

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

        #region Show methods

        private static void ShowSettings()
        {
            // Prevent double output about 'X' is null (from initlializer property) plus 'X' is unset here
            // by deactivating the event handling method temporarily.
            isInitializerStatusChangedEventActive = false;

            Console.WriteLine("\n" +
                $"     Current Settings:\n\n" +
                $"     General path    : {(pathBuilder.General == default ? " - " : pathBuilder.General)}\n" +
                $"     General prefix  : {(pathBuilder.FileName_Prefix == default ? " - " : pathBuilder.FileName_Prefix)}\n" +
                $"     General suffix  : {(pathBuilder.FileName_Suffix == default ? " - " : pathBuilder.FileName_Suffix)}\n\n" +

                $"     Path to net parameters     : {(pathBuilder.NetParameters == default ? " - " : pathBuilder.NetParameters)}\n" +
                $"     Path to trainer parameters : {(pathBuilder.TrainerParameters == default ? " - " : pathBuilder.TrainerParameters)}\n" +
                $"     Path to sample set         : {(pathBuilder.SampleSet == default ? " - " : pathBuilder.SampleSet)}\n" +
                $"     Path to initialized net    : {(pathBuilder.InitializedNet == default ? " - " : pathBuilder.InitializedNet)}\n" +
                $"     Path to trained net        : {(pathBuilder.TrainedNet == default ? " - " : pathBuilder.TrainedNet)}\n" +
                $"     Path to log file           : {(pathBuilder.Log == default ? " - " : pathBuilder.Log)}\n\n" +

                $"     Net Parameters     : {(paramBuilder.NetParameters == null ? " - " : "set")}\n" + 
                $"     Trainer Parameters : {(paramBuilder.TrainerParameters == null ? " - " : "set")}\n" +
                $"     Sample Set         : {(initializer.SampleSet.TrainSet == null || initializer.SampleSet.TestSet == null ? " - " : "set")}\n" +
                $"     Net                : {(initializer.Net == null ? " - " : "set")}\n" + 
                $"     Trainer            : {(initializer.Trainer == null ? " - " : "set")}\n\n" +

                $"     Logging is {(initializer.IsLogged ? "on." : "off.")}\n\n");

            // Reactivate the event handling method again.
            isInitializerStatusChangedEventActive = true;
        }
        private static void ShowHelp()
        {
            Console.WriteLine(
                $"     General Input Format : [Main Command] [Sub Command] [opt: Parameter] [opt: Layer Id]\n\n" +
                $"     All Commands: \n\n" +
                $"     Set general path                          : {MainCommand.path}=[general path]\n" +
                $"     Set general prefix for file names         : {MainCommand.path} {PathCommand.prefix}=[general prefix]\n" +
                $"     Set general suffix for file names         : {MainCommand.path} {PathCommand.suffix}=[general suffix]\n" +
                $"     Set path to initialized net               : {MainCommand.path} {PathCommand.net0}=[path to initialized net]\n" +
                $"     Set path to trained net                   : {MainCommand.path} {PathCommand.net1}=[path to trained net]\n" +
                $"     Set path to sample set                    : {MainCommand.path} {PathCommand.samples}=[path to sample set]\n" +
                $"     Set path to net parameters                : {MainCommand.path} {PathCommand.netpar}=[path to net parameters]\n" +
                $"     Set path to trainer parameters            : {MainCommand.path} {PathCommand.trainerpar}=[path to trainer parameters]\n" +
                $"     Set path to log file                      : {MainCommand.path} {PathCommand.log}=[path to log file]\n" +
                //$"     Use general path for all files and default names : {MainCommand.path} {PathCommand.UseGeneralPathAndDefaultNames}\n" +
                $"     Reset general path and use default names  : {MainCommand.path} {PathCommand.reset}\n\n" +

                // Implement/Check optionality
                $"     Create all parameters            : {MainCommand.create} {CreateCommand.par}\n" +
                $"     Create the net parameters        : {MainCommand.create} {CreateCommand.netpar}\n" +
                $"     Create the trainer parameters    : {MainCommand.create} {CreateCommand.trainerpar}\n" +
                $"     Create sample set, net & trainer : {MainCommand.create} {CreateCommand.all}\n" +
                $"     Create sample set                : {MainCommand.create} {CreateCommand.samples}\n" +
                $"     Create the net                   : {MainCommand.create} {CreateCommand.net} [opt: append]\n" +
                $"                                        append = append an auto-generated last layer considering all labels\n" +
                $"     Create the trainer               : {MainCommand.create} {CreateCommand.trainer}\n\n" +

                $"     Load all parameters              : {MainCommand.load} {LoadAndSaveCommand.par}\n" +
                $"     Load net parameters              : {MainCommand.load} {LoadAndSaveCommand.netpar}\n" +
                $"     Load trainer parameters          : {MainCommand.load} {LoadAndSaveCommand.trainerpar}\n" +
                $"     Load sample set, net & trainer   : {MainCommand.load} {LoadAndSaveCommand.all}\n" +
                $"     Load sample set                  : {MainCommand.load} {LoadAndSaveCommand.samples}\n" +
                $"     Load initialized net             : {MainCommand.load} {LoadAndSaveCommand.net0}\n" +
                $"     Load trained net                 : {MainCommand.load} {LoadAndSaveCommand.net1}\n\n" +

                $"     Save all parameters              : {MainCommand.save} {LoadAndSaveCommand.par}\n" +
                $"     Save net parameters              : {MainCommand.save} {LoadAndSaveCommand.netpar}\n" +
                $"     Save trainer parameters          : {MainCommand.save} {LoadAndSaveCommand.trainerpar}\n" +
                $"     Save sample set, net & trainer   : {MainCommand.save} {LoadAndSaveCommand.all}\n" +
                $"     Save sample set                  : {MainCommand.save} {LoadAndSaveCommand.samples}\n" +
                $"     Save initialized net             : {MainCommand.save} {LoadAndSaveCommand.net0}\n" +
                $"     Save trained net                 : {MainCommand.save} {LoadAndSaveCommand.net1}\n\n" +

                $"     Show Settings                    : {MainCommand.show} {ShowCommand.settings}\n" +
                $"     Show this help                   : {MainCommand.show} {ShowCommand.help}\n" +
                $"     Show all parameters              : {MainCommand.show} {ShowCommand.par}\n" +
                $"     Show net parameters              : {MainCommand.show} {ShowCommand.netpar}\n" +
                $"     Show trainer parameters          : {MainCommand.show} {ShowCommand.trainerpar}\n" +
                $"     Show net                         : {MainCommand.show} {ShowCommand.net}\n" +
                $"     Show sample set                  : {MainCommand.show} {ShowCommand.samples}\n\n");

            Console.WriteLine(
                $"     Change layer                              : {MainCommand.layer} [layer command] [opt: :parameter value] [opt: layer id]\n" +
                $"     Add layer after layer index               : {MainCommand.layer} {LayerCommand.del} [preceding layer id]\n" +
                $"     Example 1 (Add a new layer after layer 0) : {MainCommand.layer} {LayerCommand.left} L0\n\n");

            Console.WriteLine(
                $"     Change parameter                          : {MainCommand.param} [parameter command] [parameter name]\n" +
                $"                                                 {Enumerable.Repeat(' ', MainCommand.param.ToString().Length).ToStringFromCollection(string.Empty)} [opt: :parameter value] [opt: layer id or 'glob']\n" +
                $"     Example 2 (Set the global WeightMax 1)    : {MainCommand.param} {ParameterCommand.set} {ParameterName.wMax}:1 glob\n" + // global!
                $"     Example 3 (Set BiasMin of layer 3 to 2)   : {MainCommand.param} {ParameterCommand.set} {ParameterName.bMin}:2 L3\n" +
                $"     Parameter Names                           : {Enum.GetNames(typeof(ParameterName)).ToStringFromCollection(", ", 4, 49)}\n\n");

            var activationTypes = Enum.GetNames(typeof(ActivationType));
            Console.WriteLine(
                $"     Set Activation Type of layer 3 :");
            for (int i = 0; i < activationTypes.Length; i++)
            {
                Console.WriteLine(
                $"     {activationTypes[i], -30} : {MainCommand.param} {ParameterCommand.set} act:{i} L3");
            }
            Console.WriteLine("\n" + 
                $"     Activate logging    : {MainCommand.log} {LogCommand.on}\n" +
                $"     Deactivate logging  : {MainCommand.log} {LogCommand.off}\n" +
                $"     Start test training : {MainCommand.test}\n" +
                $"     Start training      : {MainCommand.train} [opt: shuffle]\n" +
                $"                           shuffle = shuffle training samples before first training\n\n");
        }
        private static void ShowAllParameters()
        {
            ShowNetParameters();
            ShowTrainerParameters();
        }
        private static void ShowNetParameters()
        {
            Console.WriteLine();

            if (paramBuilder.NetParameters == null)
            {
                Console.WriteLine("     Net parameters are not set yet.");
                return;
            }

            Console.WriteLine(
                $"     Layers         : {paramBuilder.LayerParametersCollection.Count}\n" +
                $"     WeightInitType : {paramBuilder.NetParameters.WeightInitType}\n");
            
            foreach (var lp in paramBuilder.NetParameters.LayerParametersCollection)
            {
                Console.WriteLine($"     Layer {lp.Id}: N = {lp.NeuronsPerLayer}, weightRange = {lp.WeightMin}/{lp.WeightMax}, biasRange = {lp.BiasMin}/{lp.BiasMax}, Activation = {lp.ActivationType}");
            }
        }
        private static void ShowTrainerParameters()
        {
            Console.WriteLine();

            if (paramBuilder.TrainerParameters == null)
            {
                Console.WriteLine("     Trainer parameters are not set yet.");
                return;
            }

            Console.WriteLine(
                $"     Learning Rate        : {paramBuilder.TrainerParameters.LearningRate}\n" +
                $"     Learning Rate Change : {paramBuilder.TrainerParameters.LearningRateChange}\n" +
                $"     Epochs               : {paramBuilder.TrainerParameters.Epochs}\n" +
                $"     Cost Type            : {paramBuilder.TrainerParameters.CostType}\n");
        }
        private static void ShowNet()
        {
            Console.WriteLine();

            if (initializer.Net == null)
            {
                Console.WriteLine("     Net is not set yet.");
                return;
            }

            Console.WriteLine(
                $"     Layers         : {initializer.Net.Layers.Count()}\n" +
                $"     WeightInitType : {paramBuilder.NetParameters.WeightInitType}\n");

            foreach (var layer in initializer.Net.Layers)
            {
                Console.WriteLine($"     Layer {layer.Id}: " +
                    $"N = {layer.N}, " +
                    $"weightRange = {layer.Weights?.GetMinimum<float>()}/{layer.Weights?.GetMaximum<float>()}, " +
                    $"biasRange = {layer.Biases?.GetMinimum()}/{layer.Biases?.GetMaximum()}, " +
                    $"Activation = {layer.ActivationFunction.ActivationType}");
            }
        }
        private static void ShowSampleSet()
        {
            Console.WriteLine();

            if (initializer.SampleSet == null || initializer.SampleSet.TrainSet == null || initializer.SampleSet.TestSet == null)
                throw new ArgumentException("     Sample set is not set yet. Or it is missing training samples or test samples.");

            Console.WriteLine(
                $"     Training Samples : {initializer.SampleSet.TrainSet.Count()}\n" +
                $"     Test Samples     : {initializer.SampleSet.TestSet.Count()}\n" +
                $"     Labels / Targets : {initializer.SampleSet.Targets.Count()}\n");

            Console.WriteLine("     First 5 training samples:\n");
            for (int i= 0; i <= 5; i++)
            {
                Console.WriteLine($"     Label    : {initializer.SampleSet.TrainSet[i].Label}");
                Console.WriteLine($"     Features : {initializer.SampleSet.TrainSet[i].Features.ToStringFromCollection(", ", 4, 5)}");
            }

            var labels = initializer.SampleSet.Targets.Keys.ToArray();

            Console.WriteLine("\n     First 100 labels & targets:\n");
            foreach (var kvp in initializer.SampleSet.Targets)
            {
                Console.WriteLine($"     Label   : {kvp.Key}");
                Console.WriteLine($"     Target  : {kvp.Value.ToStringFromCollection(", ", 20, 5)}");

                int index = Array.IndexOf(labels, kvp.Key);
                if (index >= 100)
                    break;
            }
        }

        #endregion

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

            // Check if parameter names exist as an enum 'ParameterName'.

            if (parameters.Count() > 0 &&
                parameters.Any(x => !Enum.GetNames(typeof(ParameterName)).Contains(x.Split(':').First().ToString())))
                throw new ArgumentException("...");

            // Check if parameter values are of type int.

            if (!parameters.Any(x => int.Parse(x.Split(':').Last()).GetType() == typeof(int)))
                throw new ArgumentException("");
        }
        private static int GetLayerId(IEnumerable<string> inputHelpers)
        {
            string onlyElementStartingWithL = inputHelpers.SingleOrDefault(x => string.Equals(x[0].ToString(), InputHelper.L.ToString()));
            
            if (onlyElementStartingWithL == null)
                return -1;

            return int.Parse(onlyElementStartingWithL.Skip(1).ToStringFromCollection());
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