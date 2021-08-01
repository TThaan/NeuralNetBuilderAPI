using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class Load : CommandableBase
    {
        #region Commandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(async () =>
            {
                LoadAndSaveCommand loadCommand = GetSubCommand<LoadAndSaveCommand>(parametersAndSubCommand, out var parameters);
                // CheckParameters(parameters, MainCommand.load);

                switch (loadCommand)
                {
                    case LoadAndSaveCommand.all:
                        await LoadSamplesAndNetAsync();
                        break;
                    case LoadAndSaveCommand.net0:
                        await initializer.LoadNetAsync();
                        break;
                    case LoadAndSaveCommand.net1:
                        await initializer.LoadTrainedNetAsync();
                        break;
                    case LoadAndSaveCommand.samples:
                        await LoadSampleSetAsync(pathBuilder.SampleSet, parameters);
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
            });
        }

        #endregion

        #region Sub Command methods

        internal static async Task<bool> LoadAllParametersAsync()
        {
            bool result = true;

            if (await paramBuilder.LoadNetParametersAsync() == false) result = false;
            if (await paramBuilder.LoadTrainerParametersAsync() == false) result = false;

            return result;
        }
        internal static async Task<bool> LoadSamplesAndNetAsync()
        {
            if (await initializer.SampleSet.LoadSampleSetAsync(pathBuilder.SampleSet, .1f, 0) == false)
                return false;
            if (await initializer.LoadNetAsync() == false)
                return false;

            return true;
        }
        /// <summary>
        /// Default values if you parameters miss an input helper:
        /// testSamplesInPercent = 10, columnIndex_Label = 0.
        /// </summary>
        internal static async Task<bool> LoadSampleSetAsync(string samplesFileName, IEnumerable<string> parameters)
        {
            // default values
            int testSamplesInPercent = 10, columnIndex_Label = 0;

            if (parameters.Count() > 0 && parameters.Any(
                x => !x.Contains(ParameterName.test.ToString()) && !x.Contains(ParameterName.label.ToString())))
                    throw new ArgumentException($"Only '{ParameterName.test}' and '{ParameterName.label}' are valid parameters for {MainCommand.load}");

            var testParam = parameters.SingleOrDefault(x => x.Contains(ParameterName.test.ToString()));
            if (testParam != null)
                if (!int.TryParse(testParam.Split(':').Last(), out testSamplesInPercent))
                    throw new ArgumentException($"Parameter value {testParam.Split(':').Last()} is not valid." +
                        "Parameter value for 'test' must be an integer between 1 and 99 (inclusive) defining how much percent of the samples will be used as test samples.");

            var labelParam = parameters.SingleOrDefault(x => x.Contains(ParameterName.label.ToString()));
            if (labelParam != null)
                if (!int.TryParse(labelParam.Split(':').Last(), out columnIndex_Label))
                    throw new ArgumentException($"Parameter value {labelParam.Split(':').Last()} is not valid." +
                        "Parameter value for 'label' must be a positive integer defining the index of the column holding the label values (First column index = 0!).");

            return await initializer.SampleSet.LoadSampleSetAsync(samplesFileName, (float)testSamplesInPercent / 100, columnIndex_Label);
        }

        #endregion
    }
}
