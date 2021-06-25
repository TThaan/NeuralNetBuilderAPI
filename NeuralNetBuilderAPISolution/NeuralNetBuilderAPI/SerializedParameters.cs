using NeuralNetBuilder.FactoriesAndParameters;
using System;

namespace NeuralNetBuilderAPI
{
    public interface ISerializedParameters
    {
        INetParameters NetParameters { get; set; }
        ITrainerParameters TrainerParameters { get; set; }
        bool UseGlobalParameters { get; set; }
        float WeightMin_Global { get; set; }
        float WeightMax_Global { get; set; }
        float BiasMin_Global { get; set; }
        float BiasMax_Global { get; set; }
    }

    [Serializable]  // Or use json only?
    public class SerializedParameters : ISerializedParameters
    {
        public INetParameters NetParameters { get; set; }
        public ITrainerParameters TrainerParameters { get; set; }
        public bool UseGlobalParameters { get; set; }
        public float WeightMin_Global { get; set; }
        public float WeightMax_Global { get; set; }
        public float BiasMin_Global { get; set; }
        public float BiasMax_Global { get; set; }
    }
}
