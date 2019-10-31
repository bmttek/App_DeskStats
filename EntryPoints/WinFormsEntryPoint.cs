using DLL_Support.Logging;
using DLL_Support.Util;
using DLL_Support.WinForms;
using Ninject;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DLL_Support.Config;
using Ninject.Parameters;
using APP_DeskStats.Module;
using APP_DeskStats.Worker;
using APP_DeskStats.WinForms;

namespace APP_DeskStats.EntryPoints
{
    class WinFormsEntryPoint
    {
        public static void Run(string[] args)
        {
            // Initialize Ninject (the DI framework)
            var kernel = new StandardKernel(new CommonModule(), new WinFormsModule());
            var appConfig = new AppConfig()
            {
                AppName = "Library Helper Suite",
                AppSection = "Signature",
                Debug = false
            };
            //SettingsData settingsData = SettingUtils.LoadSettings("Library Helper Suite",false,"program.settings","Signature");
            Paths.ClearTemp();

            // Parse the command-line arguments and see if we're doing something other than displaying the main form
            IParameter appNamekey = new ConstructorArgument("appNamekey", "Name of Staff Application");
            var lifecycle = kernel.Get<Lifecycle>(new IParameter[]
              { new ConstructorArgument("appConfig", appConfig)});
            // ConfigManager configManager = new ConfigManager(appConfig);

            lifecycle.ParseArgs(args);
            lifecycle.ExitIfRedundant();

            // Start a pending worker process
            WorkerManager.Init();

            // Set up basic application configuration
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += UnhandledException;
            TaskScheduler.UnobservedTaskException += UnhandledTaskException;

            // Show the main form
            var formFactory = kernel.Get<IFormFactory>();
            var desktop = formFactory.Create<FDesktop>();
            Invoker.Current = desktop;
            Application.Run(desktop);
        }

        private static void UnhandledTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.FatalException("An error occurred that caused the task to terminate.", e.Exception);
            e.SetObserved();
        }

        private static void UnhandledException(object sender, ThreadExceptionEventArgs e)
        {
            Log.FatalException("An error occurred that caused the application to close.", e.Exception);
        }

    }
}
