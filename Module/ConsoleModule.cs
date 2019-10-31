using DLL_Support.Automation;
using DLL_Support.Dependencies;
using DLL_Support.Operation;
using DLL_Support.Util;
using Ninject.Modules;

namespace APP_DeskStats.Module
{
    public class ConsoleModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IErrorOutput>().To<ConsoleErrorOutput>();
            Bind<IOverwritePrompt>().To<ConsoleOverwritePrompt>();
            Bind<IOperationProgress>().To<ConsoleOperationProgress>();
            Bind<IComponentInstallPrompt>().To<ConsoleComponentInstallPrompt>();
        }
    }
}
