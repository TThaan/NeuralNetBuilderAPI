﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNet_CLT.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNet_CLT.Commandables
{
    public class Path : CommandableBase
    {
        #region Commandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(() =>
            {
                PathCommand pathCommand = GetSubCommand<PathCommand>(parametersAndSubCommand, out var parameters);
                CheckParameters(parameters, Show.InputInfo_Path, ConsoleInputCheck.EnsureNoOrSingleParameter);
                var singleParameter = parameters.FirstOrDefault();

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
    }
}
