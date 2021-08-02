using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
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
            await initializer.TrainAsync(initializer.SampleSet, shuffle);
            stopwatch.Stop();

            await initializer.SaveTrainedNetAsync();
        }
        internal async static Task ExampleTraining(bool shuffle = false)
        {
            // Get samples

            if (!await initializer.SampleSet.LoadSampleSetAsync(pathBuilder.SampleSet, .1f, 0))
                return;             // Always check if the loaded sample set suits the ... parameters!
                        
            // Get net

            if (!await paramBuilder.LoadNetParametersAsync())
                return;
            if (!await initializer.CreateNetAsync())
                return;
            // if (!await initializer.LoadNetAsync())
            //     return;        // Always check if the loaded initialized net suits loaded parameters!

            // Get trainer

            if (!await paramBuilder.LoadTrainerParametersAsync())
                return;
            if (!await initializer.CreateTrainerAsync(initializer.SampleSet))
                return;
            initializer.Trainer.TrainerStatusChanged += Trainer_StatusChanged_EventHandlingMethod;

            // Activate logging
            Log.LogOn();

            // Train

            Console.WriteLine("Press any button to proceed.");
            Console.ReadKey();
            await TrainAsync(shuffle);
        }

        #endregion
    }
}
