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

        private const string commandName_SetGeneralPath = "path";
        private const string commandName_SetFileNamePrefix = "path prefix";
        private const string commandName_SetFileNameSuffix = "path suffix";
        private const string commandName_SetAllPaths = "path all";
        private const string commandName_SetInitializedNetPath = "path net -0";
        private const string commandName_SetTrainedNetPath = "path net -1";
        private const string commandName_SetSampleSetPath = "path samples";
        private const string commandName_SetNetParametersPath = "path net -p";
        private const string commandName_SetTrainerParametersPath = "path trainer -p";
        private const string commandName_SetSampleSetParametersPath = "path samples -p";
        private const string commandName_SetLogPath = "path log";

        private const string commandName_Status = "status";
        private const string commandName_Help = "help";
        private const string commandName_Log = "log on";
        private const string commandName_Unlog = "log off";
        private const string commandName_InitializeNet = "init net";
        private const string commandName_InitializeTrainer = "init trainer";
        private const string commandName_CreateSampleSet = "create samples";
        private const string commandName_LoadInitializedNet = "load net -0";
        private const string commandName_LoadTrainedNet = "load net -1";
        private const string commandName_LoadSampleSet = "load samples";
        private const string commandName_LoadNetParameters = "load net -p";
        private const string commandName_LoadTrainerParameters = "load trainer -p";
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

            SetAllPaths();
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
                else if (enteredCommand == commandName_Log)
                    Log();
                else if (enteredCommand == commandName_Unlog)
                    Unlog();
                else if (enteredCommand == commandName_SetAllPaths)
                    SetAllPaths();
                else if (enteredCommand == commandName_TestTraining)
                    await TestTraining();
                else if (enteredCommand == commandName_LoadNetParameters)
                    await initializer.LoadNetParametersAsync();
                else if (enteredCommand == commandName_LoadTrainerParameters)
                    await initializer.LoadTrainerParametersAsync();
                else if (enteredCommand == commandName_LoadSampleSetParameters)
                    await initializer.LoadSampleSetParametersAsync();
                else if (enteredCommand == commandName_LoadInitializedNet)
                    await initializer.LoadInitializedNetAsync();
                else if (enteredCommand == commandName_LoadTrainedNet)
                    await initializer.LoadTrainedNetAsync();
                else if (enteredCommand == commandName_LoadSampleSet)
                    await initializer.LoadSampleSetAsync();
                else if (enteredCommand == commandName_InitializeNet)
                    await initializer.CreateNetAsync();
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
                if (enteredCommand == commandName_SetGeneralPath)
                    SetGeneralPath(enteredPath);
                else if (enteredCommand == commandName_SetFileNamePrefix)
                    SetFileNamePrefix(enteredPath);
                else if (enteredCommand == commandName_SetFileNameSuffix)
                    SetFileNameSuffix(enteredPath);
                else if (enteredCommand == commandName_SetNetParametersPath)
                    SetNetParametersPath(enteredPath);
                else if (enteredCommand == commandName_SetNetParametersPath)
                    SetNetParametersPath(enteredPath);
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
        private static void ShowStatus()
        {
            Console.WriteLine("\n" +
                //$"  Current Settings\n" +
                $"  General path is {(initializer.GeneralPath == default ? "unset." : initializer.GeneralPath)}\n" +
                $"  General prefix is {(initializer.FileName_Prefix == default ? "unset." : initializer.FileName_Prefix)}\n" +
                $"  General suffix is {(initializer.FileName_Suffix == default ? "unset." : initializer.FileName_Suffix)}\n\n" +
                $"  Path to net parameters is {(initializer.NetParametersPath == default ? "unset." : initializer.NetParametersPath)}\n" +
                $"  Path to trainer parameters is {(initializer.TrainerParametersPath == default ? "unset." : initializer.TrainerParametersPath)}\n" +
                $"  Path to sample set parameters is {(initializer.SampleSetParametersPath == default ? "unset." : initializer.SampleSetParametersPath)}\n" +
                $"  Path to initialized net is {(initializer.InitializedNetPath == default ? "unset." : initializer.InitializedNetPath)}\n" +
                $"  Path to trained net is {(initializer.TrainedNetPath == default ? "unset." : initializer.TrainedNetPath)}\n" +
                $"  Path to sample set is {(initializer.SampleSetPath == default ? "unset." : initializer.SampleSetPath)}\n" +
                $"  Path to log file is {(initializer.LogPath == default ? "unset." : initializer.LogPath)}\n\n" +
                $"  Logging is {(initializer.IsLogged ? "activated." : "deactivated.")}\n" +
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
                            $"  Set general path                     : {commandName_SetGeneralPath}=[general path]\n" +
                            $"  Set general prefix for file names    : {commandName_SetFileNamePrefix}=[general prefix]\n" +
                            $"  Set general suffix for file names    : {commandName_SetFileNameSuffix}=[general suffix]\n" +
                            $"  Set path to initialized net          : {commandName_SetInitializedNetPath}=[path to initialized net]\n" +
                            $"  Set path to trained net              : {commandName_SetTrainedNetPath}=[path to trained net]\n" +
                            $"  Set path to sample set               : {commandName_SetSampleSetPath}=[path to sample set]\n" +
                            $"  Set path to net parameters           : {commandName_SetNetParametersPath}=[path to net parameters]\n" +
                            $"  Set path to trainer parameters       : {commandName_SetTrainerParametersPath}=[path to trainer parameters]\n" +
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
        private static void SetGeneralPath(string path)
        {
            initializer.GeneralPath = path;
            Console.WriteLine("General path is set.");
        }
        private static void SetFileNamePrefix(string prefix)
        {
            initializer.FileName_Prefix = prefix;
            Console.WriteLine($"The file name has prefix {prefix} now.");
        }
        private static void SetFileNameSuffix(string suffix)
        {
            initializer.FileName_Suffix = suffix;
            Console.WriteLine($"The file name has suffix {suffix} now.");
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
        private static void SetNetParametersPath(string path)
        {
            initializer.NetParametersPath = path;
            Console.WriteLine("Path to net parameters is set.");
        }
        private static void SetTrainerParametersPath(string path)
        {
            initializer.TrainerParametersPath = path;
            Console.WriteLine("Path to trainer parameters is set.");
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
            initializer.GeneralPath = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\";
            initializer.FileName_Prefix = @"Test\";
            initializer.FileName_Suffix = "_test.txt";
            SetAllPaths();

            if(!await initializer.LoadNetParametersAsync())
                return;
            if(!await initializer.LoadTrainerParametersAsync())
                return;
            if(!await initializer.LoadInitializedNetAsync())
                return;        // Always check if the loaded initialized net suits loaded parameters!

            if(!await initializer.LoadSampleSetAsync())
                return;             // Always check if the loaded sample set suits the ... parameters!
            if(!await initializer.CreateNetAsync())
                return;
            if(!await initializer.CreateTrainerAsync())
                return;
            initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;

            await TrainAsync();
        }
        private static void SetAllPaths()
        {
            string path = initializer.GeneralPath;
            string prefix =initializer.FileName_Prefix;
            string suffix =initializer.FileName_Suffix;

            SetNetParametersPath(@$"{path}{prefix}{initializer.FileName_NetParameters}{suffix}");
            SetTrainerParametersPath(@$"{path}{prefix}{initializer.FileName_TrainerParameters}{suffix}");
            SetLogPath(@$"{path}{prefix}{initializer.FileName_Log}{suffix}");
            SetSampleSetParametersPath(@$"{path}{prefix}{initializer.FileName_SampleSetParameters}{suffix}");
            SetSampleSetPath(@$"{path}{prefix}{initializer.FileName_SampleSet}{suffix}");
            SetInitializedNetPath(@$"{path}{prefix}{initializer.FileName_InitializedNet}{suffix}");
            SetTrainedNetPath(@$"{path}{prefix}{initializer.FileName_TrainedNet}{suffix}");
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
