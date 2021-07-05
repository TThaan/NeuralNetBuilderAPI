using NeuralNetBuilder;
using NeuralNetBuilder.Builders;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NeuralNetBuilderAPI
{
    public class Program
    {
        #region fields

        private static Initializer initializer;
        private static Stopwatch stopwatch = new Stopwatch();
        private static PathBuilder paths;
        private static ParameterBuilder parameters;
        private static CommandNames commands;
        private static string commandsPath = AppDomain.CurrentDomain.BaseDirectory + @"\CommandNames.txt";
        private static bool isInitializerStatusChangedEventActive = true;

        #endregion

        #region methods

        static async Task Main(string[] args)
        {
            initializer = new Initializer();
            initializer.InitializerStatusChanged += Initializer_StatusChanged_EventHandlingMethod;
            paths = initializer.Paths;
            parameters = initializer.Parameters;
            commands = CommandNames.GetDefaultCommandNames();

            paths.ResetPaths();
            ShowHelp();
            ShowSettings();

            await ExecuteConsoleCommands();
        }

        private static async Task ExecuteConsoleCommands()
        {
            string consoleInput = Console.ReadLine();
            
            AnalyzeInput(consoleInput, out string enteredCommand, out string enteredPath, out string enteredParameterName, out string enteredParameterValue, out string layerId);

            if (enteredPath == null)
            {
                // Show
                if (enteredCommand == commands.ShowHelp)
                    ShowHelp();
                else if (enteredCommand == commands.ShowSettings)
                    ShowSettings();
                else if (enteredCommand == commands.ShowNetParameters)
                    ShowNetParameters();
                else if (enteredCommand == commands.ShowTrainerParameters)
                    ShowTrainerParameters();
                else if (enteredCommand == commands.ShowSampleSetParameters)
                    ShowSampleSetParameters();

                // Load
                else if (enteredCommand == commands.LoadNetParameters)
                    await parameters.LoadNetParametersAsync();
                else if (enteredCommand == commands.LoadTrainerParameters)
                    await parameters.LoadTrainerParametersAsync();
                else if (enteredCommand == commands.LoadSampleSetParameters)
                    await parameters.LoadSampleSetParametersAsync();
                else if (enteredCommand == commands.LoadInitializedNet)
                    await initializer.LoadInitializedNetAsync();
                else if (enteredCommand == commands.LoadTrainedNet)
                    await initializer.LoadTrainedNetAsync();
                else if (enteredCommand == commands.LoadSampleSet)
                    await initializer.LoadSampleSetAsync();

                // Create
                else if (enteredCommand == commands.CreateSampleSetParameters)
                    parameters.CreateSampleSetParameters(enteredParameterValue);
                else if (enteredCommand == commands.CreateNetParameters)
                    parameters.CreateNetParameters();
                else if (enteredCommand == commands.CreateTrainerParameters)
                    parameters.CreateTrainerParameters();
                else if (enteredCommand == commands.CreateNet)
                    await initializer.CreateNetAsync();
                else if (enteredCommand == commands.CreateTrainer)
                {
                    if (await initializer.CreateTrainerAsync())
                        initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;
                }
                else if (enteredCommand == commands.CreateSampleSet)
                    await initializer.CreateSampleSetAsync();

                // Save
                else if (enteredCommand == commands.SaveSampleSetParameters)
                    await parameters.SaveSampleSetParametersAsync();
                else if (enteredCommand == commands.SaveNetParameters)
                    await parameters.SaveNetParametersAsync();
                else if (enteredCommand == commands.SaveTrainerParameters)
                    await parameters.SaveTrainerParametersAsync();
                else if (enteredCommand == commands.SaveInitializedNet)
                    await initializer.SaveInitializedNetAsync();
                else if (enteredCommand == commands.SaveTrainedNet)
                    await initializer.SaveTrainedNetAsync();
                else if (enteredCommand == commands.SaveSampleSet)
                    await initializer.SaveSampleSetAsync();

                // Change a parameter
                else if (enteredCommand == commands.ChangeASampleSetParameter)
                    parameters.ChangeASampleSetParameter(enteredParameterName, enteredParameterValue);
                else if (enteredCommand == commands.ChangeANetParameter)
                    parameters.ChangeANetParameter(enteredParameterName, enteredParameterValue);
                else if (enteredCommand == commands.ChangeALayerParameter)
                    parameters.ChangeALayerParameter(enteredParameterName, enteredParameterValue);
                else if (enteredCommand == commands.ChangeATrainerParameter)
                    parameters.ChangeATrainerParameter(layerId, enteredParameterName, enteredParameterValue);

                // Misc
                else if (enteredCommand == commands.Log)
                    Log();
                else if (enteredCommand == commands.Unlog)
                    Unlog();
                else if (enteredCommand == commands.TestTraining)
                    await TestTraining();
                else if (enteredCommand == commands.Train)
                    await TrainAsync();
                else if (enteredCommand == commands.ResetPaths)
                    paths.ResetPaths();
                else if (enteredCommand == commands.UseGeneralPathAndDefaultNames)
                    paths.UseGeneralPathAndDefaultNames();

                else
                    Console.WriteLine("Unkown Command.");
            }
            else
            {
                // Set path
                if (enteredCommand == commands.SetGeneralPath)
                    paths.SetGeneralPath(enteredPath);
                else if (enteredCommand == commands.SetFileNamePrefix)
                    paths.SetFileNamePrefix(enteredPath);
                else if (enteredCommand == commands.SetFileNameSuffix)
                    paths.SetFileNameSuffix(enteredPath);
                else if (enteredCommand == commands.SetNetParametersPath)
                    paths.SetNetParametersPath(enteredPath);
                else if (enteredCommand == commands.SetNetParametersPath)
                    paths.SetNetParametersPath(enteredPath);
                else if (enteredCommand == commands.SetSampleSetParametersPath)
                    paths.SetSampleSetParametersPath(enteredPath);
                else if (enteredCommand == commands.SetInitializedNetPath)
                    paths.SetInitializedNetPath(enteredPath);
                else if (enteredCommand == commands.SetTrainedNetPath)
                    paths.SetTrainedNetPath(enteredPath);
                else if (enteredCommand == commands.SetLogPath)
                    paths.SetLogPath(enteredPath);
                else if (enteredCommand == commands.SetSampleSetPath)
                    paths.SetSampleSetPath(enteredPath);
            }

            await ExecuteConsoleCommands();
        }

        private static async Task TrainAsync()
        {
            stopwatch.Reset();
            stopwatch.Start();
            await initializer.TrainAsync();
            stopwatch.Stop();

            await initializer.SaveTrainedNetAsync();
        }

        private static void ShowSettings()
        {
            // Prevent double output about 'X' is null (from initlializer property) plus 'X' is unset here
            // by deactivating the event handling method temporarily.
            isInitializerStatusChangedEventActive = false;

            Console.WriteLine("\n" +
                $"     Current Settings:\n\n" +
                $"     General path    : {(paths.General == default ? " - " : paths.General)}\n" +
                $"     General prefix  : {(paths.FileName_Prefix == default ? " - " : paths.FileName_Prefix)}\n" +
                $"     General suffix  : {(paths.FileName_Suffix == default ? " - " : paths.FileName_Suffix)}\n\n" +

                $"     Path to sample set parameters    : {(paths.SampleSetParameters == default ? " - " : paths.SampleSetParameters)}\n" +
                $"     Path to net parameters           : {(paths.NetParameters == default ? " - " : paths.NetParameters)}\n" +
                $"     Path to trainer parameters       : {(paths.TrainerParameters == default ? " - " : paths.TrainerParameters)}\n" +
                $"     Path to sample set               : {(paths.SampleSet == default ? " - " : paths.SampleSet)}\n" +
                $"     Path to initialized net          : {(paths.InitializedNet == default ? " - " : paths.InitializedNet)}\n" +
                $"     Path to trained net              : {(paths.TrainedNet == default ? " - " : paths.TrainedNet)}\n" +
                $"     Path to log file                 : {(paths.Log == default ? " - " : paths.Log)}\n\n" +

                $"     Sample Set Parameters : {(parameters.SampleSetParameters == null ? " - " : $"set (Name: {parameters.SampleSetParameters.Name})")}\n" +
                $"     Net Parameters        : {(parameters.NetParameters == null ? " - " : "set")}\n" + 
                $"     Trainer Parameters    : {(parameters.TrainerParameters == null ? " - " : "set")}\n" +
                $"     Sample Set            : {(initializer.SampleSet == null ? " - " : "set")}\n" +
                $"     Net                   : {(initializer.Net == null ? " - " : "set")}\n" + 
                $"     Trainer               : {(initializer.Trainer == null ? " - " : "set")}\n\n" +

                $"     Available sample set templates: {parameters.SampleSetTemplates.ToStringFromCollection()}\n" +
                $"     Logging is {(initializer.IsLogged ? "on." : "off.")}\n\n");

            // Reactivate the event handling method again.
            isInitializerStatusChangedEventActive = true;
        }
        private static void ShowHelp()
        {
            // wa load/save trainer?
            Console.WriteLine("\n" +
                $"     All Commands: \n\n" +
                $"     Set general path                                 : {commands.SetGeneralPath}=[general path]\n" +
                $"     Set general prefix for file names                : {commands.SetFileNamePrefix}=[general prefix]\n" +
                $"     Set general suffix for file names                : {commands.SetFileNameSuffix}=[general suffix]\n" +
                $"     Set path to initialized net                      : {commands.SetInitializedNetPath}=[path to initialized net]\n" +
                $"     Set path to trained net                          : {commands.SetTrainedNetPath}=[path to trained net]\n" +
                $"     Set path to sample set                           : {commands.SetSampleSetPath}=[path to sample set]\n" +
                $"     Set path to net parameters                       : {commands.SetNetParametersPath}=[path to net parameters]\n" +
                $"     Set path to trainer parameters                   : {commands.SetTrainerParametersPath}=[path to trainer parameters]\n" +
                $"     Set path to sample set parameters                : {commands.SetSampleSetParametersPath}=[path to sample set parameters]\n" +
                $"     Set path to log file                             : {commands.SetLogPath}=[path to log file]\n" +
                $"     Use general path for all files and default names : {commands.UseGeneralPathAndDefaultNames}\n" +
                $"     Reset general path and use default names         : {commands.ResetPaths}\n\n" +

                $"     Load sample set parameters          : {commands.LoadSampleSetParameters}\n" +
                $"     Load net parameters                 : {commands.LoadNetParameters}\n" +
                $"     Load trainer parameters             : {commands.LoadTrainerParameters}\n" +
                $"     Load initialized net                : {commands.LoadInitializedNet}\n" +
                $"     Load trained net                    : {commands.LoadTrainedNet}\n" +
                $"     Load sample set                     : {commands.LoadSampleSet}\n\n" +

                $"     Create sample set parameters        : {commands.CreateSampleSetParameters}\n" +
                $"     Create named sample set parameters  : {commands.CreateSampleSetParameters} [template name]\n" +
                $"     Create the net parameters           : {commands.CreateNetParameters}\n" +
                $"     Create the trainer parameters       : {commands.CreateTrainerParameters}\n" +
                $"     Create sample set                   : {commands.CreateSampleSet}\n" +
                $"     Create the net                      : {commands.CreateNet}\n" +
                $"     Create the trainer                  : {commands.CreateTrainer}\n\n" +
                                                           
                $"     Save sample set parameters          : {commands.SaveSampleSetParameters}\n" +
                $"     Save net parameters                 : {commands.SaveNetParameters}\n" +
                $"     Save trainer parameters             : {commands.SaveTrainerParameters}\n" +
                $"     Save sample set                     : {commands.SaveSampleSet}\n" +
                $"     Save initialized net                : {commands.SaveInitializedNet}\n" +
                $"     Save trained net                    : {commands.SaveTrainedNet}\n\n" +
                                                           
                $"     Show Settings                       : {commands.ShowSettings}\n" +
                $"     Show this help                      : {commands.ShowHelp}\n" +
                $"     Show net parameters                 : {commands.ShowNetParameters}\n" +
                $"     Show trainer parameters             : {commands.ShowTrainerParameters}\n" +
                $"     Show sample set parameters          : {commands.ShowSampleSetParameters}\n\n" +
                                                           
                $"     Deactivate logging                  : {commands.Unlog}\n" +
                $"     Start test training                 : {commands.TestTraining}\n" +
                $"     Start training                      : {commands.Train}\n\n");
        }
        private static void ShowNetParameters()
        {
            if (parameters.NetParameters == null || parameters.LayerParametersCollection == null)   // Always create (empty or default?) LayerParametersCollection when creating NetParameters! That also lets you remove the 2nd check here'!
                return;

            Console.WriteLine("\n" +
                $"     Layers         : {parameters.LayerParametersCollection.Count}\n" +
                $"     WeightInitType : {parameters.NetParameters.WeightInitType}\n");
            
            foreach (var lp in parameters.NetParameters.LayerParametersCollection)
            {
                Console.WriteLine($"\n     Layer {lp.Id}: N={lp.NeuronsPerLayer}, wMin={lp.WeightMin}/wMax={lp.WeightMax}, bMin={lp.BiasMin}/bMax={lp.BiasMax}, Act={lp.ActivationType}");
            }

            Console.WriteLine();
        }
        private static void ShowTrainerParameters()
        {
            if (parameters.TrainerParameters == null)
                return;

            Console.WriteLine("\n" +
                $"     Learning Rate        : {parameters.TrainerParameters.LearningRate}\n" +
                $"     Learning Rate Change : {parameters.TrainerParameters.LearningRateChange}\n" +
                $"     Epochs               : {parameters.TrainerParameters.Epochs}\n" +
                $"     Cost Type            : {parameters.TrainerParameters.CostType}\n\n");
        }
        private static void ShowSampleSetParameters()
        {
            if (parameters.SampleSetParameters == null)
                return;

            Console.WriteLine("\n" +
                $"     Name                                : {parameters.SampleSetParameters.Name}\n" +
                $"     DefaultTestingSamples               : {parameters.SampleSetParameters.AllTestingSamples}\n" +
                $"     DefaultTrainingSamples              : {parameters.SampleSetParameters.AllTrainingSamples}\n" +
                $"     TestingSamples                      : {parameters.SampleSetParameters.TestingSamples}\n" +
                $"     TrainingSamples                     : {parameters.SampleSetParameters.TrainingSamples}\n" +
                $"     InputDistortion                     : {parameters.SampleSetParameters.InputDistortion}\n" +
                $"     TargetTolerance                     : {parameters.SampleSetParameters.TargetTolerance}\n");
            foreach (var path in parameters.SampleSetParameters.Paths)   // Always create (empty or default?) SampleSetParameters.Paths when creating SampleSetParameters!
            {
                Console.WriteLine($"     {path.Key, -20}: {path.Value}");
            }
            Console.WriteLine();
        }

        private static void Log()
        {
            initializer.IsLogged = true;
            Console.WriteLine("Logging activated.");
        }
        private static void Unlog()
        {
            initializer.IsLogged = false;
            Console.WriteLine("Logging deactivated.");
        }
        private async static Task TestTraining()
        {
            paths.General = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\";
            paths.FileName_Prefix = @"Test\";
            paths.FileName_Suffix = "_test.txt";
            paths.ResetPaths();

            if (!await parameters.LoadNetParametersAsync())
                return;
            if (!await parameters.LoadTrainerParametersAsync())
                return;
            if (!await initializer.LoadInitializedNetAsync())
                return;        // Always check if the loaded initialized net suits loaded parameters!

            if (!await initializer.LoadSampleSetAsync())
                return;             // Always check if the loaded sample set suits the ... parameters!
            if (!await initializer.CreateNetAsync())
                return;
            if (!await initializer.CreateTrainerAsync())
                return;
            initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;

            await TrainAsync();
        }

        #region helpers

        private static bool AnalyzeInput(string consoleInput, out string command, out string path, out string paramName, out string paramValue, out string layerId)
        {
            command = null;
            path = null;
            paramName = null;
            paramValue = null;
            layerId = null;

            try
            {
                if (!consoleInput.Contains('=') && !consoleInput.Contains(':'))
                {
                    command = consoleInput;
                }
                else if (consoleInput.Contains('=') && !consoleInput.Contains(':'))
                {
                    if (consoleInput.Where(x => x == '=').Count() > 1)
                        throw new ArgumentException("No more than one '=' allowed per command!");

                    var tmp = consoleInput.Split('=');
                    command = tmp.First();
                    path = tmp.Last();
                }
                else if (!consoleInput.Contains('=') && consoleInput.Contains(':'))
                {
                    if (consoleInput.Where(x => x == ':').Count() > 1)
                        throw new ArgumentException("No more than one ':' allowed per command!");

                    var tmp = consoleInput.Split(':');
                    command = commands.ChangeANetParameter;
                    paramName = tmp.First();
                    paramValue = tmp.Last();

                    if (command.Contains(nameof(commands.ChangeALayerParameter)))
                    {
                        layerId = command.Substring(0, nameof(commands.ChangeALayerParameter).Length);
                        command.Replace(layerId, "");
                    }
                }
                else if (consoleInput.Contains('=') && consoleInput.Contains(':'))
                {
                    throw new ArgumentException("'=' AND ':' in one command are not allowed!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        #endregion

        #endregion

        #region event handling methods

        private static void Initializer_StatusChanged_EventHandlingMethod(object initializer, InitializerStatusChangedEventArgs e)
        {
            if(isInitializerStatusChangedEventActive)
                Console.WriteLine($"{e.Info}");
        }
        private static void Trainer_StatusChanged_EventHandlingMethod(object trainer, TrainerStatusChangedEventArgs e)
        {
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds,10}: {e.Info}");
        }

        #endregion
    }
}