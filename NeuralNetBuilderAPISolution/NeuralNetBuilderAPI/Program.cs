using DeepLearningDataProvider;
using DeepLearningDataProvider.Builders;
using NeuralNetBuilder;
using NeuralNetBuilder.Builders;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NeuralNetBuilderAPI
{
    public class Program
    {
        #region fields

        private static NeuralNetBuilder.Initializer initializer;
        private static NeuralNetBuilder.Builders.PathBuilder pathBuilder;
        private static ParameterBuilder paramBuilder;

        private static DeepLearningDataProvider.Initializer samplesInitializer;
        private static DeepLearningDataProvider.Builders.PathBuilder samplesPathBuilder;
        //private static SampleSetParameterBuilder sampleParamBuilder;

        private static Stopwatch stopwatch = new Stopwatch();
        private static CommandNames commands;
        private static string commandsPath = AppDomain.CurrentDomain.BaseDirectory + @"\CommandNames.txt";
        private static bool isInitializerStatusChangedEventActive = true;
        private static bool isDataProviderChangedEventActive = true;

        #endregion

        #region methods

        static async Task Main(string[] args)
        {
            initializer = new NeuralNetBuilder.Initializer();
            initializer.InitializerStatusChanged += NetbuilderChanged_EventHandlingMethod;
            pathBuilder = initializer.Paths;
            
            paramBuilder = initializer.ParameterBuilder;

            samplesInitializer = new DeepLearningDataProvider.Initializer();
            samplesInitializer.DataProviderChanged += DataProviderChanged_EventHandlingMethod;
            samplesPathBuilder = samplesInitializer.PathBuilder;
            samplesPathBuilder.SetSampleSetPath(Path.Combine(@"C:\Users\Jan_PC\Desktop\FourPixCam\", "Samples.csv"));
            //sampleParamBuilder = samplesInitializer.ParameterBuilder;

            commands = CommandNames.GetDefaultCommandNames();

            pathBuilder.ResetPaths();
            ShowHelp();
            ShowSettings();

            await ExecuteConsoleCommands();
        }

        private static async Task ExecuteConsoleCommands()
        {
            Console.WriteLine();
            string consoleInput = Console.ReadLine();
            
            AnalyzeInput(consoleInput, out string enteredCommand, out string enteredPath, out string enteredParameterName, out string enteredParameterValue, out string layerId);

            if (enteredPath == null)
            {
                #region Show

                if (enteredCommand == commands.ShowHelp)
                    ShowHelp();
                else if (enteredCommand == commands.ShowSettings)
                    ShowSettings();
                else if (enteredCommand == commands.ShowNetParameters)
                    ShowNetParameters();
                else if (enteredCommand == commands.ShowTrainerParameters)
                    ShowTrainerParameters();
                //else if (enteredCommand == commands.ShowSampleSetParameters)
                //    ShowSampleSetParameters();
                else if (enteredCommand == commands.ShowAllParameters)
                    ShowAllParameters();

                #endregion

                #region Load

                else if (enteredCommand == commands.LoadAllParameters)
                    await LoadAllParametersAsync();
                //else if (enteredCommand == commands.LoadSampleSetParameters)
                //    await sampleParamBuilder.LoadSampleSetParametersAsync();
                else if (enteredCommand == commands.LoadNetParameters)
                    await paramBuilder.LoadNetParametersAsync();
                else if (enteredCommand == commands.LoadTrainerParameters)
                    await paramBuilder.LoadTrainerParametersAsync();
                else if (enteredCommand == commands.LoadSamplesNetAndTrainer)
                    await LoadSamplesNetAndTrainerAsync();
                else if (enteredCommand == commands.LoadSampleSet)
                    await samplesInitializer.GetSampleSetAsync(samplesPathBuilder.SampleSet, .01f, 0);
                    //await samplesInitializer.LoadSampleSetViaMLNetAsync(samplesPathBuilder.SampleSet, .01f, 0);    // Task: dynamize testSamplesFaction!
                else if (enteredCommand == commands.LoadInitializedNet)
                    await initializer.LoadInitializedNetAsync();
                else if (enteredCommand == commands.LoadTrainedNet)
                    await initializer.LoadTrainedNetAsync();

                #endregion

                #region Create

                else if (enteredCommand == commands.CreateAllParameters)
                    CreateAllParameters(enteredParameterValue);
                //else if (enteredCommand == commands.CreateSampleSetParameters)
                //    sampleParamBuilder.CreateSampleSetParameters(enteredParameterValue);
                else if (enteredCommand == commands.CreateNetParameters)
                    paramBuilder.CreateNetParameters();
                else if (enteredCommand == commands.CreateTrainerParameters)
                    paramBuilder.CreateTrainerParameters();
                else if (enteredCommand == commands.CreateSamplesNetAndTrainer)
                    await CreateSamplesNetAndTrainerAsync();
                //else if (enteredCommand == commands.CreateSampleSet)
                //    await samplesInitializer.CreateSampleSetAsync();
                else if (enteredCommand == commands.CreateNet)
                    await initializer.CreateNetAsync();
                else if (enteredCommand == commands.CreateTrainer)
                {
                    if (await initializer.CreateTrainerAsync(samplesInitializer.SampleSet))
                        initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;
                }

                #endregion

                #region Save

                else if (enteredCommand == commands.SaveAllParameters)
                    await SaveAllParametersAsync();
                //else if (enteredCommand == commands.SaveSampleSetParameters)
                //    await sampleParamBuilder.SaveSampleSetParametersAsync();
                else if (enteredCommand == commands.SaveNetParameters)
                    await paramBuilder.SaveNetParametersAsync();
                else if (enteredCommand == commands.SaveTrainerParameters)
                    await paramBuilder.SaveTrainerParametersAsync();
                else if (enteredCommand == commands.SaveSamplesNetAndTrainer)
                    await SaveSamplesNetAndTrainerAsync();
                else if (enteredCommand == commands.SaveSampleSet)
                    await samplesInitializer.SaveSampleSetAsync();
                else if (enteredCommand == commands.SaveInitializedNet)
                    await initializer.SaveInitializedNetAsync();
                else if (enteredCommand == commands.SaveTrainedNet)
                    await initializer.SaveTrainedNetAsync();

                #endregion

                #region Change parameter

                else if (enteredCommand == commands.ChangeParameter)
                {
                    //sampleParamBuilder.ChangeParameter(enteredParameterName, enteredParameterValue);
                    paramBuilder.ChangeParameter(enteredParameterName, enteredParameterValue, layerId);
                }

                #endregion

                #region Misc

                else if (enteredCommand == commands.Log)
                    Log();
                else if (enteredCommand == commands.Unlog)
                    Unlog();
                else if (enteredCommand == commands.TestTraining)
                    await TestTraining();
                else if (enteredCommand == commands.Train)
                    await TrainAsync();
                else if (enteredCommand == commands.ResetPaths)
                    pathBuilder.ResetPaths();
                else if (enteredCommand == commands.UseGeneralPathAndDefaultNames)
                    pathBuilder.UseGeneralPathAndDefaultNames();

                else
                    Console.WriteLine("Unkown Command.");

                #endregion
            }
            else
            {
                #region Set path

                if (enteredCommand == commands.SetGeneralPath)
                    pathBuilder.SetGeneralPath(enteredPath);
                else if (enteredCommand == commands.SetFileNamePrefix)
                    pathBuilder.SetFileNamePrefix(enteredPath);
                else if (enteredCommand == commands.SetFileNameSuffix)
                    pathBuilder.SetFileNameSuffix(enteredPath);
                else if (enteredCommand == commands.SetNetParametersPath)
                    pathBuilder.SetNetParametersPath(enteredPath);
                else if (enteredCommand == commands.SetNetParametersPath)
                    pathBuilder.SetNetParametersPath(enteredPath);
                //else if (enteredCommand == commands.SetSampleSetParametersPath)
                //    samplesPathBuilder.SetSampleSetParametersPath(enteredPath);
                else if (enteredCommand == commands.SetInitializedNetPath)
                    pathBuilder.SetInitializedNetPath(enteredPath);
                else if (enteredCommand == commands.SetTrainedNetPath)
                    pathBuilder.SetTrainedNetPath(enteredPath);
                else if (enteredCommand == commands.SetLogPath)
                    pathBuilder.SetLogPath(enteredPath);
                else if (enteredCommand == commands.SetSampleSetPath)
                    samplesPathBuilder.SetSampleSetPath(enteredPath);

                #endregion
            }

            await ExecuteConsoleCommands();
        }

        #region combining methods

        public static async Task<bool> CreateSamplesNetAndTrainerAsync()
        {
            //if (await samplesInitializer.CreateSampleSetAsync() == false)
            //    return false;
            if (await initializer.CreateNetAsync() == false)
                return false;
            if (await initializer.CreateTrainerAsync(samplesInitializer.SampleSet) == false)
                return false;

            return true;
        }
        public static bool CreateAllParameters(string templateName)
        {
            //if (sampleParamBuilder.CreateSampleSetParameters(templateName) == false)
            //    return false;

            paramBuilder.CreateNetParameters();
            paramBuilder.CreateTrainerParameters();

            return true;
        }
        public static async Task<bool> LoadAllParametersAsync()
        {
            bool result = true;

            //if (await sampleParamBuilder.LoadSampleSetParametersAsync() == false) result = false;
            if (await paramBuilder.LoadNetParametersAsync() == false) result = false;
            if (await paramBuilder.LoadTrainerParametersAsync() == false) result = false;

            return result;
        }
        public static async Task<bool> SaveAllParametersAsync()
        {
            bool result = true;

            //if (await sampleParamBuilder.SaveSampleSetParametersAsync() == false) result = false;
            if (await paramBuilder.SaveNetParametersAsync() == false) result = false;
            if (await paramBuilder.SaveTrainerParametersAsync() == false) result = false;

            return result;
        }
        public static async Task<bool> SaveSamplesNetAndTrainerAsync() // incl trained net but no trainer
        {
            if (await samplesInitializer.SaveSampleSetAsync() == false)
                return false;
            if (await initializer.SaveInitializedNetAsync() == false)
                return false;
            if (await initializer.SaveTrainedNetAsync() == false)
                return false;

            return true;
        }
        public static async Task<bool> LoadSamplesNetAndTrainerAsync() // incl trained net but no trainer
        {
            if (await samplesInitializer.GetSampleSetAsync(samplesPathBuilder.SampleSet, .01f, 0) == false)
                return false;
            if (await initializer.LoadInitializedNetAsync() == false)
                return false;
            if (await initializer.LoadTrainedNetAsync() == false)
                return false;

            return true;
        }

        #endregion

        private static async Task TrainAsync()
        {
            stopwatch.Reset();
            stopwatch.Start();
            await initializer.TrainAsync(samplesInitializer.SampleSet);
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
                $"     General path    : {(pathBuilder.General == default ? " - " : pathBuilder.General)}\n" +
                $"     General prefix  : {(pathBuilder.FileName_Prefix == default ? " - " : pathBuilder.FileName_Prefix)}\n" +
                $"     General suffix  : {(pathBuilder.FileName_Suffix == default ? " - " : pathBuilder.FileName_Suffix)}\n\n" +

                //$"     Path to sample set parameters    : {(samplesPathBuilder.SampleSetParameters == default ? " - " : samplesPathBuilder.SampleSetParameters)}\n" +
                $"     Path to net parameters           : {(pathBuilder.NetParameters == default ? " - " : pathBuilder.NetParameters)}\n" +
                $"     Path to trainer parameters       : {(pathBuilder.TrainerParameters == default ? " - " : pathBuilder.TrainerParameters)}\n" +
                $"     Path to sample set               : {(samplesPathBuilder.SampleSet == default ? " - " : samplesPathBuilder.SampleSet)}\n" +
                $"     Path to initialized net          : {(pathBuilder.InitializedNet == default ? " - " : pathBuilder.InitializedNet)}\n" +
                $"     Path to trained net              : {(pathBuilder.TrainedNet == default ? " - " : pathBuilder.TrainedNet)}\n" +
                $"     Path to log file                 : {(pathBuilder.Log == default ? " - " : pathBuilder.Log)}\n\n" +

                //$"     Sample Set Parameters : {(sampleParamBuilder.Parameters == null ? " - " : $"set (Name: {sampleParamBuilder.Parameters.Name})")}\n" +
                $"     Net Parameters        : {(paramBuilder.NetParameters == null ? " - " : "set")}\n" + 
                $"     Trainer Parameters    : {(paramBuilder.TrainerParameters == null ? " - " : "set")}\n" +
                $"     Sample Set            : {(samplesInitializer.SampleSet.TrainSet == null || samplesInitializer.SampleSet.TestSet == null ? " - " : "set")}\n" +
                $"     Net                   : {(initializer.Net == null ? " - " : "set")}\n" + 
                $"     Trainer               : {(initializer.Trainer == null ? " - " : "set")}\n\n" +

                //$"     Available sample set templates: {sampleParamBuilder.DefaultParameters.ToStringFromCollection()}\n" +
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
                //$"     Set path to sample set parameters                : {commands.SetSampleSetParametersPath}=[path to sample set parameters]\n" +
                $"     Set path to log file                             : {commands.SetLogPath}=[path to log file]\n" +
                $"     Use general path for all files and default names : {commands.UseGeneralPathAndDefaultNames}\n" +
                $"     Reset general path and use default names         : {commands.ResetPaths}\n\n" +

                $"     Load all parameters                 : {commands.LoadAllParameters}\n" +
                //$"     Load sample set parameters          : {commands.LoadSampleSetParameters}\n" +
                $"     Load net parameters                 : {commands.LoadNetParameters}\n" +
                $"     Load trainer parameters             : {commands.LoadTrainerParameters}\n" +
                $"     Load sample set, net & trainer      : {commands.LoadSamplesNetAndTrainer}\n" +
                $"     Load sample set                     : {commands.LoadSampleSet}\n" +
                $"     Load initialized net                : {commands.LoadInitializedNet}\n" +
                $"     Load trained net                    : {commands.LoadTrainedNet}\n\n" +

                // Implement/Check optionality
                $"     Create all parameters               : {commands.CreateAllParameters} [optional: template name]\n" +
                //$"     Create sample set parameters        : {commands.CreateSampleSetParameters}\n" +
                //$"     Create named sample set parameters  : {commands.CreateSampleSetParameters} [template name]\n" +
                $"     Create the net parameters           : {commands.CreateNetParameters}\n" +
                $"     Create the trainer parameters       : {commands.CreateTrainerParameters}\n" +
                $"     Create sample set, net & trainer    : {commands.CreateSamplesNetAndTrainer}\n" +
                $"     Create sample set                   : {commands.CreateSampleSet}\n" +
                $"     Create the net                      : {commands.CreateNet}\n" +
                $"     Create the trainer                  : {commands.CreateTrainer}\n\n" +

                $"     Save all parameters                 : {commands.SaveAllParameters}\n" +
                //$"     Save sample set parameters          : {commands.SaveSampleSetParameters}\n" +
                $"     Save net parameters                 : {commands.SaveNetParameters}\n" +
                $"     Save trainer parameters             : {commands.SaveTrainerParameters}\n" +
                $"     Save sample set, net & trainer      : {commands.SaveSamplesNetAndTrainer}\n" +
                $"     Save sample set                     : {commands.SaveSampleSet}\n" +
                $"     Save initialized net                : {commands.SaveInitializedNet}\n" +
                $"     Save trained net                    : {commands.SaveTrainedNet}\n\n" +
                
                $"     Show Settings                       : {commands.ShowSettings}\n" +
                $"     Show this help                      : {commands.ShowHelp}\n" +
                $"     Show all parameters                 : {commands.ShowAllParameters}\n" +
                $"     Show net parameters                 : {commands.ShowNetParameters}\n" +
                $"     Show trainer parameters             : {commands.ShowTrainerParameters}\n\n" +
                //$"     Show sample set parameters          : {commands.ShowSampleSetParameters}\n" +

                $"     Change parameter                          : {commands.ChangeParameter} [optional: layer id] [parameter name] [optional: :parameter value]\n" +
                $"     Example 1 (Add a new layer after layer 0) : {commands.ChangeParameter} 0 add\n" +
                $"     Example 2 (Set the global WeightMax 1)    : {commands.ChangeParameter} wMax:1\n" +
                $"     Example 3 (Set BiasMin of layer 3 to 2)   : {commands.ChangeParameter} bMin:2\n" +
                $"     Parameter Names                           : {paramBuilder.ParameterNames.Values.ToStringFromCollection()}\n\n" +

                $"     Deactivate logging                  : {commands.Unlog}\n" +
                $"     Start test training                 : {commands.TestTraining}\n" +
                $"     Start training                      : {commands.Train}\n\n");
        }
        private static void ShowAllParameters()
        {
            //ShowSampleSetParameters();
            ShowNetParameters();
            ShowTrainerParameters();
        }
        private static void ShowNetParameters()
        {
            if (paramBuilder.NetParameters == null || paramBuilder.LayerParametersCollection == null)   // Always create (empty or default?) LayerParametersCollection when creating NetParameters! That also lets you remove the 2nd check here'!
                return;

            Console.WriteLine("\n" +
                $"     Layers         : {paramBuilder.LayerParametersCollection.Count}\n" +
                $"     WeightInitType : {paramBuilder.NetParameters.WeightInitType}\n");
            
            foreach (var lp in paramBuilder.NetParameters.LayerParametersCollection)
            {
                Console.WriteLine($"\n     Layer {lp.Id}: N = {lp.NeuronsPerLayer}, wMin/wMax = {lp.WeightMin}/{lp.WeightMax}, bMin/bMax = {lp.BiasMin}/{lp.BiasMax}, Activation = {lp.ActivationType}");
            }

            Console.WriteLine();
        }
        private static void ShowTrainerParameters()
        {
            if (paramBuilder.TrainerParameters == null)
                return;

            Console.WriteLine("\n" +
                $"     Learning Rate        : {paramBuilder.TrainerParameters.LearningRate}\n" +
                $"     Learning Rate Change : {paramBuilder.TrainerParameters.LearningRateChange}\n" +
                $"     Epochs               : {paramBuilder.TrainerParameters.Epochs}\n" +
                $"     Cost Type            : {paramBuilder.TrainerParameters.CostType}\n\n");
        }
        //private static void ShowSampleSetParameters()
        //{
        //    if (sampleParamBuilder.Parameters == null)
        //        return;

        //    Console.WriteLine("\n" +
        //        $"     Name                                : {sampleParamBuilder.Parameters.Name}\n" +
        //        $"     DefaultTestingSamples               : {sampleParamBuilder.Parameters.AllTestingSamples}\n" +
        //        $"     DefaultTrainingSamples              : {sampleParamBuilder.Parameters.AllTrainingSamples}\n" +
        //        $"     TestingSamples                      : {sampleParamBuilder.Parameters.TestingSamples}\n" +
        //        $"     TrainingSamples                     : {sampleParamBuilder.Parameters.TrainingSamples}\n" +
        //        $"     InputDistortion                     : {sampleParamBuilder.Parameters.InputDistortion}\n" +
        //        $"     TargetTolerance                     : {sampleParamBuilder.Parameters.TargetTolerance}\n");
        //    foreach (var path in sampleParamBuilder.Parameters.Paths)   // Always create (empty or default?) SampleSetParameters.Paths when creating SampleSetParameters!
        //    {
        //        Console.WriteLine($"     {path.Key, -20}: {path.Value}");
        //    }
        //    Console.WriteLine();
        //}

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
            pathBuilder.General = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\";
            pathBuilder.FileName_Prefix = @"Test\";
            pathBuilder.FileName_Suffix = "_test.txt";
            pathBuilder.ResetPaths();

            if (!await paramBuilder.LoadNetParametersAsync())
                return;
            if (!await paramBuilder.LoadTrainerParametersAsync())
                return;
            if (!await initializer.LoadInitializedNetAsync())
                return;        // Always check if the loaded initialized net suits loaded parameters!

            if (!await samplesInitializer.GetSampleSetAsync(samplesPathBuilder.SampleSet, .01f, 0))
                return;             // Always check if the loaded sample set suits the ... parameters!
            if (!await initializer.CreateNetAsync())
                return;
            if (!await initializer.CreateTrainerAsync(samplesInitializer.SampleSet))
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
                if(string.Equals(new string(consoleInput.Take(commands.ChangeParameter.Length).ToArray()), commands.ChangeParameter))
                {
                    SetCommandAndLayerId(consoleInput, ref command, ref layerId);
                    var partialString = consoleInput.Remove(0, commands.ChangeParameter.Length + layerId.Length + 1);
                    if (partialString.Contains(':'))
                    {
                        paramName = partialString.Split(':').First();
                        paramValue = partialString.Split(':').Last();
                    }
                    else 
                    {
                        paramName = partialString;
                    }
                }
                else if (!consoleInput.Contains('=') && !consoleInput.Contains(':'))
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
                    command = commands.ChangeParameter;
                    paramName = tmp.First();
                    paramValue = tmp.Last();
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
        private static void SetCommandAndLayerId(string consoleInput, ref string command, ref string layerId)
        {
            // Get the word after the first space in the command string (ie after commands.ChangeParameter)
            command = commands.ChangeParameter;
            layerId = consoleInput.Split(' ').ElementAt(1);
        }

        #endregion

        #endregion

        #region event handling methods

        private static void NetbuilderChanged_EventHandlingMethod(object initializer, InitializerStatusChangedEventArgs e)
        {
            if(isInitializerStatusChangedEventActive)
                Console.WriteLine($"{e.Info}");
        }private static void DataProviderChanged_EventHandlingMethod(object initializer, DataProviderChangedEventArgs e)
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