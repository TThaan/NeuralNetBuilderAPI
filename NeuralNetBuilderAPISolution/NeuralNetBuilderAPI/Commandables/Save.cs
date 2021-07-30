using NeuralNetBuilder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class Save : CommandableBase
    {
        #region ICommandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(async () =>
            {
                LoadAndSaveCommand saveCommand = GetSubCommand<LoadAndSaveCommand>(parametersAndSubCommand, out var parameters);
                CheckParameters(parameters, MainCommand.save, ConsoleInputCheck.EnsureSingleParameter);
                string singleParameter = parametersAndSubCommand.ElementAt(1);

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
                        await initializer.SampleSet.SaveSampleSetAsync(pathBuilder.SampleSet);
                        break;
                    case LoadAndSaveCommand.par:
                        await SaveAllParametersAsync(singleParameter.ToEnum<PresetValue>());
                        break;
                    case LoadAndSaveCommand.netpar:
                        await SaveNetParametersAsync(singleParameter.ToEnum<PresetValue>());
                        break;
                    case LoadAndSaveCommand.trainerpar:
                        await SaveTrainerParametersAsync(singleParameter.ToEnum<PresetValue>());
                        break;
                    default:
                        break;
                }
            });
        }

        #endregion

        #region Sub Command methods

        internal static async Task SaveAllParametersAsync(PresetValue indented = PresetValue.indented)
        {
            await SaveNetParametersAsync(indented);
            await SaveTrainerParametersAsync(indented);
        }
        internal static async Task SaveNetParametersAsync(PresetValue indented = PresetValue.indented)
        {
            Formatting formatting = Formatting.None;

            if (indented == PresetValue.indented)
                formatting = Formatting.Indented;
            else if (indented != PresetValue.no)
                throw new ArgumentException($"{indented} is not a valid parameter for {MainCommand.save} {LoadAndSaveCommand.netpar}.\n" +
                    $"When saving net parameters you can add parameter {PresetValue.indented} or no parameter at all.");

            await paramBuilder.SaveNetParametersAsync(formatting);
        }
        internal static async Task SaveTrainerParametersAsync(PresetValue indented = PresetValue.indented)
        {
            Formatting formatting = Formatting.None;

            if (indented == PresetValue.indented)
                formatting = Formatting.Indented;
            else if (indented != PresetValue.no)
                throw new ArgumentException($"{indented} is not a valid parameter for {MainCommand.save} {LoadAndSaveCommand.trainerpar}.\n" +
                    $"When saving trainer parameters you can add parameter {PresetValue.indented} or no parameter at all.");

            await paramBuilder.SaveNetParametersAsync(formatting);
        }
        internal static async Task SaveSamplesNetAndTrainerAsync() // incl trained net but no trainer
        {
            await initializer.SampleSet.SaveSampleSetAsync(pathBuilder.SampleSet);
            await initializer.SaveInitializedNetAsync();
            await initializer.SaveTrainedNetAsync();
        }

        #endregion
    }
}
