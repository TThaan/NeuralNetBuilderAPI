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
        public abstract Task Execute(IEnumerable<string> parameters);
        
        protected static TCommand GetSubCommand<TCommand>(IEnumerable<string> parameters)
        {
            return parameters.First().Split(Separator_ConsoleInput).First().ToEnum<TCommand>();
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
    }
}
