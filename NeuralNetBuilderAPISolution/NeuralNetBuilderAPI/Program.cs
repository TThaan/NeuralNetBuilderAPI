using DeepLearningDataProvider;
using NeuralNetBuilder;
using NeuralNetBuilder.FactoriesAndParameters;
using Newtonsoft.Json;
using System;
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

        //private static JsonSerializerSettings jsonSerializerSettings;   // better in NetBuilder?
        private static INetParameters netParameters;
        private static ITrainerParameters trainerParameters;
        private static INet net, trainedNet;
        private static ITrainer trainer;
        private static ISampleSet sampleSet;
        private static string parametersPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_Parameters.txt";
        private static string logPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_Log.txt";
        private static string _sampleSetParametersPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_SampleSetParameters.txt";
        private static string sampleSetPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_SampleSet.txt";
        private static string initializedNetPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_InitializedNet.txt";
        private static string trainedNetPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\ConsoleApi_TrainedNet.txt";

        private const string commandName_SetParametersPath = "params";
        private const string commandName_GetSampleSet = "getsamples";
        private const string commandName_SetLogPath = "log";
        //private const string netPath = nameof(netPath);
        //private const string trainerPath = nameof(trainerPath);
        private const string commandName_Train = "train";
        private const string commandName_Help = "help";

        #endregion

        static async Task Main(string[] args)
        {
            // jsonSerializerSettings = GetJsonSerialzerSettings();
            ShowHelp();

            await ExecuteConsoleCommands();

            
        }

        //private static JsonSerializerSettings GetJsonSerialzerSettings()
        //{
        //    var result = new JsonSerializerSettings();

        //    var netParamsConverter = new NetParametersConverter();
        //    var trainerParamsConverter = new TrainerParametersConverter();
        //    result.Converters.Add(netParamsConverter);
        //    result.Converters.Add(trainerParamsConverter);

        //    return result;
        //}

        private static async Task ExecuteConsoleCommands()
        {
            ShowCurrentSettings();

            string consoleInput = Console.ReadLine();
            string enteredCommand = consoleInput.Split('=').First();
            string enteredPath = consoleInput.Contains('=') ? consoleInput.Split('=').Last() : default;


            if (enteredPath == default)
            {
                if (enteredCommand == commandName_Train)
                    await TrainAsync();
                if (enteredCommand == commandName_GetSampleSet)
                    sampleSet = await GetSampleSet(default);
                else if (enteredCommand == commandName_Help)
                    ShowHelp();
                else
                    Console.WriteLine("Unkown Command.\n");
            }
            else
            {
                if (File.Exists(enteredPath))
                {
                    if (enteredCommand == commandName_SetParametersPath)
                        parametersPath = enteredPath;
                    else if (enteredCommand == commandName_SetLogPath)
                        logPath = enteredPath;
                    else if (enteredCommand == commandName_GetSampleSet)
                        sampleSet = await GetSampleSet(enteredPath);
                }
                else { Console.WriteLine($"Cannot find file {enteredPath}\n"); }
            }

            await ExecuteConsoleCommands();
        }

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
        }
        static async Task<ISampleSet> GetSampleSet(string sampleSetParametersPath)
        {
            ISampleSet result = default;
            _sampleSetParametersPath = sampleSetParametersPath == default
                ? _sampleSetParametersPath : sampleSetParametersPath;

            try
            {
                var jsonString = File.ReadAllText(_sampleSetParametersPath);
                var sampleSetParams = JsonConvert.DeserializeObject<SampleSetParameters>(jsonString);
                var sampleSetSteward = new SampleSetSteward();

                Console.WriteLine("Loading samples, please wait...");
                result = await sampleSetSteward.CreateSampleSetAsync(sampleSetParams);
                Console.WriteLine("Successfully loaded samples.");
            }
            catch (Exception e) { Console.WriteLine(e.Message); }

            return result;
        }
        static async Task TrainAsync()
        {
            try
            {
                if (sampleSet == null)
                    sampleSet = await GetSampleSet(default);
                // throw new ArgumentException("You got no sample set yet!");

                await LoadParametersAsync(parametersPath);
                net = await InitializeNetAsync();
                await InitializeTrainerAsync(net);

                Console.WriteLine("Training, please wait...");
                await TrainAsync(trainerParameters.Epochs, logPath);
                Console.WriteLine("Finished training.");

                return;
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
        public static async Task TrainAsync(int epochs, string logName = default)
        {
            await trainer.Train(logName, epochs);
            trainedNet = trainer.TrainedNet.GetCopy();
        }

        #region helpers

        private static void ShowCurrentSettings()
        {
            Console.WriteLine(
                            $"\nCurrent Settings\n" +
                            $"Path to serialized parameters: {parametersPath}\n" +
                            $"Path to sample set parameters: {_sampleSetParametersPath}\n" +
                            $"Path to logging output: {logPath}\n");
        }
        private static void ShowHelp()
        {
            Console.WriteLine(
                            $"Set path to serialized parameters : {commandName_SetParametersPath}=[path to serialized parameters]\n" +
                            $"Set path to logging output        : {commandName_SetLogPath}=[path to logging output]\n" +
                            $"Get sample set                    : {commandName_GetSampleSet}=[path to SampleSetParameters]\n" +
                            $"Get default sample set            : {commandName_GetSampleSet}\n" +
                            $"Start training                    : {commandName_Train}\n" +
                            $"Show this help                    : {commandName_Help}\n\n");
        }

        #endregion
    }
}
