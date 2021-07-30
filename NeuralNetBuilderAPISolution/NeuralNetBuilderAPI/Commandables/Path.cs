using NeuralNetBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class Path : CommandableBase
    {
        #region ICommandable

        public override async Task Execute(IEnumerable<string> parameters)
        {
            await Task.Run(() =>
            {
                CheckParameters(parameters);
                PathCommand pathCommand = GetSubCommand<PathCommand>(parameters);
                string singleParameter = parameters.ElementAt(1).GetParameterValue_String();

                switch (pathCommand)
                {
                    case PathCommand.prefix:
                        pathBuilder.SetFileNamePrefix(singleParameter);
                        break;
                    case PathCommand.suffix:
                        pathBuilder.SetFileNameSuffix(singleParameter);
                        break;
                    case PathCommand.reset:
                        pathBuilder.ResetPaths();
                        break;
                    case PathCommand.general:
                        pathBuilder.SetGeneralPath(singleParameter);
                        break;
                    case PathCommand.net0:
                        pathBuilder.SetInitializedNetPath(singleParameter);
                        break;
                    case PathCommand.net1:
                        pathBuilder.SetTrainedNetPath(singleParameter);
                        break;
                    case PathCommand.samples:
                        pathBuilder.SetSampleSetPath(singleParameter);
                        break;
                    case PathCommand.netpar:
                        pathBuilder.SetNetParametersPath(singleParameter);
                        break;
                    case PathCommand.trainerpar:
                        pathBuilder.SetTrainerParametersPath(singleParameter);
                        break;
                    case PathCommand.log:
                        pathBuilder.SetLogPath(singleParameter);
                        break;
                    default:
                        break;
                }
            });
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
                    $"The main command {MainCommand.path} must be followed by one of the following sub commands: \n" +
                    $"{Enum.GetNames(typeof(PathCommand)).ToStringFromCollection()}.");
        }
        private static void CheckParameterStructure(IEnumerable<string> parameters)
        {
            if (parameters.Count() > 2)
                throw new ArgumentException(
                    $"The main command {MainCommand.path} must be followed by a sub command and except in the case of the sub command {PathCommand.reset} a full file name.\n");
        }

        #endregion
    }
}
