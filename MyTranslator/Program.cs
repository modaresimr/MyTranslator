using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyTranslator
{
    static class Program
    {
        

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] param)
        {
            //            keyboardHook.KeyPressed +=
            //                new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            //            keyboardHook.RegisterHotKey(MyTranslator.ModifierKeys.Control,
            //                Keys.C);
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (param.Any() && param[0] == "setIEVersion")
            {
                Utils.SetIEVersion();
                Thread.Sleep(1000);
            }
            Application.Run(new MyTranslator());
            //Application.Run(new Form1());

        }
        
    }
    }
