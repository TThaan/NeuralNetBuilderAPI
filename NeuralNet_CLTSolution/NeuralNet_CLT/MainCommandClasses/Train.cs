using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeepLearningDataProvider.SampleSetHelpers;
using static NeuralNet_CLT.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNet_CLT.Commandables
{
    public class Train : CommandableBase
    {
        #region Commandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(async () =>
            {
                TrainCommand trainCommand = GetSubCommand<TrainCommand>(parametersAndSubCommand, out var parameters);
                CheckParameters(parameters, Show.InputInfo_Train, ConsoleInputCheck.EnsureNoOrSingleParameter);
                var singleParameter = GetRestrictedParameter(parameters, PresetValue.shuffle, true, $"{MainCommand.train}");    // GetSingleParameter<PresetValue>(parameters);  // Include in GetValidParameter?

                switch (trainCommand)
                {
                    case TrainCommand.Undefined:
                        break;
                    case TrainCommand.start:
                        await TrainAsync(singleParameter);
                        break;
                    case TrainCommand.example:
                        await ExampleTraining(singleParameter);
                        break;
                    default:
                        break;
                }
            });
        }

        #endregion

        #region Sub Command methods

        internal static async Task TrainAsync(bool shuffle = false)
        {
            stopwatch.Reset();
            stopwatch.Start();
            await initializer.Trainer.TrainAsync(shuffle, pathBuilder.Log);
            stopwatch.Stop();

            // await initializer.SaveTrainedNetAsync();
        }
        internal async static Task ExampleTraining(bool shuffle = false)
        {
            // Set Debugging paths

            pathBuilder.SetGeneralPath(System.IO.Path.Combine(pathBuilder.General, "Debugging"));

            // Get samples

            await initializer.SampleSet.LoadSamplesAsync(pathBuilder.SampleSet, 0, null);
            initializer.SampleSet.Initialize(.1m);
                        
            // Get net

            await paramBuilder.LoadNetParametersAsync(pathBuilder.NetParameters);
            initializer.Net.Initialize(initializer.ParameterBuilder.NetParameters);
            // if (!await initializer.LoadNetAsync())
            //     return;        // Always check if the loaded initialized net suits loaded parameters!

            // Get trainer

            await paramBuilder.LoadTrainerParametersAsync(pathBuilder.TrainerParameters);
            initializer.Trainer.Initialize(initializer.ParameterBuilder.TrainerParameters, initializer.Net, initializer.SampleSet);
            initializer.Trainer.PropertyChanged += Trainer_PropertyChanged;

            // Activate logging
            isLogged = true;

            // Train

            Console.WriteLine("Press any button to proceed.");
            // Console.ReadKey();
            await TrainAsync(shuffle);
        }

        #endregion
    }
}
