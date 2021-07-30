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
            string layerId_String = parameters.SingleOrDefault(x => Equals(x.Split(':').First(), ParameterName.L.ToString()));

            if (layerId_String == null)
                return -1;
            // throw new ArgumentException($"Cannot find a parameter for the layer index. (Expected: {ParameterName.L}:[index (positive integer)]).");

            if (!int.TryParse(layerId_String, out int result))
                throw new ArgumentException($"Cannot transform {layerId_String} into a layer index (positive integer).");

            paramsWithoutLayerId = parameters.Where(x => !Equals(x.Split(':').First(), ParameterName.L.ToString())).ToArray();

            return result;
        }        
        protected static void CheckParameters(IEnumerable<string> parameters, MainCommand mainCommand, params ConsoleInputCheck[] checks)
        {
            // Check if there is no parameter at all.
            if (checks.Contains(ConsoleInputCheck.EnsureNoParameter) && parameters.Count() > 0)
                throw new ArgumentException($"The main command {mainCommand} must be followed by one of the following sub commands: \n" +
                    $"{Enum.GetNames(typeof(ShowCommand)).ToStringFromCollection()}.");

            // check if there is exactly one single parameter.
            if (checks.Contains(ConsoleInputCheck.EnsureSingleParameter) && parameters.Count() == 1)
                throw new ArgumentException($"The main command {mainCommand} must be followed by one of the following sub commands and nothing else: \n" +
                    $"{Enum.GetNames(typeof(ShowCommand)).ToStringFromCollection()}.");
        }

    }
}
