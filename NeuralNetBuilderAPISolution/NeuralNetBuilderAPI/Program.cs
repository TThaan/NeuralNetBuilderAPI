using NeuralNetBuilder;
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

        private static Initializer initializer;
        private static Stopwatch stopwatch = new Stopwatch();

        private const string commandName_SetInitializedNetPath = "net path -0";
        private const string commandName_SetTrainedNetPath = "net path -1";
        private const string commandName_SetSampleSetPath = "samples path";
        private const string commandName_SetNetAndTrainerParametersPath = "net path -p";
        private const string commandName_SetSampleSetParametersPath = "samples path -p";
        private const string commandName_Unlog = "unlog";
        private const string commandName_SetLogPath = "log";
        private const string commandName_Status = "status";
        private const string commandName_Help = "help";
        private const string commandName_InitializeNet = "init net";
        private const string commandName_InitializeTrainer = "init trainer";
        private const string commandName_CreateSampleSet = "create samples";
        private const string commandName_LoadInitializedNet = "load net -0";
        private const string commandName_LoadTrainedNet = "load net -1";
        private const string commandName_LoadSampleSet = "load samples";
        private const string commandName_LoadNetAndTrainerParameters = "load net -p";
        private const string commandName_LoadSampleSetParameters = "load samples -p";
        private const string commandName_SaveInitializedNet = "save net -0";
        private const string commandName_SaveTrainedNet = "save net -1";
        private const string commandName_SaveSampleSet = "save samples";
        private const string commandName_Train = "train";
        private const string commandName_TestTraining = "test";

        #endregion

        #region important methods

        static async Task Main(string[] args)
        {
            initializer = new Initializer();
            initializer.InitializerStatusChanged += Initializer_StatusChanged_EventHandlingMethod;

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
                else if (enteredCommand == commandName_TestTraining)
                    await TestTraining();
                else if (enteredCommand == commandName_LoadNetAndTrainerParameters)
                    await initializer.LoadNetAndTrainerParametersAsync();
                else if (enteredCommand == commandName_LoadSampleSetParameters)
                    await initializer.LoadSampleSetParametersAsync();
                else if (enteredCommand == commandName_LoadInitializedNet)
                    await initializer.LoadInitializedNetAsync();
                else if (enteredCommand == commandName_LoadTrainedNet)
                    await initializer.LoadTrainedNetAsync();
                else if (enteredCommand == commandName_LoadSampleSet)
                    await initializer.LoadSampleSetAsync();
                else if (enteredCommand == commandName_InitializeNet)
                    await initializer.InitializeNetAsync();
                else if (enteredCommand == commandName_InitializeTrainer)
                {
                    if (await initializer.CreateTrainerAsync())
                        initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;
                }
                else if (enteredCommand == commandName_CreateSampleSet)
                    await initializer.CreateSampleSetAsync();
                else if (enteredCommand == commandName_Train)
                    await TrainAsync();
                else if (enteredCommand == commandName_SaveInitializedNet)
                    await initializer.SaveInitializedNetAsync();
                else if (enteredCommand == commandName_SaveTrainedNet)
                    await initializer.SaveTrainedNetAsync();
                else if (enteredCommand == commandName_SaveSampleSet)
                    await initializer.SaveSampleSetAsync();
                else
                    Console.WriteLine("Unkown Command.");
            }
            else
            {
                if (File.Exists(enteredPath))
                {
                    if (enteredCommand == commandName_SetNetAndTrainerParametersPath)
                        SetNetAndTrainerParametersPath(enteredPath);
                    else if (enteredCommand == commandName_SetSampleSetParametersPath)
                        SetSampleSetParametersPath(enteredPath);
                    else if (enteredCommand == commandName_SetInitializedNetPath)
                        SetInitializedNetPath(enteredPath);
                    else if (enteredCommand == commandName_SetTrainedNetPath)
                        SetTrainedNetPath(enteredPath);
                    else if (enteredCommand == commandName_SetLogPath)
                        SetLogPath(enteredPath);
                    else if (enteredCommand == commandName_SetSampleSetPath)
                        SetSampleSetPath(enteredPath);
                }
                else { Console.WriteLine($"Cannot find file {enteredPath}"); }
            }

            await ExecuteConsoleCommands();
        }
        private static async Task TrainAsync()
        {
            stopwatch.Reset();
            stopwatch.Start();
            await initializer.TrainAsync();
            stopwatch.Stop();
        }
        private static void ShowStatus()
        {
            Console.WriteLine("\n" +
                            //$"  Current Settings\n" +
                            $"  Path to net & trainer parameters is {(initializer.NetAndTrainerParametersPath == default ? "unset." : initializer.NetAndTrainerParametersPath)}\n" +
                            $"  Path to sample set parameters is {(initializer.SampleSetParametersPath == default ? "unset." : initializer.SampleSetParametersPath)}\n" +
                            $"  Path to initialized net is {(initializer.InitializedNetPath == default ? "unset." : initializer.InitializedNetPath)}\n" +
                            $"  Path to trained net is {(initializer.TrainedNetPath == default ? "unset." : initializer.TrainedNetPath)}\n" +
                            $"  Path to sample set is {(initializer.SampleSetPath == default ? "unset." : initializer.SampleSetPath)}\n" +
                            $"  Path to log file is {(initializer.LogPath == default ? "unset." : initializer.LogPath)}\n\n" +
                            $"  Logging is {(initializer.LogPath == default ? "deactivated." : "activated.")}\n" +
                            $"  Sample Set Parameters are {(initializer?.SampleSetParameters == null ? "unset." : "set.")}\n" +
                            $"  Sample Set is {(initializer?.SampleSet == null ? "unset." : "set.")}\n" +
                            $"  Net Parameters are {(initializer?.NetParameters == null ? "unset." : "set.")}\n" +
                            $"  Trainer Parameters are {(initializer?.TrainerParameters == null ? "unset." : "set.")}\n" +
                            $"  Net is {(initializer?.Net == null ? "raw." : "initialized.")}\n" +
                            $"  Trainer is {(initializer?.Trainer == null ? "raw." : "initialized.")}\n");
        }
        private static void ShowHelp()
        {
            Console.WriteLine("\n" +
                            $"  Set path to initialized net          : {commandName_SetInitializedNetPath}=[path to initialized net]\n" +
                            $"  Set path to trained net              : {commandName_SetTrainedNetPath}=[path to trained net]\n" +
                            $"  Set path to sample set               : {commandName_SetSampleSetPath}=[path to sample set]\n" +
                            $"  Set path to net & trainer parameters : {commandName_SetNetAndTrainerParametersPath}=[path to net & trainer parameters]\n" +
                            $"  Set path to sample set parameters    : {commandName_SetSampleSetParametersPath}=[path to sample set parameters]\n" +
                            $"  Load sample set parameters from file : {commandName_LoadSampleSetParameters}\n" +
                            $"  Create sample set                    : {commandName_CreateSampleSet}\n" +
                            $"  Load initialized net from file       : {commandName_LoadInitializedNet}\n" +
                            $"  Load trained net from file           : {commandName_LoadTrainedNet}\n" +
                            $"  Load sample set from file            : {commandName_LoadSampleSet}\n" +
                            $"  Initialize the net                   : {commandName_InitializeNet}\n" +
                            $"  Initialize the trainer               : {commandName_InitializeTrainer}\n" +
                            $"  Save initialized net to file         : {commandName_SaveInitializedNet}\n" +
                            $"  Save trained net to file             : {commandName_SaveTrainedNet}\n" +
                            $"  Save sample set to file              : {commandName_SaveSampleSet}\n" +
                            $"  Set path to log file                 : {commandName_SetLogPath}=[path to log file]\n" +
                            $"  Deactivate logging                   : {commandName_Unlog}\n" +
                            $"  Start training                       : {commandName_Train}\n" +
                            $"  Show Status                          : {commandName_Status}\n" +
                            $"  Show this help                       : {commandName_Help}\n");
        }

        #endregion

        #region single purpose methods

        private static void Unlog()
        {
            initializer.LogPath = default;
            Console.WriteLine("Logging deactivated.");
        }
        private static void SetInitializedNetPath(string path)
        {
            initializer.InitializedNetPath = path;
            Console.WriteLine("Path to the initialized net is set.");
        }
        private static void SetTrainedNetPath(string path)
        {
            initializer.TrainedNetPath = path;
            Console.WriteLine("Path to the trained net is set.");
        }
        private static void SetSampleSetPath(string path)
        {
            initializer.SampleSetPath = path;
            Console.WriteLine("Path to the sample set is set.");
        }
        private static void SetNetAndTrainerParametersPath(string path)
        {
            initializer.NetAndTrainerParametersPath = path;
            Console.WriteLine("Path to parameters for net and trainer is set.");
        }
        private static void SetSampleSetParametersPath(string path)
        {
            initializer.SampleSetParametersPath = path;
            Console.WriteLine("Path to parameters for the sample set is set.");
        }
        private static void SetLogPath(string path)
        {
            initializer.LogPath = path;
            Console.WriteLine("Path to the log file is set.");
        }

        #endregion

        #region combined method calls

        private async static Task TestTraining()
        {
            await initializer.LoadNetAndTrainerParametersAsync();
            await initializer.LoadInitializedNetAsync();
            await initializer.LoadSampleSetAsync();
            await initializer.CreateTrainerAsync();
            initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;

            await TrainAsync();
        }

        #endregion

        #region event handling methods

        private static void Initializer_StatusChanged_EventHandlingMethod(object initializer, InitializerStatusChangedEventArgs e)
        {
            Console.WriteLine($"{e.Info}");
        }
        private static void Trainer_StatusChanged_EventHandlingMethod(object trainer, TrainerStatusChangedEventArgs e)
        {
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds,10}: {e.Info}");
        }

        #endregion
    }
}
