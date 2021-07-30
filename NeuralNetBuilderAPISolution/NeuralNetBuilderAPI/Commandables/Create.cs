using NeuralNetBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class Create : CommandableBase
    {
        #region ICommandable

        public override async Task Execute(IEnumerable<string> parameters)
        {
            await Task.Run(async () =>
            {
                CheckParameters(parameters);
                CreateCommand createCommand = GetSubCommand<CreateCommand>(parameters);
                string singleParameter = parameters.ElementAt(1);

                switch (createCommand)
                {
                    case CreateCommand.all:
                        await CreateNetAndTrainerAsync();
                        break;
                    case CreateCommand.net:
                        await initializer.CreateNetAsync(singleParameter.ToEnum<PresetValue>());
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

        #region helpers

        private static void CheckParameters(IEnumerable<string> parameters)
        {
            CheckSubCommand(parameters);
            CheckParameterStructure(parameters);
        }
        // in base class?
        private static void CheckSubCommand(IEnumerable<string> parameters)
        {
            if (parameters.Count() == 0)
                throw new ArgumentException(
                    $"The main command {MainCommand.create} must be followed by one of the following sub commands: \n" +
                    $"{Enum.GetNames(typeof(CreateCommand)).ToStringFromCollection()}.");
        }
        private static void CheckParameterStructure(IEnumerable<string> parameters)
        {
            if (parameters.Count() > 2)
                throw new ArgumentException(
                    $"The main command {MainCommand.create} must be followed by a sub command and in case of the sub command {CreateCommand.net} an optional parameter ('{PresetValue.append}').\n" +
                    "Anything else is invalid");
        }

        #endregion
    }
}
