using System.Collections.Generic;
using System.Threading.Tasks;
using static NeuralNet_CLT.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNet_CLT.Commandables
{
    public class Layer : CommandableBase
    {
        #region Commandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(() =>
            {
                LayerCommand layerCommand = GetSubCommand<LayerCommand>(parametersAndSubCommand, out var parameters);
                CheckParameters(parameters, Show.InputInfo_Layer, ConsoleInputCheck.EnsureSingleParameter);
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
    }
}
