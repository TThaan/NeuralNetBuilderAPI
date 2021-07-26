using DeepLearningDataProvider;
using NeuralNetBuilder;
using NeuralNetBuilder.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NeuralNetBuilderAPI
{
    public class Program
    {
        #region Fit Console Window

        // https://stackoverflow.com/a/42334329/10547243

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        const int MONITOR_DEFAULTTOPRIMARY = 1;

        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            public static MONITORINFO Default
            {
                get { var inst = new MONITORINFO(); inst.cbSize = (uint)Marshal.SizeOf(inst); return inst; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int x, y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        const uint SW_RESTORE = 9;

        [StructLayout(LayoutKind.Sequential)]
        struct WINDOWPLACEMENT
        {
            public uint Length;
            public uint Flags;
            public uint ShowCmd;
            public POINT MinPosition;
            public POINT MaxPosition;
            public RECT NormalPosition;
            public static WINDOWPLACEMENT Default
            {
                get
                {
                    var instance = new WINDOWPLACEMENT();
                    instance.Length = (uint)Marshal.SizeOf(instance);
                    return instance;
                }
            }
        }

        #endregion

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

            // Get this console window's hWnd (window handle).
            IntPtr hWnd = GetConsoleWindow();

            // Get information about the monitor (display) that the window is (mostly) displayed on.
            // The .rcWork field contains the monitor's work area, i.e., the usable space excluding
            // the taskbar (and "application desktop toolbars" - see https://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx)
            var mi = MONITORINFO.Default;
            GetMonitorInfo(MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY), ref mi);

            // Get information about this window's current placement.
            var wp = WINDOWPLACEMENT.Default;
            GetWindowPlacement(hWnd, ref wp);

            // Calculate the window's new position: lower left corner.
            // !! Inexplicably, on W10, work-area coordinates (0,0) appear to be (7,7) pixels 
            // !! away from the true edge of the screen / taskbar.
            int fudgeOffset = 7;
            wp.NormalPosition = new RECT()
            {
                Left = -fudgeOffset + mi.rcWork.Right / 2,
                Top = mi.rcWork.Top,// mi.rcWork.Bottom - (wp.NormalPosition.Bottom - wp.NormalPosition.Top),
                Right = mi.rcWork.Right,
                Bottom = fudgeOffset + mi.rcWork.Bottom
            };

            // Place the window at the new position.
            SetWindowPlacement(hWnd, ref wp);

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

            pathBuilder.ResetPaths();
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
                    out string parameter, 
                    out int layerId);

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
                        default:
                            break;
                    }

                }

                #endregion

                #region Path

                else if (mainCommand == MainCommand.path)
                {
                    PathCommand pathCommand = subCommand_String.ToEnum<PathCommand>();
                    string path = parameter;

                    switch (pathCommand)
                    {
                        case PathCommand.prefix:
                            pathBuilder.SetFileNamePrefix(path);
                            break;
                        case PathCommand.suffix:
                            pathBuilder.SetFileNameSuffix(path);
                            break;
                        case PathCommand.reset:
                            pathBuilder.ResetPaths();
                            break;
                        case PathCommand.general:
                            pathBuilder.SetGeneralPath(path);
                            break;
                        case PathCommand.net0:
                            pathBuilder.SetInitializedNetPath(path);
                            break;
                        case PathCommand.net1:
                            pathBuilder.SetTrainedNetPath(path);
                            break;
                        case PathCommand.samples:
                            pathBuilder.SetSampleSetPath(path);
                            break;
                        case PathCommand.netpar:
                            pathBuilder.SetNetParametersPath(path);
                            break;
                        case PathCommand.trainerpar:
                            pathBuilder.SetTrainerParametersPath(path);
                            break;
                        case PathCommand.log:
                            pathBuilder.SetLogPath(path);
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
                            await initializer.CreateNetAsync();
                            break;
                        case CreateCommand.trainer:
                            if (await initializer.CreateTrainerAsync(initializer.SampleSet))
                                initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;
                            break;
                        //case CreateCommand.samples:
                        //    break;
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
                            await initializer.SampleSet.LoadSampleSetAsync(pathBuilder.SampleSet, .01f, 0); // dist etc only in CreatSampleSet?  // Task: dynamize testSamplesFaction!
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
                            await SaveAllParametersAsync();
                            break;
                        case LoadAndSaveCommand.netpar:
                            await paramBuilder.SaveNetParametersAsync();
                            break;
                        case LoadAndSaveCommand.trainerpar:
                            await paramBuilder.SaveTrainerParametersAsync();
                            break;
                        default:
                            break;
                    }
                }

                #endregion

                #region Change parameter

                else if (mainCommand == MainCommand.p)
                {
                    ChangeParameterCommand changeParameterCommand = subCommand_String.ToEnum<ChangeParameterCommand>();
                    //int layerId = int.Parse(layerId_String);

                    switch (changeParameterCommand)
                    {
                        case ChangeParameterCommand.set:
                            paramBuilder.ChangeParameter(parameter, layerId);
                            break;
                        case ChangeParameterCommand.add:
                            paramBuilder.AddLayerAfter(layerId);
                            break;
                        default:
                            break;
                    }
                }

                #endregion

                #region Misc

                else if (mainCommand == MainCommand.logon)
                    Log();
                else if (mainCommand == MainCommand.logoff)
                    Unlog();
                else if (mainCommand == MainCommand.test)
                    await TestTraining();
                else if (mainCommand == MainCommand.train)
                    await TrainAsync();

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

        #region combining methods

        public static async Task<bool> CreateNetAndTrainerAsync()
        {
            if (await initializer.CreateNetAsync() == false)
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
        public static async Task<bool> SaveAllParametersAsync()
        {
            bool result = true;

            if (await paramBuilder.SaveNetParametersAsync() == false) result = false;
            if (await paramBuilder.SaveTrainerParametersAsync() == false) result = false;

            return result;
        }
        public static async Task<bool> SaveSamplesNetAndTrainerAsync() // incl trained net but no trainer
        {
            if (await initializer.SampleSet.SaveSampleSetAsync(pathBuilder.SampleSet) == false)
                return false;
            if (await initializer.SaveInitializedNetAsync() == false)
                return false;
            if (await initializer.SaveTrainedNetAsync() == false)
                return false;

            return true;
        }
        public static async Task<bool> LoadSamplesNetAndTrainerAsync() // incl trained net but no trainer
        {
            if (await initializer.SampleSet.LoadSampleSetAsync(pathBuilder.SampleSet, .01f, 0) == false)
                return false;
            if (await initializer.LoadNetAsync() == false)
                return false;
            //if (await initializer.LoadTrainedNetAsync() == false)
            //    return false;

            return true;
        }

        #endregion

        private static async Task TrainAsync()
        {
            stopwatch.Reset();
            stopwatch.Start();
            await initializer.TrainAsync(initializer.SampleSet);
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

                $"     Path to net parameters           : {(pathBuilder.NetParameters == default ? " - " : pathBuilder.NetParameters)}\n" +
                $"     Path to trainer parameters       : {(pathBuilder.TrainerParameters == default ? " - " : pathBuilder.TrainerParameters)}\n" +
                $"     Path to sample set               : {(pathBuilder.SampleSet == default ? " - " : pathBuilder.SampleSet)}\n" +
                $"     Path to initialized net          : {(pathBuilder.InitializedNet == default ? " - " : pathBuilder.InitializedNet)}\n" +
                $"     Path to trained net              : {(pathBuilder.TrainedNet == default ? " - " : pathBuilder.TrainedNet)}\n" +
                $"     Path to log file                 : {(pathBuilder.Log == default ? " - " : pathBuilder.Log)}\n\n" +

                $"     Net Parameters        : {(paramBuilder.NetParameters == null ? " - " : "set")}\n" + 
                $"     Trainer Parameters    : {(paramBuilder.TrainerParameters == null ? " - " : "set")}\n" +
                $"     Sample Set            : {(initializer.SampleSet.TrainSet == null || initializer.SampleSet.TestSet == null ? " - " : "set")}\n" +
                $"     Net                   : {(initializer.Net == null ? " - " : "set")}\n" + 
                $"     Trainer               : {(initializer.Trainer == null ? " - " : "set")}\n\n" +

                $"     Logging is {(initializer.IsLogged ? "on." : "off.")}\n\n");

            // Reactivate the event handling method again.
            isInitializerStatusChangedEventActive = true;
        }
        private static void ShowHelp()
        {
            // wa load/save trainer?
            Console.WriteLine("\n" +
                $"     General Input Format                      : [Main Command] [Sub Command] [opt: Parameter] [opt: Layer Id]\n\n" +
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
                $"     Create all parameters               : {MainCommand.create} {CreateCommand.par} [optional: template name]\n" +
                $"     Create the net parameters           : {MainCommand.create} {CreateCommand.netpar}\n" +
                $"     Create the trainer parameters       : {MainCommand.create} {CreateCommand.trainerpar}\n" +
                $"     Create sample set, net & trainer    : {MainCommand.create} {CreateCommand.all}\n" +
                $"     Create sample set                   : {MainCommand.create} {CreateCommand.samples}\n" +
                $"     Create the net                      : {MainCommand.create} {CreateCommand.net}\n" +
                $"     Create the trainer                  : {MainCommand.create} {CreateCommand.trainer}\n\n" +

                $"     Load all parameters                 : {MainCommand.load} {LoadAndSaveCommand.par}\n" +
                $"     Load net parameters                 : {MainCommand.load} {LoadAndSaveCommand.netpar}\n" +
                $"     Load trainer parameters             : {MainCommand.load} {LoadAndSaveCommand.trainerpar}\n" +
                $"     Load sample set, net & trainer      : {MainCommand.load} {LoadAndSaveCommand.all}\n" +
                $"     Load sample set                     : {MainCommand.load} {LoadAndSaveCommand.samples}\n" +
                $"     Load initialized net                : {MainCommand.load} {LoadAndSaveCommand.net0}\n" +
                $"     Load trained net                    : {MainCommand.load} {LoadAndSaveCommand.net1}\n\n" +

                $"     Save all parameters                 : {MainCommand.save} {LoadAndSaveCommand.par}\n" +
                $"     Save net parameters                 : {MainCommand.save} {LoadAndSaveCommand.netpar}\n" +
                $"     Save trainer parameters             : {MainCommand.save} {LoadAndSaveCommand.trainerpar}\n" +
                $"     Save sample set, net & trainer      : {MainCommand.save} {LoadAndSaveCommand.all}\n" +
                $"     Save sample set                     : {MainCommand.save} {LoadAndSaveCommand.samples}\n" +
                $"     Save initialized net                : {MainCommand.save} {LoadAndSaveCommand.net0}\n" +
                $"     Save trained net                    : {MainCommand.save} {LoadAndSaveCommand.net1}\n\n" +

                $"     Show Settings                       : {MainCommand.show} {ShowCommand.settings}\n" +
                $"     Show this help                      : {MainCommand.show} {ShowCommand.help}\n" +
                $"     Show all parameters                 : {MainCommand.show} {ShowCommand.par}\n" +
                $"     Show net parameters                 : {MainCommand.show} {ShowCommand.netpar}\n" +
                $"     Show trainer parameters             : {MainCommand.show} {ShowCommand.trainerpar}\n");
                                                              
            Console.WriteLine("\n" +                          
                $"     Add layer after layer index               : {MainCommand.p} {ChangeParameterCommand.add} [preceding layer id]\n" +
                $"     Example 1 (Add a new layer after layer 0) : {MainCommand.p} {ChangeParameterCommand.add} 0\n" +
                $"     Change parameter                          : {MainCommand.p} {ChangeParameterCommand.set} [parameter name] [optional: :parameter value] [optional: layer id]\n" +
                $"     Example 2 (Set the global WeightMax 1)    : {MainCommand.p} {ChangeParameterCommand.set} {ParameterName.wMax}:1 glob\n" + // global!
                $"     Example 3 (Set BiasMin of layer 3 to 2)   : {MainCommand.p} {ChangeParameterCommand.set} {ParameterName.bMin}:2 3\n" +
                $"     Parameter Names                           : {Enum.GetNames(typeof(ParameterName)).ToStringFromCollection()}\n\n");

            var activationTypes = Enum.GetNames(typeof(ActivationType));
            Console.WriteLine(
                $" Set Activation Type of layer 3                :");
            for (int i = 0; i < activationTypes.Length; i++)
            {
                Console.WriteLine(
                $"     {activationTypes[i], -30}            : {MainCommand.p} {ChangeParameterCommand.set} act:{i} 3");
            }
            Console.WriteLine("\n" + 
                $"     Activate logging                  : {MainCommand.logon}\n" +
                $"     Deactivate logging                  : {MainCommand.logoff}\n" +
                $"     Start test training                 : {MainCommand.test}\n" +
                $"     Start training                      : {MainCommand.train}\n\n");
        }
        private static void ShowAllParameters()
        {
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
                Console.WriteLine($"     Layer {lp.Id}: N = {lp.NeuronsPerLayer}, weightRange = {lp.WeightMin}/{lp.WeightMax}, biasRange = {lp.BiasMin}/{lp.BiasMax}, Activation = {lp.ActivationType}");
            }

            // Console.WriteLine();
        }
        private static void ShowTrainerParameters()
        {
            if (paramBuilder.TrainerParameters == null)
                return;

            Console.WriteLine("\n" +
                $"     Learning Rate        : {paramBuilder.TrainerParameters.LearningRate}\n" +
                $"     Learning Rate Change : {paramBuilder.TrainerParameters.LearningRateChange}\n" +
                $"     Epochs               : {paramBuilder.TrainerParameters.Epochs}\n" +
                $"     Cost Type            : {paramBuilder.TrainerParameters.CostType}\n");
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
            if (!await initializer.LoadNetAsync())
                return;        // Always check if the loaded initialized net suits loaded parameters!

            if (!await initializer.SampleSet.LoadSampleSetAsync(pathBuilder.SampleSet, .01f, 0))
                return;             // Always check if the loaded sample set suits the ... parameters!
            if (!await initializer.CreateNetAsync())
                return;
            if (!await initializer.CreateTrainerAsync(initializer.SampleSet))
                return;
            initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;

            await TrainAsync();
        }

        #region helpers

        private static void AnalyzeInput(string consoleInput, out MainCommand mainCommand, out string subCommand_String, out string parameter, out int layerId)
        {
            if (consoleInput.Length <= 1)
                throw new ArgumentException("A console input must consist of at least two units: a main command and a sub command.");

            var splitInput = consoleInput.Split(' ');

            mainCommand = splitInput[0].ToEnum<MainCommand>();
            subCommand_String = splitInput[1];

            var inputHelpers = GetInputHelpers(splitInput.Skip(2));
            layerId = GetLayerId(inputHelpers);
            var commandsAndParams = splitInput.Except(inputHelpers).ToArray();   // Exclude all InputHelpers!
            parameter = commandsAndParams.Last();

            switch (mainCommand)
            {
                case MainCommand.Undefined:
                    break;
                case MainCommand.path:
                    subCommand_String = GetSubCommand<PathCommand>(commandsAndParams[1]);
                    break;
                case MainCommand.show:
                    subCommand_String = GetSubCommand<ShowCommand>(commandsAndParams[1]);
                    break;
                case MainCommand.create:
                    subCommand_String = GetSubCommand<CreateCommand>(commandsAndParams[1]);
                    break;
                case MainCommand.load:
                    subCommand_String = GetSubCommand<LoadAndSaveCommand>(commandsAndParams[1]);
                    break;
                case MainCommand.save:
                    subCommand_String = GetSubCommand<LoadAndSaveCommand>(commandsAndParams[1]);
                    break;
                case MainCommand.logon:
                    break;
                case MainCommand.logoff:
                    break;
                case MainCommand.train:
                    break;
                case MainCommand.test:
                    break;
                case MainCommand.p:
                    subCommand_String = GetSubCommand<ChangeParameterCommand>(commandsAndParams[1]);
                    break;
                default:
                    break;
            }

            if (subCommand_String == null)
                throw new ArgumentException($"A valid sub command is missing." );
        }
        private static string GetSubCommand<TEnum>(string subCommand)
        {
            if (Enum.GetNames(typeof(TEnum)).Contains(subCommand))
                return subCommand;
            else
                throw new ArgumentException($"SubCommand '{subCommand}' does not exist.");
        }
        private static string GetParameterValue(string parameter)
        {
            if (parameter.Contains(':'))
                return parameter.Split(':').Last();
            else
                return null;
        }
        private static IEnumerable<string> GetInputHelpers(IEnumerable<string> consoleInputWithoutCommands)
        {
            var allInpHelpers = Enum.GetNames(typeof(InputHelper));

            var elementsStartingWithAnInputHelper = consoleInputWithoutCommands.Where(
                x => allInpHelpers.Contains(x.First().ToString())
                );
            var elementsWithANumberAfterL = elementsStartingWithAnInputHelper.Skip(1).Where(x => int.Parse(x).GetType() == typeof(int)); ;
            
            return elementsWithANumberAfterL;
        }
        private static int GetLayerId(IEnumerable<string> inputHelpers)
        {
            string onlyElementStartingWithL = inputHelpers.SingleOrDefault(x => string.Equals(x[0].ToString(), InputHelper.L.ToString()));
            int layerId = int.Parse(onlyElementStartingWithL.Skip(1).ToString());
            return layerId;
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