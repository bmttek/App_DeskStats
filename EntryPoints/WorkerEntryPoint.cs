using DLL_Support.Logging;
using DLL_Support.Util;
using DLL_Support.WinForms;
using Ninject;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel;
using APP_DeskStats.Worker;
using APP_DeskStats.Module;

namespace APP_DeskStats.EntryPoints
{
    public static class WorkerEntryPoint
    {
        public static void Run(string[] args)
        {
            try
            {
#if DEBUG
                // Debugger.Launch();
#endif

                // Initialize Ninject (the DI framework)
                var kernel = new StandardKernel(new CommonModule(), new WinFormsModule());

                // Expect a single argument, the parent process id
                if (args.Length != 1 || !int.TryParse(args[0], out int procId) || !IsProcessRunning(procId))
                {
                    return;
                }

                // Set up basic application configuration
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += UnhandledException;
                TaskScheduler.UnobservedTaskException += UnhandledTaskException;

                // Set up a form for the worker process
                // A parent form is needed for some operations, namely 64-bit TWAIN scanning
                var form = new BackgroundForm();
                Invoker.Current = form;

                // Connect to the main NAPS2 process and listen for assigned work
                string pipeName = string.Format(WorkerManager.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id);
                using (var host = new ServiceHost(typeof(WorkerService)))
                {
                    host.Description.Behaviors.Add(new ServiceFactoryBehavior(() => kernel.Get<WorkerService>()));
                    host.AddServiceEndpoint(typeof(IWorkerService),
                        new NetNamedPipeBinding { ReceiveTimeout = TimeSpan.FromHours(24), SendTimeout = TimeSpan.FromHours(24) }, pipeName);
                    host.Open();
                    // Send a character to stdout to indicate that the process is ready for work
                    Console.Write('k');
                    Application.Run(form);
                }
            }
            catch (Exception ex)
            {
                Console.Write('k');
                Log.FatalException("An error occurred that caused the worker application to close.", ex);
                Environment.Exit(1);
            }
        }

        private static bool IsProcessRunning(int procId)
        {
            try
            {
                var proc = Process.GetProcessById(procId);
                return !proc.HasExited;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static void UnhandledTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.FatalException("An error occurred that caused the worker task to terminate.", e.Exception);
            e.SetObserved();
        }

        private static void UnhandledException(object sender, ThreadExceptionEventArgs e)
        {
            Log.FatalException("An error occurred that caused the worker to close.", e.Exception);
        }
    }
}
