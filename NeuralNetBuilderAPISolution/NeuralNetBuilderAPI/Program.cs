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

        private static INetParameters netParameters;
        private static ITrainerParameters trainerParameters;
        private static INet net, trainedNet;
        private static ITrainer trainer;
        private static ISampleSet sampleSet;
        private static string parametersPath;
        private static string logPath;

        private const string commandName_ParametersPath = "parameters";
        //private const string netPath = nameof(netPath);
        //private const string trainerPath = nameof(trainerPath);
        private const string commandName_LogPath = "log";
        private const string commandName_Train = "train";
        private const string commandName_Help = "help";

        #endregion

        static async Task Main(string[] args)
        {
            ShowHelp();

            await GetConsoleInput();

            
        }

        private static async Task GetConsoleInput()
        {
            ShowCurrentSettings();

            string consoleInput = Console.ReadLine();
            string enteredCommand = consoleInput.Split('=').First();
            string enteredPath = consoleInput.Contains('=') ? consoleInput.Split('=').Last() : default;


            if (enteredPath == default)
            {
                if (enteredCommand == commandName_Train)
                    try
                    {
                        await LoadParametersAsync(parametersPath);
                        net = await InitializeNetAsync();
                        await InitializeTrainerAsync(net);
                        await TrainAsync(trainerParameters.Epochs, logPath);
                        return;
                    }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                else
                    Console.WriteLine("Unkown Command.\n");
            }
            else
            {
                if (File.Exists(enteredPath))
                {
                    if (enteredCommand == commandName_ParametersPath)
                        parametersPath = enteredPath;
                    else if (enteredCommand == commandName_LogPath)
                        logPath = enteredPath;
                }
                else { Console.WriteLine($"Cannot find file {enteredPath}\n"); }
            }

            await GetConsoleInput();
        }

        public static async Task LoadParametersAsync(string path)
        {
            await Task.Run(() =>
            {
                try
                {
                    var jasonParams = File.ReadAllText(path);
                    ISerializedParameters sp = JsonConvert.DeserializeObject<ISerializedParameters>(jasonParams);
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
                            $"Path to logging output: {logPath}\n");
        }
        private static void ShowHelp()
        {
            Console.WriteLine(
                            $"Set path to serialized parameters: {commandName_ParametersPath}=[path to serialized parameters]\n" +
                            $"Set path to logging output: {commandName_LogPath}=[path to logging output]\n" +
                            $"Start training: {commandName_Train}\n" +
                            $"Show this help: {commandName_Help}\n\n");
        }

        #endregion
    }
}
