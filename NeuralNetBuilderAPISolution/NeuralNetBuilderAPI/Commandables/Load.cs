using NeuralNetBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class Load : CommandableBase
    {
        #region ICommandable

        public override async Task Execute(IEnumerable<string> parameters)
        {
            await Task.Run(async () =>
            {
                CheckParameters(parameters);
                LoadAndSaveCommand loadCommand = GetSubCommand<LoadAndSaveCommand>(parameters);

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

        #region helpers

        private static void CheckParameters(IEnumerable<string> parameters)
        {
            CheckSubCommand(parameters);
            //CheckParameterStructure(parameters);
        }
        // in base class?
        private static void CheckSubCommand(IEnumerable<string> parameters)
        {
            if (parameters.Count() == 0)
                throw new ArgumentException(
                    $"The main command {MainCommand.load} must be followed by one of the following sub commands: \n" +
                    $"{Enum.GetNames(typeof(LoadAndSaveCommand)).ToStringFromCollection()}.");
        }
        //private static void CheckParameterStructure(IEnumerable<string> parameters)
        //{
        //    if (parameters.Count() > 2)
        //        throw new ArgumentException(
        //            $"The main command {MainCommand.load} must be followed by a sub command and in case of the sub command {LoadAndSaveCommand.net} an optional parameter ('{PresetValue.append}').\n" +
        //            "Anything else is invalid");
        //}

        #endregion
    }
}
