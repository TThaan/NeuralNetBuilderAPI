using NeuralNetBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class ChangeParameter : CommandableBase
    {
        #region ICommandable

        public override async Task Execute(IEnumerable<string> parameters)
        {
            await Task.Run(() =>
            {
                CheckParameters(parameters);
                ParameterCommand parameterCommand = GetSubCommand<ParameterCommand>(parameters);

                int layerId = GetLayerId(parameters, out var paramsWithoutId);                

                switch (parameterCommand)
                {
                    case ParameterCommand.set:
                        Set(parameters, layerId);
                        break;
                    default:
                        break;
                }
            });
        }

        #endregion

        #region Parameter Methods

        public static void Set(IEnumerable<string> parameters, int layerId)
        {
            foreach (var p in parameters)
            {
                ParameterName name = p.GetParameterName();
                string value = p.GetParameterValue_String();

                // Trainer Parameters

                switch (name)
                {
                    case ParameterName.Eta:
                        paramBuilder.SetLearningRate(float.Parse(value));
                        return;
                    case ParameterName.dEta:
                        paramBuilder.SetLearningRateChange(float.Parse(value));
                        return;
                    case ParameterName.cost:
                        paramBuilder.SetCostType(int.Parse(value));
                        return;
                    case ParameterName.epochs:
                        paramBuilder.SetEpochs(int.Parse(value));
                        return;
                }

                // Net parameters

                switch (name)
                {
                    case ParameterName.wInit:
                        paramBuilder.SetWeightInitType(int.Parse(value));
                        return;
                        // Or glob as layerId?
                        //case ParameterName.wMinGlob:
                        //    SetWeightMin_Globally(float.Parse(parameterValue));
                        //    break;
                        //case ParameterName.wMaxGlobally:
                        //    SetWeightMax_Globally(float.Parse(parameterValue));
                        //    break;
                        //case ParameterName.bMinGlob:
                        //    SetBiasMin_Globally(float.Parse(parameterValue));
                        //    break;
                        //case ParameterName.bMaxGlob:
                        //    SetBiasMax_Globally(float.Parse(parameterValue));
                        //    break;
                }

                // Layer Parameters

                if (layerId < 0 || layerId > paramBuilder.LayerParametersCollection.Count - 1)
                    throw new ArgumentException("Missing an existing layer id!");

                switch (name)
                {
                    case ParameterName.act:
                        paramBuilder.SetActivationTypeAtLayer(layerId, int.Parse(value));
                        return;
                    case ParameterName.N:
                        paramBuilder.SetNeuronsAtLayer(layerId, int.Parse(value));
                        return;
                    case ParameterName.wMax:
                        paramBuilder.SetWeightMaxAtLayer(layerId, float.Parse(value));
                        return;
                    case ParameterName.wMin:
                        paramBuilder.SetWeightMinAtLayer(layerId, float.Parse(value));
                        return;
                    case ParameterName.bMax:
                        paramBuilder.SetBiasMaxAtLayer(layerId, float.Parse(value));
                        return;
                    case ParameterName.bMin:
                        paramBuilder.SetBiasMinAtLayer(layerId, float.Parse(value));
                        return;
                };


                throw new ArgumentException($"Parameter {name} unknown.");
            }
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
                    $"The main command {MainCommand.param} must be followed by one of the following sub commands: \n" +
                    $"{Enum.GetNames(typeof(ParameterCommand)).ToStringFromCollection()}.");
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
