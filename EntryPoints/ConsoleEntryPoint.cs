using DLL_Support.Util;
using Ninject;
using APP_DeskStats.Module;
using APP_DeskStats.Worker;

namespace APP_DeskStats.EntryPoints
{
    class ConsoleEntryPoint
    {
        public static void Run(string[] args)
        {
            // Initialize Ninject (the DI framework)
            var kernel = new StandardKernel(new CommonModule(), new ConsoleModule());

            Paths.ClearTemp();

            // Parse the command-line arguments (and display help text if appropriate)
            //var options = new AutomatedScanningOptions();
            //if (!CommandLine.Parser.Default.ParseArguments(args, options))
            //{
            //    return;
            //}

            // Start a pending worker process
            WorkerManager.Init();

            // Run the scan automation logic
            //var scanning = kernel.Get<AutomatedScanning>(new ConstructorArgument("options", options));
            //scanning.Execute().Wait();
        }
    }
}
