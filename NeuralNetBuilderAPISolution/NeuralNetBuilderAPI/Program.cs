using DeepLearningDataProvider;
using NeuralNetBuilder;
using NeuralNetBuilder.FactoriesAndParameters;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// You can use this console API as a standalone to work with the NeuralNetBuilder
/// by just running it and using the console window or
/// you can use it like a library in a real GUI (e.g. like "AIDemoUI" does).
/// </summary>
namespace NeuralNetBuilderAPI
{
    public class Program
    {
        #region fields

        private static INetParameters netParameters;
        private static ITrainerParameters trainerParameters;
        private static INet net, trainedNet;
        private static ITrainer trainer;
        private static ISampleSet sampleSet;
        private static string netAndTrainerParametersPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_Parameters.txt";
        private static string logPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_Log.txt";
        private static string sampleSetParametersPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_SampleSetParameters.txt";
        private static string sampleSetPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_SampleSet.txt";
        private static string initializedNetPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_InitializedNet.txt";
        private static string trainedNetPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_TrainedNet.txt";

        private const string commandName_SetNetAndTrainerParametersPath = "np";
        private const string commandName_SetSampleSetParametersPath = "sp";
        private const string commandName_Unlog = "unlog";
        private const string commandName_SetLogPath = "log";
        private const string commandName_Status = "status";
        private const string commandName_Help = "help";
        private const string commandName_GetSampleSet = "getsamples";
        private const string commandName_Train = "train";

        private static Stopwatch stopwatch = new Stopwatch();

        #endregion

        static async Task Main(string[] args)
        {
            ShowHelp();
            ShowStatus();

            await ExecuteConsoleCommands();
        }

