using System.Collections.Generic;
using System.Threading.Tasks;
using static NeuralNet_CLT.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNet_CLT.Commandables
{
    public class Create : CommandableBase
    {
        #region Commandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(() =>
            {
                CreateCommand createCommand = GetSubCommand<CreateCommand>(parametersAndSubCommand, out var parameters);
                CheckParameters(parameters, Show.InputInfo_Create, ConsoleInputCheck.EnsureNoOrSingleParameter);
                var appendLabelsLayerToNetParameters = GetRestrictedParameter(parameters, PresetValue.append, true, $"{MainCommand.create}"); // GetSingleParameter<PresetValue>(parameters);  // Include in GetValidParameter?

                switch (createCommand)
                {
                    case CreateCommand.all:
                        CreateNetAndTrainerAsync();
                        break;
                    case CreateCommand.net:
                        //if (appendLabelsLayerToNetParameters == true)
                        //    initializer.AppendLabelsLayerToNetParameters();
                        initializer.Net.Initialize(initializer.ParameterBuilder.NetParameters);
                        break;
                    case CreateCommand.trainer:
                        initializer.Trainer.Initialize(initializer.ParameterBuilder.TrainerParameters, initializer.Net, initializer.SampleSet);
                        initializer.Trainer.PropertyChanged += Trainer_PropertyChanged;
                        break;
                    //case CreateCommand.par:
                    //    CreateAllParameters();
                    //    break;
                    //case CreateCommand.netpar:
                    //    paramBuilder.CreateNetParameters();
                    //    break;
                    //case CreateCommand.trainerpar:
                    //    paramBuilder.CreateTrainerParameters();
                    //    break;
                    default:
                        break;
                }
            });
        }

        #endregion

        #region Sub Command methods

        //internal static bool CreateAllParameters()
        //{
        //    paramBuilder.CreateNetParameters();
        //    paramBuilder.CreateTrainerParameters();

        //    return true;
        //}
        internal static void CreateNetAndTrainerAsync()
        {
            initializer.Net.Initialize(initializer.ParameterBuilder.NetParameters);
            initializer.Trainer.Initialize(initializer.ParameterBuilder.TrainerParameters, initializer.Net, initializer.SampleSet);
        }

        #endregion
    }
}
