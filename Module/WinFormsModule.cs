using DLL_Support.Dependencies;
using DLL_Support.Operation;
using DLL_Support.Util;
using DLL_Support.WinForms;
using Ninject.Modules;

namespace APP_DeskStats.Module
{
    public class WinFormsModule : NinjectModule
    {
        public override void Load()
        {
            //Bind<IPdfPasswordProvider>().To<WinFormsPdfPasswordProvider>();
            Bind<IErrorOutput>().To<MessageBoxErrorOutput>();
            Bind<IOverwritePrompt>().To<WinFormsOverwritePrompt>();
            Bind<IOperationProgress>().To<WinFormsOperationProgress>().InSingletonScope();
            Bind<IComponentInstallPrompt>().To<WinFormsComponentInstallPrompt>();
        }
    }
}
