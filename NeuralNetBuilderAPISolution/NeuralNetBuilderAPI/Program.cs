﻿using NeuralNetBuilder;
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

        #endregion

        #region methods

        static async Task Main(string[] args)
        {
            initializer = new Initializer();
            initializer.InitializerStatusChanged += Initializer_StatusChanged_EventHandlingMethod;
            paths = initializer.Paths;
            parameters = initializer.Parameters;
            commands = CommandNames.GetDefaultCommandNames();

            paths.SetAllPaths();
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
                if (enteredCommand == commands.Help)
                    ShowHelp();
                else if (enteredCommand == commands.Status)
                    ShowStatus();
                else if (enteredCommand == commands.Log)
                    Log();
                else if (enteredCommand == commands.Unlog)
                    Unlog();
                else if (enteredCommand == commands.SetAllPaths)
                    paths.SetAllPaths();
                else if (enteredCommand == commands.TestTraining)
                    await TestTraining();
                else if (enteredCommand == commands.LoadNetParameters)
                    await initializer.LoadNetParametersAsync();
                else if (enteredCommand == commands.LoadTrainerParameters)
                    await initializer.LoadTrainerParametersAsync();
                else if (enteredCommand == commands.LoadSampleSetParameters)
                    await initializer.LoadSampleSetParametersAsync();
                else if (enteredCommand == commands.LoadInitializedNet)
                    await initializer.LoadInitializedNetAsync();
                else if (enteredCommand == commands.LoadTrainedNet)
                    await initializer.LoadTrainedNetAsync();
                else if (enteredCommand == commands.LoadSampleSet)
                    await initializer.LoadSampleSetAsync();
                else if (enteredCommand == commands.InitializeNet)
                    await initializer.CreateNetAsync();
                else if (enteredCommand == commands.InitializeTrainer)
                {
                    if (await initializer.CreateTrainerAsync())
                        initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;
                }
                else if (enteredCommand == commands.CreateSampleSet)
                    await initializer.CreateSampleSetAsync();
                else if (enteredCommand == commands.Train)
                    await TrainAsync();
                else if (enteredCommand == commands.SaveInitializedNet)
                    await initializer.SaveInitializedNetAsync();
                else if (enteredCommand == commands.SaveTrainedNet)
                    await initializer.SaveTrainedNetAsync();
                else if (enteredCommand == commands.SaveSampleSet)
                    await initializer.SaveSampleSetAsync();
                else
                    Console.WriteLine("Unkown Command.");
            }
            else
            {
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
        private static void ShowStatus()
        {
            Console.WriteLine("\n" +
                //$"  Current Settings\n" +
                $"  General path is {(paths.General == default ? "unset." : paths.General)}\n" +
                $"  General prefix is {(paths.FileName_Prefix == default ? "unset." : paths.FileName_Prefix)}\n" +
                $"  General suffix is {(paths.FileName_Suffix == default ? "unset." : paths.FileName_Suffix)}\n\n" +
                $"  Path to net parameters is {(paths.NetParameters == default ? "unset." : paths.NetParameters)}\n" +
                $"  Path to trainer parameters is {(paths.TrainerParameters == default ? "unset." : paths.TrainerParameters)}\n" +
                $"  Path to sample set parameters is {(paths.SampleSetParameters == default ? "unset." : paths.SampleSetParameters)}\n" +
                $"  Path to initialized net is {(paths.InitializedNet == default ? "unset." : paths.InitializedNet)}\n" +
                $"  Path to trained net is {(paths.TrainedNet == default ? "unset." : paths.TrainedNet)}\n" +
                $"  Path to sample set is {(paths.SampleSet == default ? "unset." : paths.SampleSet)}\n" +
                $"  Path to log file is {(paths.Log == default ? "unset." : paths.Log)}\n\n" +
                $"  Logging is {(initializer.IsLogged ? "activated." : "deactivated.")}\n" +
                $"  Sample Set Parameters are {(parameters.SampleSetParameters == null ? "unset." : "set.")}\n" +
                $"  Sample Set is {(initializer?.SampleSet == null ? "unset." : "set.")}\n" +
                $"  Net Parameters are {(parameters.NetParameters == null ? "unset." : "set.")}\n" +
                $"  Trainer Parameters are {(parameters.TrainerParameters == null ? "unset." : "set.")}\n" +
                $"  Net is {(initializer?.Net == null ? "raw." : "initialized.")}\n" +
                $"  Trainer is {(initializer?.Trainer == null ? "raw." : "initialized.")}\n");
        }
        private static void ShowHelp()
        {
            Console.WriteLine("\n" +
                            $"  Set general path                     : {commands.SetGeneralPath}=[general path]\n" +
                            $"  Set general prefix for file names    : {commands.SetFileNamePrefix}=[general prefix]\n" +
                            $"  Set general suffix for file names    : {commands.SetFileNameSuffix}=[general suffix]\n" +
                            $"  Set path to initialized net          : {commands.SetInitializedNetPath}=[path to initialized net]\n" +
                            $"  Set path to trained net              : {commands.SetTrainedNetPath}=[path to trained net]\n" +
                            $"  Set path to sample set               : {commands.SetSampleSetPath}=[path to sample set]\n" +
                            $"  Set path to net parameters           : {commands.SetNetParametersPath}=[path to net parameters]\n" +
                            $"  Set path to trainer parameters       : {commands.SetTrainerParametersPath}=[path to trainer parameters]\n" +
                            $"  Set path to sample set parameters    : {commands.SetSampleSetParametersPath}=[path to sample set parameters]\n" +
                            $"  Load sample set parameters from file : {commands.LoadSampleSetParameters}\n" +
                            $"  Create sample set                    : {commands.CreateSampleSet}\n" +
                            $"  Load initialized net from file       : {commands.LoadInitializedNet}\n" +
                            $"  Load trained net from file           : {commands.LoadTrainedNet}\n" +
                            $"  Load sample set from file            : {commands.LoadSampleSet}\n" +
                            $"  Initialize the net                   : {commands.InitializeNet}\n" +
                            $"  Initialize the trainer               : {commands.InitializeTrainer}\n" +
                            $"  Save initialized net to file         : {commands.SaveInitializedNet}\n" +
                            $"  Save trained net to file             : {commands.SaveTrainedNet}\n" +
                            $"  Save sample set to file              : {commands.SaveSampleSet}\n" +
                            $"  Set path to log file                 : {commands.SetLogPath}=[path to log file]\n" +
                            $"  Deactivate logging                   : {commands.Unlog}\n" +
                            $"  Start training                       : {commands.Train}\n" +
                            $"  Show Status                          : {commands.Status}\n" +
                            $"  Show this help                       : {commands.Help}\n");
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
            initializer.Paths.General = @"C:\Users\Jan_PC\Documents\_NeuralNetApp\Saves\";
            initializer.Paths.FileName_Prefix = @"Test\";
            initializer.Paths.FileName_Suffix = "_test.txt";
            paths.SetAllPaths();

            if (!await initializer.LoadNetParametersAsync())
                return;
            if (!await initializer.LoadTrainerParametersAsync())
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