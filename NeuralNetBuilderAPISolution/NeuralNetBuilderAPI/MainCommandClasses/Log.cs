using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NeuralNetBuilderAPI.Program;   // To give this ICommandable access to Program. initializer/pathBuilder/paramBuilder. (Later: Use DI!)

namespace NeuralNetBuilderAPI.Commandables
{
    public class Log : CommandableBase
    {
        #region Commandable

        public override async Task Execute(IEnumerable<string> parametersAndSubCommand)
        {
            await Task.Run(() =>
            {
                LogCommand logCommand = GetSubCommand<LogCommand>(parametersAndSubCommand, out var parameters);
                CheckParameters(parameters, Show.InputInfo_Log, ConsoleInputCheck.EnsureNoParameter);

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
    }
}