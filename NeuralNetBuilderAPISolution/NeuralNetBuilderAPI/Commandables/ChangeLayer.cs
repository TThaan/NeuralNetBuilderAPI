using NeuralNetBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class ChangeLayer : CommandableBase
    {
        #region ICommandable

        public override async Task Execute(IEnumerable<string> parameters)
        {
            await Task.Run(() =>
            {
                CheckParameters(parameters);
                LayerCommand layerCommand = GetSubCommand<LayerCommand>(parameters);

                int layerId = GetLayerId(parameters, out var paramsWithoutId);

                switch (layerCommand)
                {
                    case LayerCommand.add:
                        paramBuilder.AddLayerAfter(layerId);
                        break;
                    case LayerCommand.del:
                        paramBuilder.DeleteLayer(layerId);
                        break;
                    case LayerCommand.left:
                        paramBuilder.MoveLayerLeft(layerId);
                        break;
                    case LayerCommand.right:
                        paramBuilder.MoveLayerRight(layerId);
                        break;
                    default:
                        break;
                }
            });
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
                    $"The main command {MainCommand.layer} must be followed by one of the following sub commands: \n" +
                    $"{Enum.GetNames(typeof(LayerCommand)).ToStringFromCollection()}.");
        }
        //private static void CheckParameterStructure(IEnumerable<string> parameters)
        //{
        //    if (parameters.Count() > 2)
        //        throw new ArgumentException(
        //            $"The main command {MainCommand.path} must be followed by a sub command and except in the case of the sub command {PathCommand.reset} a full file name.\n");
        //}

        #endregion
    }
}
