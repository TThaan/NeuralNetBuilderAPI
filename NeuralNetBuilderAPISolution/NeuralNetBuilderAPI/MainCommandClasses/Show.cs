using NeuralNetBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class Show : CommandableBase
    {
        #region Commandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(() =>
            {
                ShowCommand showCommand = GetSubCommand<ShowCommand>(parametersAndSubCommand, out var parameters);
                CheckParameters(parameters, InputInfo_Show, ConsoleInputCheck.EnsureNoParameter);

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
            });
        }

        #endregion

        #region Sub Command methods

        internal static void ShowSettings()
        {
            // Exchange ':' and ' ' with separators

            // Prevent double output about 'X' is null (from initlializer property) plus 'X' is unset here
            // by deactivating the event handling method temporarily.
            isInitializerStatusChangedEventActive = false;

            Console.WriteLine("\n" +
                $"     Current Settings:\n\n" +
                $"     General path    : {(pathBuilder.General == default ? " - " : pathBuilder.General)}\n" +
                $"     General prefix  : {(pathBuilder.BasicName_Prefix == default ? " - " : pathBuilder.BasicName_Prefix)}\n" +
                $"     General suffix  : {(pathBuilder.BasicName_Suffix == default ? " - " : pathBuilder.BasicName_Suffix)}\n\n" +

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
        internal static void ShowHelp()
        {
            Console.WriteLine(InputInfo_Show);
            Console.WriteLine(InputInfo_Path);
            Console.WriteLine(InputInfo_Create);
            Console.WriteLine(InputInfo_Load);
            Console.WriteLine(InputInfo_Save);
            Console.WriteLine(InputInfo_Layer);
            Console.WriteLine(InputInfo_Param);
            Console.WriteLine(InputInfo_Log);
            Console.WriteLine(InputInfo_Train);
        }
        internal static void ShowAllParameters()
        {
            ShowNetParameters();
            ShowTrainerParameters();
        }
        internal static void ShowNetParameters()
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
        internal static void ShowTrainerParameters()
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
        internal static void ShowNet()
        {
            Console.WriteLine();

            if (initializer.Net == null)
            {
                Console.WriteLine("     Net is not set yet.");
                return;
            }

            // Has to be defined before using it in the following formatted string.
            var mulishString = paramBuilder.NetParameters == null
                ? "Unknown (Info is only available in net parameters not in the initialized net.)"
                : paramBuilder.NetParameters.WeightInitType.ToString();

            Console.WriteLine(
                $"     Layers         : {initializer.Net.Layers.Count()}\n" +
                $"     WeightInitType : {mulishString}\n");

            foreach (var layer in initializer.Net.Layers)
            {
                Console.WriteLine($"     Layer {layer.Id}: " +
                    $"N = {layer.N}, " +
                    $"weightRange = {layer.Weights?.GetMinimum<float>()}/{layer.Weights?.GetMaximum<float>()}, " +
                    $"biasRange = {layer.Biases?.GetMinimum()}/{layer.Biases?.GetMaximum()}, " +
                    $"Activation = {layer.ActivationFunction.ActivationType}");
            }
        }
        internal static void ShowSampleSet()
        {
            Console.WriteLine();

            if (initializer.SampleSet == null || initializer.SampleSet.TrainSet == null || initializer.SampleSet.TestSet == null)
                throw new ArgumentException("     Sample set is not set yet. Or it is missing training samples or test samples.");

            Console.WriteLine(
                $"     Training Samples : {initializer.SampleSet.TrainSet.Count()}\n" +
                $"     Test Samples     : {initializer.SampleSet.TestSet.Count()}\n" +
                $"     Labels / Targets : {initializer.SampleSet.Targets.Count()}\n");

            Console.WriteLine("     First 5 training samples:\n");
            for (int i = 0; i <= 5; i++)
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

        internal static string InputInfo_Show =>
                $"     Show Settings                    : {MainCommand.show} {ShowCommand.settings}\n" +
                $"     Show this help                   : {MainCommand.show} {ShowCommand.help}\n" +
                $"     Show all parameters              : {MainCommand.show} {ShowCommand.par}\n" +
                $"     Show net parameters              : {MainCommand.show} {ShowCommand.netpar}\n" +
                $"     Show trainer parameters          : {MainCommand.show} {ShowCommand.trainerpar}\n" +
                $"     Show net                         : {MainCommand.show} {ShowCommand.net}\n" +
                $"     Show sample set                  : {MainCommand.show} {ShowCommand.samples}\n\n";
        internal static string InputInfo_Path =>
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
                $"     Reset general path and use default names  : {MainCommand.path} {PathCommand.reset}\n\n";
        internal static string InputInfo_Create =>
                //$"     Create all parameters            : {MainCommand.create} {CreateCommand.par}\n" +
                //$"     Create the net parameters        : {MainCommand.create} {CreateCommand.netpar}\n" +
                //$"     Create the trainer parameters    : {MainCommand.create} {CreateCommand.trainerpar}\n" +
                $"     Create sample set, net & trainer : {MainCommand.create} {CreateCommand.all}\n" +
                $"     Create sample set                : {MainCommand.create} {CreateCommand.samples}\n" +
                $"     Create the net                   : {MainCommand.create} {CreateCommand.net} [opt: append]\n" +
                $"                                        append = append an auto-generated last layer considering all labels\n" +
                $"     Create the trainer               : {MainCommand.create} {CreateCommand.trainer}\n\n";
        internal static string InputInfo_Load =>
                $"     Load all parameters              : {MainCommand.load} {LoadAndSaveCommand.par}\n" +
                $"     Load net parameters              : {MainCommand.load} {LoadAndSaveCommand.netpar}\n" +
                $"     Load trainer parameters          : {MainCommand.load} {LoadAndSaveCommand.trainerpar}\n" +
                $"     Load sample set, net & trainer   : {MainCommand.load} {LoadAndSaveCommand.all}\n" +
                $"     Load sample set                  : {MainCommand.load} {LoadAndSaveCommand.samples}\n" +
                $"     Load initialized net             : {MainCommand.load} {LoadAndSaveCommand.net0}\n" +
                $"     Load trained net                 : {MainCommand.load} {LoadAndSaveCommand.net1}\n\n";
        internal static string InputInfo_Save =>
                $"     Save all parameters              : {MainCommand.save} {LoadAndSaveCommand.par}\n" +
                $"     Save net parameters              : {MainCommand.save} {LoadAndSaveCommand.netpar}\n" +
                $"     Save trainer parameters          : {MainCommand.save} {LoadAndSaveCommand.trainerpar}\n" +
                $"     Save sample set, net & trainer   : {MainCommand.save} {LoadAndSaveCommand.all}\n" +
                $"     Save sample set                  : {MainCommand.save} {LoadAndSaveCommand.samples}\n" +
                $"     Save initialized net             : {MainCommand.save} {LoadAndSaveCommand.net0}\n" +
                $"     Save trained net                 : {MainCommand.save} {LoadAndSaveCommand.net1}\n\n";
        internal static string InputInfo_Layer =>
                $"     Change layer                              : {MainCommand.layer} [layer command] [opt: :parameter value] [opt: layer id]\n" +
                $"     Add layer after layer index               : {MainCommand.layer} {LayerCommand.del} [preceding layer id]\n" +
                $"     Example 1 (Add a new layer after layer 0) : {MainCommand.layer} {LayerCommand.left} L0\n\n";
        internal static string InputInfo_Param
        {
            get
            {
                string result =
                    $"     Change parameter                          : {MainCommand.param} [parameter command] [parameter name]\n" +
                    $"                                                 {Enumerable.Repeat(' ', MainCommand.param.ToString().Length).ToStringFromCollection(string.Empty)} [opt: :parameter value] [opt: layer id or 'glob']\n" +
                    $"     Example 2 (Set the global WeightMax 1)    : {MainCommand.param} {ParameterCommand.set} {ParameterName.wMax}:1 glob\n" + // global!
                    $"     Example 3 (Set BiasMin of layer 3 to 2)   : {MainCommand.param} {ParameterCommand.set} {ParameterName.bMin}:2 L3\n" +
                    $"     Parameter Names                           : {Enum.GetNames(typeof(ParameterName)).ToStringFromCollection(", ", 4, 49)}\n\n";

                var activationTypes = Enum.GetNames(typeof(ActivationType));
                result += $"     Set Activation Type of layer 3 :\n";
                for (int i = 0; i < activationTypes.Length; i++)
                {
                    result += $"     {activationTypes[i],-30} : {MainCommand.param} {ParameterCommand.set} act:{i} L3\n";
                }

                return result;
            }
        }
        internal static string InputInfo_Log =>
                $"     Activate logging    : {MainCommand.log} {LogCommand.on}\n" +
                $"     Deactivate logging  : {MainCommand.log} {LogCommand.off}\n";
        internal static string InputInfo_Train =>
                $"     Start training      : {MainCommand.train} {TrainCommand.start} [opt: shuffle]\n" +
                $"     Start test training : {MainCommand.train} {TrainCommand.example} [opt: shuffle]\n" +
                $"                           shuffle = shuffle training samples before first training\n\n";

        #endregion
    }
}
