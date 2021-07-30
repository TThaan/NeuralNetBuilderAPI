using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuralNetBuilderAPI.Commandables
{
    public interface ICommandable
    {
        Task Execute(IEnumerable<string> parameters);
    }
}