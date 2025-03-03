﻿using MachineLearningDataProvider.SampleSetHelpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NeuralNet_CLT.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNet_CLT.Commandables
{
    public class Save : CommandableBase
    {
        #region Commandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(async () =>
            {
                LoadAndSaveCommand saveCommand = GetSubCommand<LoadAndSaveCommand>(parametersAndSubCommand, out var parameters);
                CheckParameters(parameters, Show.InputInfo_Save, ConsoleInputCheck.EnsureNoOrSingleParameter);
                var singleParameter = GetRestrictedParameter(parameters, PresetValue.append, Formatting.Indented, $"{MainCommand.save}");   // GetSingleParameter<PresetValue>(parameters);  // Include in GetValidParameter?

                switch (saveCommand)
                {
                    case LoadAndSaveCommand.all:
                        await SaveSamplesNetAndTrainerAsync();
                        break;
                    case LoadAndSaveCommand.net0:
                        await initializer.SaveInitializedNetAsync(pathBuilder.InitializedNet);
                        break;
                    case LoadAndSaveCommand.net1:
                        await initializer.SaveTrainedNetAsync(pathBuilder.TrainedNet);
                        break;
                    case LoadAndSaveCommand.samples:
                        await initializer.SampleSet.SaveSamplesAsync(pathBuilder.SampleSet);
                        break;
                    case LoadAndSaveCommand.par:
                        await SaveAllParametersAsync(singleParameter);
                        break;
                    case LoadAndSaveCommand.netpar:
                        await paramBuilder.SaveNetParametersAsync(pathBuilder.NetParameters, singleParameter);
                        break;
                    case LoadAndSaveCommand.trainerpar:
                        await paramBuilder.SaveTrainerParametersAsync(pathBuilder.TrainerParameters, singleParameter);
                        break;
                    default:
                        break;
                }
            });
        }

        #endregion

        #region Sub Command methods

        internal static async Task SaveAllParametersAsync(Formatting formatting)
        {
            await paramBuilder.SaveNetParametersAsync(pathBuilder.NetParameters, formatting);
            await paramBuilder.SaveTrainerParametersAsync(pathBuilder.TrainerParameters, formatting);
        }
        internal static async Task SaveSamplesNetAndTrainerAsync() // incl trained net but no trainer
        {
            await initializer.SampleSet.SaveSamplesAsync(pathBuilder.SampleSet);
            await initializer.SaveInitializedNetAsync(pathBuilder.InitializedNet);
            await initializer.SaveTrainedNetAsync(pathBuilder.TrainedNet);
        }

        #endregion
    }
}
