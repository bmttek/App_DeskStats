using APP_DeskStats.EntryPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APP_DeskStats
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            typeof(WinFormsEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
        }
    }
}
