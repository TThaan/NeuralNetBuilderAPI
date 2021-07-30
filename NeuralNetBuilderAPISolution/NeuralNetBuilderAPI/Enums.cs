namespace NeuralNetBuilderAPI
{
    public enum MainCommand
    {
        Undefined,
        path, show, create, load, save, param, layer, log,
        // stand alone - commands (no sub command, parameters etc needed):
        train
    }
    public enum TrainCommand
    {
        Undefined,
        example,
        start
    }
    public enum ShowCommand
    {
        Undefined,
        help, settings, par, netpar, trainerpar, net,
        samples
    }
    public enum PathCommand
    {
        Undefined,
        prefix, suffix, reset, general, net0, net1, samples, netpar, trainerpar, log
    }
    public enum LoadAndSaveCommand
    {
        Undefined,
        all, net0, net1, samples, par, netpar, trainerpar,
    }
    public enum CreateCommand
    {
        Undefined,
        all, net, trainer, samples, par, netpar, trainerpar
    }
    public enum ParameterCommand
    {
        Undefined, set
    }
    public enum LayerCommand
    {
        Undefined, add, del, left, right
    }
    public enum LogCommand
    {
        on, off
    }
    //public enum InputHelper
    //{
    //}
    public enum ParameterName
    {
        Undefined,
        // net parameters
        wInit,
        // layer parameters
        act, wMax, wMin, bMax, bMin, N,
        // trainer parameters
        cost, epochs, Eta, dEta,

        L, // layer index,
        test, label // "load samples parameters"
    }
    public enum ConsoleInputCheck
    {
        Undefined,
        EnsureNoParameter,      // No parameter at all is allowed.
        EnsureSingleParameter,  // No more nor less than one parameter is allowed.
        // SubCommandIsDismissed   // No sub command is needed.
    }
}