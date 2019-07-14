﻿using NAPS2.Automation;
using NAPS2.Modules;
using NAPS2.Remoting.Worker;
using Ninject;
using Ninject.Parameters;

namespace NAPS2.EntryPoints
{
    /// <summary>
    /// The entry point for NAPS2.Console.exe, the NAPS2 CLI.
    /// </summary>
    public static class ConsoleEntryPoint
    {
        public static void Run(string[] args)
        {
            // Initialize Ninject (the DI framework)
            var kernel = new StandardKernel(new CommonModule(), new ConsoleModule(), new StaticDefaultsModule());

            Paths.ClearTemp();

            // Parse the command-line arguments (and display help text if appropriate)
            var options = new AutomatedScanningOptions();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return;
            }
            
            // Start a pending worker process
            kernel.Get<IWorkerFactory>().Init();

            // Run the scan automation logic
            var scanning = kernel.Get<AutomatedScanning>(new ConstructorArgument("options", options));
            scanning.Execute().Wait();
        }
    }
}
