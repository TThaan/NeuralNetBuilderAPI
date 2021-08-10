using NeuralNetBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.GlobalConstants;

namespace NeuralNetBuilderAPI.Commandables
{
    public abstract class CommandableBase   // : ICommandable
    {
        public abstract Task Execute(IEnumerable<string> parametersAndSubcommand);
        
        protected static TCommand GetSubCommand<TCommand>(IEnumerable<string> parametersAndSubCommand, out IEnumerable<string> parameters)
        {
            // Check if there is no parameter at all.
            if (parametersAndSubCommand.Count() == 0)
                throw new ArgumentException($"This main command must be followed by one of the following sub commands: \n" +
                    $"{Enum.GetNames(typeof(TCommand)).Skip(1).ToStringFromCollection()}.");    // Skip(1) = skipping '..Command.Undefined'

            parameters = parametersAndSubCommand.Skip(1);
            return parametersAndSubCommand.First().Split(Separator_ConsoleInput).First().ToEnum<TCommand>();
        }
        protected static int GetLayerId(IEnumerable<string> parameters, out string[] paramsWithoutLayerId)
        {
            paramsWithoutLayerId = null;
            string layerId_String = parameters.SingleOrDefault(x => Equals(x.Split(Separator_Parameter).First(), ParameterName.L.ToString()));

            if (layerId_String == null)
            {
                paramsWithoutLayerId = parameters.ToArray();
                return -1;
            }
            // throw new ArgumentException($"Cannot find a parameter for the layer index. (Expected: {ParameterName.L}:[index (positive integer)]).");

            if (!int.TryParse(layerId_String.Split(Separator_Parameter).Last(), out int result))
                throw new ArgumentException($"Cannot transform {layerId_String} into a layer index (positive integer).");

            paramsWithoutLayerId = parameters.Where(x => !Equals(x.Split(':').First(), ParameterName.L.ToString())).ToArray();

            return result;
        }
        protected static TParam GetSingleParameter<TParam>(IEnumerable<string> parameters)
        {
            TParam result;
            string singleParameter = parameters.Count() == 0 ? null : parameters.First().GetParameterValue_String();

            if (typeof(TParam).IsEnum)
                result = singleParameter == null ? default : (TParam)(object)singleParameter.ToEnum<PresetValue>();
            else
                result = singleParameter == null ? default : (TParam)(object)singleParameter;

            return result;
        }
        /// <summary>
        /// Make/Use a mapper between valid PresetValue[] values and T[] validParameters when you need more than one valid value?
        /// </summary>
        protected static T GetRestrictedParameter<T>(IEnumerable<string> parameters, PresetValue validValue, T validParameter, string source)//, PresetValue passed vs valid
        {
            T result = default;
            PresetValue passedValue;

            try
            {
                passedValue = GetSingleParameter<PresetValue>(parameters);
            }
            catch
            {
                throw new ArgumentException($"{GetSingleParameter<string>(parameters)} is not a valid parameter for {source}.\n" +
                    $"Add parameter {validValue} or no parameter at all.");
            }

            if (passedValue == validValue)
                result = validParameter;
            else if (passedValue != PresetValue.undefined)
                throw new ArgumentException($"{passedValue} is not a valid parameter for {source}.\n" +
                    $"Add parameter {PresetValue.indented} or no parameter at all.");

            return result;
        }

        protected static void CheckParameters(IEnumerable<string> parameters, string inputInfo, params ConsoleInputCheck[] checks)   // Pass a list of needed/valid parameters to show in exception message?
        {
            // Check if there is no parameter at all.
            if (checks.Contains(ConsoleInputCheck.EnsureNoParameter) && parameters.Count() != 0)
                throw new ArgumentException($"No parameters are allowed here.\n" +
                    $"{inputInfo}");

            // Check if there is exactly one single parameter.
            if (checks.Contains(ConsoleInputCheck.EnsureSingleParameter) && parameters.Count() != 1)
                throw new ArgumentException($"A single parameter is needed and no more.\n" +
                    $"{inputInfo}");

            // Check if there is no or one single parameter.
            if (checks.Contains(ConsoleInputCheck.EnsureNoOrSingleParameter) && parameters.Count() > 1)
                // task: Is the message true for 0 parameters?
                throw new ArgumentException($"A single parameter is needed or none at all.\n" +
                    $"{inputInfo}");

            // Check if there is at least one parameter.
            if (checks.Contains(ConsoleInputCheck.EnsureOneOrMoreParameters) && parameters.Count() < 1)
                throw new ArgumentException($"At least one parameter is missing.\n" +
                    $"{inputInfo}");

            // Check if there are multiple parameters.
            if (checks.Contains(ConsoleInputCheck.EnsureMultipleParameters) && parameters.Count() < 2)
                throw new ArgumentException($"Some parameters are missing.\n" +
                    $"{inputInfo}");
        }
    }
}
