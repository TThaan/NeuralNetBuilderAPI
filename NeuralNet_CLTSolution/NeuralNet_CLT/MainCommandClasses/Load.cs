﻿using MachineLearningDataProvider.SampleSetHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNet_CLT.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNet_CLT.Commandables
{
    public class Load : CommandableBase
    {
        #region Commandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(async () =>
            {
                LoadAndSaveCommand loadCommand = GetSubCommand<LoadAndSaveCommand>(parametersAndSubCommand, out var parameters);
                if (loadCommand == LoadAndSaveCommand.samples)
                    CheckParameters(parameters, Show.InputInfo_Load);
                else
                    CheckParameters(parameters, Show.InputInfo_Load, ConsoleInputCheck.EnsureNoParameter);

                switch (loadCommand)
                {
                    case LoadAndSaveCommand.all:
                        await LoadSamplesAndNetAsync();
                        break;
                    case LoadAndSaveCommand.net0:
                        await initializer.LoadNetAsync(pathBuilder.InitializedNet);
                        break;
                    case LoadAndSaveCommand.net1:
                        await initializer.LoadTrainedNetAsync(pathBuilder.TrainedNet);
                        break;
                    case LoadAndSaveCommand.samples:
                        await LoadSampleSetAsync(pathBuilder.SampleSet, parameters);
                        break;
                    case LoadAndSaveCommand.par:
                        await LoadAllParametersAsync();
                        break;
                    case LoadAndSaveCommand.netpar:
                        await paramBuilder.LoadNetParametersAsync(pathBuilder.NetParameters);
                        break;
                    case LoadAndSaveCommand.trainerpar:
                        await paramBuilder.LoadTrainerParametersAsync(pathBuilder.TrainerParameters);
                        break;
                    default:
                        break;
                }
            });
        }

        #endregion

        #region Sub Command methods

        internal static async Task LoadAllParametersAsync()
        {
            await paramBuilder.LoadNetParametersAsync(pathBuilder.NetParameters);
            await paramBuilder.LoadTrainerParametersAsync(pathBuilder.TrainerParameters);
        }
        internal static async Task LoadSamplesAndNetAsync()
        {
            await LoadSampleSetAsync(pathBuilder.SampleSet, null);
            await initializer.LoadNetAsync(pathBuilder.InitializedNet);
        }
        /// <summary>
        /// Default values if you parameters miss an input helper:
        /// testSamplesInPercent = 10, columnIndex_Label = 0.
        /// 
        /// Task: Separate loading data from initializing (i.e. preparing)!
        /// </summary>
        internal static async Task LoadSampleSetAsync(string samplesFileName, IEnumerable<string> parameters)
        {
            // default values
            int testSamplesInPercent = 10, columnIndex_Label = 0;

            if (parameters.Count() > 0 && parameters.Any(
                x => !x.Contains(ParameterName.split.ToString()) && !x.Contains(ParameterName.label.ToString())))
                    throw new ArgumentException($"Only '{ParameterName.split}' and '{ParameterName.label}' are valid parameters for {MainCommand.load}");

            var testParam = parameters.SingleOrDefault(x => x.Contains(ParameterName.split.ToString()));
            if (testParam != null)
                if (!int.TryParse(testParam.Split(':').Last(), out testSamplesInPercent))
                    throw new ArgumentException($"Parameter value {testParam.Split(':').Last()} is not valid." +
                        "Parameter value for 'test' must be an integer between 1 and 99 (inclusive) defining how much percent of the samples will be used as test samples.");

            var labelParam = parameters.SingleOrDefault(x => x.Contains(ParameterName.label.ToString()));
            if (labelParam != null)
                if (!int.TryParse(labelParam.Split(':').Last(), out columnIndex_Label))
                    throw new ArgumentException($"Parameter value {labelParam.Split(':').Last()} is not valid." +
                        "Parameter value for 'label' must be a positive integer defining the index of the column holding the label values (First column index = 0!).");

            await initializer.SampleSet.LoadSamplesAsync(samplesFileName, columnIndex_Label, null);
            initializer.SampleSet.Initialize((decimal)testSamplesInPercent / 100);
        }

        #endregion
    }
}
