using NeuralNetBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class Log : CommandableBase
    {
        #region ICommandable

        public override async Task Execute(IEnumerable<string> parameters)
        {
            await Task.Run(() =>
            {
                CheckParameters(parameters);
                LogCommand logCommand = GetSubCommand<LogCommand>(parameters);

                switch (logCommand)
                {
                    case LogCommand.on:
                        LogOn();
                        break;
                    case LogCommand.off:
                        LogOff();
                        break;
                    default:
                        break;
                }
            });
        }

        #endregion

        #region Sub Command methods

        internal static void LogOn()
        {
            initializer.IsLogged = true;
            Console.WriteLine("Logging activated.");
        }
        internal static void LogOff()
        {
            initializer.IsLogged = false;
            Console.WriteLine("Logging deactivated.");
        }

        #endregion

        #region helpers

        private static void CheckParameters(IEnumerable<string> parameters)
        {
            CheckSubCommand(parameters);
        }
        // in base class?
        private static void CheckSubCommand(IEnumerable<string> parameters)
        {
            if (parameters.Count() == 0)
                throw new ArgumentException($"The main command {MainCommand.log} must be followed by one of the following sub commands: \n" +
                    $"{Enum.GetNames(typeof(LogCommand)).ToStringFromCollection()}.");
            
            else if (parameters.Count() > 1)
                throw new ArgumentException($"The main command {MainCommand.show} must be followed by one of the following sub commands and nothing else: \n" +
                    $"{Enum.GetNames(typeof(LogCommand)).ToStringFromCollection()}.");
        }

        #endregion
    }
}