        private static async Task ExecuteConsoleCommands()
        {
            string consoleInput = Console.ReadLine();
            string enteredCommand = consoleInput.Split('=').First();
            string enteredPath = consoleInput.Contains('=') ? consoleInput.Split('=').Last() : default;

            if (enteredPath == default)
            {
                if (enteredCommand == commandName_Help)
                    ShowHelp();
                else if (enteredCommand == commandName_Status)
                    ShowStatus();
                else if (enteredCommand == commandName_Unlog)
                    Unlog();
                else if (enteredCommand == commandName_GetSampleSet)
                    sampleSet = await GetSampleSet(default);
                else if (enteredCommand == commandName_Train)
                    await TrainAsync();
                else
                    Console.WriteLine("Unkown Command.");
            }
            else
            {
                if (File.Exists(enteredPath))
                {
                    if (enteredCommand == commandName_SetNetAndTrainerParametersPath)
                        SetNetAndTrainerParametersPath(enteredPath);
                    else if (enteredCommand == commandName_SetLogPath)
                        SetLogPath(enteredPath);
                    else if (enteredCommand == commandName_SetSampleSetParametersPath)
                        SetSampleSetParametersPath(enteredPath);
                }
                else { Console.WriteLine($"Cannot find file {enteredPath}"); }
            }

            await ExecuteConsoleCommands();
        }
        private static async Task TrainAsync()
        {
            try
            {
                if (sampleSet == null)
                    sampleSet = await GetSampleSet(default);

                await LoadParametersAsync(netAndTrainerParametersPath);
                net = await InitializeNetAsync();
                await InitializeTrainerAsync(net);

                Console.WriteLine($"\n            Training, please wait...\n");

                stopwatch.Reset();
                stopwatch.Start();
                await trainer.Train(logPath, trainerParameters.Epochs);
                stopwatch.Stop();

                trainedNet = trainer.TrainedNet.GetCopy();
                Console.WriteLine($"\n            Finished training.\n");

                return;
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        #region helpers for method 'TrainAsync()'

        public static async Task LoadParametersAsync(string path)
        {
            await Task.Run(() =>
            {
                try
                {
                    var jasonParams = File.ReadAllText(path);
                    var sp = JsonConvert.DeserializeObject<SerializedParameters>(jasonParams);
                    netParameters = sp.NetParameters;
                    trainerParameters = sp.TrainerParameters;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"That didn't work.\n({e.Message})");
                    return;
                }
            });
        }
        public static async Task<INet> InitializeNetAsync()
        {
            INet rawNet = Initializer.GetRawNet();
            return await Task.Run(() => Initializer.InitializeNet(rawNet, netParameters));
        }
        public static async Task InitializeTrainerAsync(INet net)
        {
            ITrainer rawTrainer = Initializer.GetRawTrainer();
            trainer = await Task.Run(() => Initializer.InitializeTrainer(rawTrainer, net.GetCopy(), trainerParameters, sampleSet));
            trainer.TrainerStatusChanged += Trainer_StatusChanged;
        }
        public static async Task<ISampleSet> GetSampleSet(string path)
        {
            ISampleSet result = default;
            sampleSetParametersPath = path == default
                ? sampleSetParametersPath : path;

            try
            {
                var jsonString = File.ReadAllText(sampleSetParametersPath);
                var sampleSetParams = JsonConvert.DeserializeObject<SampleSetParameters>(jsonString);
                var sampleSetSteward = new SampleSetSteward();

                Console.WriteLine("\n            Loading samples, please wait...");
                result = await sampleSetSteward.CreateSampleSetAsync(sampleSetParams);
                Console.WriteLine("            Successfully loaded samples.\n");
            }
            catch (Exception e) { Console.WriteLine(e.Message); }

            return result;
        }

        #endregion

        #region helpers for method 'ExecuteConsoleCommands()'

        private static void ShowStatus()
        {
            Console.WriteLine("\n" +
                            //$"  Current Settings\n" +
                            $"  Path to serialized parameters is {(netAndTrainerParametersPath == default ? "unset." : netAndTrainerParametersPath)}\n" +
                            $"  Path to sample set parameters is {(sampleSetParametersPath == default ? "unset." : sampleSetParametersPath)}\n" +
                            $"  Path to logging output is {(logPath == default ? "unset." : logPath)}\n" +
                            $"  Logging is {(logPath == default ? "deactivated." : "activated.")}\n");
        }
        private static void ShowHelp()
        {
            Console.WriteLine("\n" +
                            $"  Set path to net & trainer parameters : {commandName_SetNetAndTrainerParametersPath}=[path to net & trainer parameters]\n" +
                            $"  Set path to sample set parameters    : {commandName_SetSampleSetParametersPath}=[path to sample set parameters]\n" +
                            $"  Get sample set                       : {commandName_GetSampleSet}=[path to sample set parameters]\n" +
                            $"  Get default sample set               : {commandName_GetSampleSet}\n" +
                            $"  Set path to log file                 : {commandName_SetLogPath}=[path to log file]\n" +
                            $"  Deactivate logging                   : {commandName_Unlog}\n" +
                            $"  Start training                       : {commandName_Train}\n" +
                            $"  Show Status                          : {commandName_Status}\n" +
                            $"  Show this help                       : {commandName_Help}\n");
        }
        private static void Unlog()
        {
            logPath = default;
            Console.WriteLine("Logging deactivated.");
        }
        private static void SetNetAndTrainerParametersPath(string path)
        {
            netAndTrainerParametersPath = path;
            Console.WriteLine("Path to parameters for net and trainer are set.");
        }
        private static void SetSampleSetParametersPath(string path)
        {
            sampleSetParametersPath = path;
            Console.WriteLine("Path to parameters for the sample set are set.");
        }
        private static void SetLogPath(string path)
        {
            logPath = path;
            Console.WriteLine("Path to the log file is set.");
        }

        #endregion

        #region event handling methods

        private static void Trainer_StatusChanged(object trainer, TrainerStatusChangedEventArgs e)
        {
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds, 10}: {e.Info}");
        }

        #endregion
    }
}
