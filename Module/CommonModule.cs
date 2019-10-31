using APP_DeskStats.Worker;
using DLL_Support.Config;
using DLL_Support.Email;
using DLL_Support.Helpers;
using DLL_Support.ILS;
using DLL_Support.ILS.SIP2;
using DLL_Support.Logging;
using DLL_Support.Ninject;
using DLL_Support.Operation;
using DLL_Support.Platform;
using DLL_Support.Stats;
using DLL_Support.Stats.API;
using DLL_Support.Util;
using DLL_Support.WinForms;
using DLL_Support.Worker;
using Ninject;
using Ninject.Modules;
using NLog;
using System.Diagnostics;
using ILogger = DLL_Support.Logging.ILogger;

namespace APP_DeskStats.Module
{
    class CommonModule : NinjectModule
    {
        public override void Load()
        {
            AppConfig appConfig = new AppConfig()
            {
                AppName = "Library Helper Suite",
                AppSection = "Signature",
                Debug = false
            };
            Bind<IEmailProviderFactory>().To<NinjectEmailProviderFactory>().WithConstructorArgument("appConfig", appConfig);
            Bind<IStatProviderFactory>().To<NinjectStatProviderFactory>().WithConstructorArgument("appConfig", appConfig);
            Bind<IIlsProviderFactory>().To<NinjectIlsProviderFactory>().WithConstructorArgument("appConfig", appConfig);
            //Bind<IStorageProviderFactory>().To<NinjectStorageProviderFactory>().WithConstructorArgument("appConfig", appConfig);
            Bind<IConfigManager>().To<ConfigManager>().InSingletonScope().WithConstructorArgument("appConfig", appConfig);
            Bind<IStatProvider>().To<ApiStatProvider>().InSingletonScope().WithConstructorArgument("appConfig", appConfig);
            Bind<IIlsProvider>().To<Sip2IlsProvider>().InSingletonScope().WithConstructorArgument("appConfig", appConfig);
            //Bind<IStorageProvider>().To<MssqlStorageProvider>().InSingletonScope().WithConstructorArgument("appConfig", appConfig);
            Bind<IWorkerServiceFactory>().To<NinjectWorkerServiceFactory>();
            Bind<IWorkerContext>().ToMethod(ctx => WorkerManager.NextWorker());
            Bind<IFormFactory>().To<NinjectFormFactory>().WithConstructorArgument("appConfig", appConfig);
            Bind<NotificationManager>().ToSelf().InSingletonScope().WithConstructorArgument("appConfig", appConfig);
            Bind<IOperationFactory>().To<NinjectOperationFactory>().WithConstructorArgument("appConfig", appConfig);
            Bind<ILogger>().To<NLogLogger>().InSingletonScope().WithConstructorArgument("appConfig", appConfig);
            Bind<ChangeTracker>().ToSelf().InSingletonScope().WithConstructorArgument("appConfig", appConfig);
            Bind<StatHelper>().ToSelf().WithConstructorArgument("appConfig", appConfig);
            //Bind<StillImage>().ToSelf().InSingletonScope();
            // Bind<IBlankDetector>().To<ThresholdBlankDetector>();
            //Bind<IAutoSave>().To<AutoSave>();

            Log.Logger = new NLogLogger(appConfig);
            if (PlatformCompat.System.CanUseWin32)
            {
                Log.EventLogger = Kernel.Get<WindowsEventLogger>();
            }
#if DEBUG
            Debug.Listeners.Add(new NLogTraceListener());
#endif
        }
    }
}
