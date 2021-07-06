using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace NeuralNetBuilderAPI
{
    internal class CommandNames
    {
        #region Class creating method

        public static CommandNames GetDefaultCommandNames()
        {
            return new CommandNames();
        }
        public static async Task<CommandNames> LoadCommandNamesAsync(string path)
        {
            CommandNames result;

            if (File.Exists(path))
            {
                // Load command names
                var json = await File.ReadAllTextAsync(path);
                result = JsonConvert.DeserializeObject<CommandNames>(json);
            }
            else
            {
                // Save and use default command names
                result = new CommandNames();
                var json = JsonConvert.SerializeObject(result);
                await File.WriteAllTextAsync(json, path);
            }

            return result;
        }
        public static async Task SaveCommandNamesAsync(string path, CommandNames commandNames)
        {
            var json = JsonConvert.SerializeObject(commandNames);
            await File.WriteAllTextAsync(json, path);
        }

        #endregion

        #region command names

        public string SetGeneralPath { get; } = "path";
        public string SetFileNamePrefix { get; } = "path prefix";
        public string SetFileNameSuffix { get; } = "path suffix";
        public string ResetPaths { get; } = "path reset";
        public string UseGeneralPathAndDefaultNames { get; } = "path general";
        public string SetInitializedNetPath { get; } = "path net -0";
        public string SetTrainedNetPath { get; } = "path net -1";
        public string SetSampleSetPath { get; } = "path samples";
        public string SetNetParametersPath { get; } = "path net -p";
        public string SetTrainerParametersPath { get; } = "path trainer -p";
        public string SetSampleSetParametersPath { get; } = "path samples -p";
        public string SetLogPath { get; } = "path log";
        
        public string ShowHelp { get; } = "show help";
        public string ShowSettings { get; } = "show settings";
        public string ShowAllParameters { get; } = "show -p";
        public string ShowNetParameters { get; } = "show net -p";
        public string ShowTrainerParameters { get; } = "show trainer -p";
        public string ShowSampleSetParameters { get; } = "show samples -p";

        public string CreateAllParameters { get; } = "create -p";
        public string CreateNetParameters { get; } = "create net -p";
        public string CreateTrainerParameters { get; } = "create trainer -p";
        public string CreateSampleSetParameters { get; } = "create samples -p";
        public string CreateSamplesNetAndTrainer { get; } = "create";
        public string CreateNet { get; } = "create net";
        public string CreateTrainer { get; } = "create trainer";
        public string CreateSampleSet { get; } = "create samples";

        public string LoadSamplesNetAndTrainer { get; } = "load";
        public string LoadInitializedNet { get; } = "load net -0";
        public string LoadTrainedNet { get; } = "load net -1";
        public string LoadSampleSet { get; } = "load samples";
        public string LoadAllParameters { get; } = "load -p";
        public string LoadNetParameters { get; } = "load net -p";
        public string LoadTrainerParameters { get; } = "load trainer -p";
        public string LoadSampleSetParameters { get; } = "load samples -p";

        public string SaveSamplesNetAndTrainer { get; } = "save";
        public string SaveAllParameters { get; } = "save -p";
        public string SaveNetParameters { get; } = "save net -p";
        public string SaveTrainerParameters { get; } = "save trainer -p";
        public string SaveSampleSetParameters { get; } = "save samples -p";
        public string SaveInitializedNet { get; } = "save net -0";
        public string SaveTrainedNet { get; } = "save net -1";
        public string SaveSampleSet { get; } = "save samples";

        public string Log { get; } = "log on";
        public string Unlog { get; } = "log off";
        public string Train { get; } = "train";
        public string TestTraining { get; } = "test";

        public string ChangeParameter { get; set; } = "p ";
        //public string ChangeSampleSetParameter { get; set; } = "samples";
        //public string ChangeNetParameter { get; set; } = "net";
        //public string ChangeLayerParameter { get; set; } = "layer";
        //public string ChangeTrainerParameter { get; set; } = "trainer";

        #endregion
    }
}
