using NeuralNetBuilder;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class Create : CommandableBase
    {
        #region Commandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(async () =>
            {
                CreateCommand createCommand = GetSubCommand<CreateCommand>(parametersAndSubCommand, out var parameters);
                CheckParameters(parameters, MainCommand.create, ConsoleInputCheck.EnsureNoOrSingleParameter);
                var singleParameter = GetSingleParameter<PresetValue>(parameters);

                switch (createCommand)
                {
                    case CreateCommand.all:
                        await CreateNetAndTrainerAsync();
                        break;
                    case CreateCommand.net:
                        await initializer.CreateNetAsync(singleParameter);
                        break;
                    case CreateCommand.trainer:
                        if (await initializer.CreateTrainerAsync(initializer.SampleSet))
                            initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;
                        break;
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
            });
        }

        #endregion

        #region Sub Command methods

        internal static bool CreateAllParameters()
        {
            paramBuilder.CreateNetParameters();
            paramBuilder.CreateTrainerParameters();

            return true;
        }
        internal static async Task<bool> CreateNetAndTrainerAsync()
        {
            if (await initializer.CreateNetAsync())
                return false;
            if (await initializer.CreateTrainerAsync(initializer.SampleSet) == false)
                return false;

            return true;
        }

        #endregion
    }
}
