using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace MyTranslator
{
    public partial class MyTranslator : Form
    {

        public MyTranslator()
        {
            InitializeComponent();

        }
        protected override void WndProc(ref Message m)
        {
            clipboardViewer.WndProc(ref m);
            base.WndProc(ref m);
        }
            private void Form1_Load(object sender, EventArgs e)
        {
            if (Control.IsKeyLocked(Keys.CapsLock))
                toggleCapsLock();
            var a = "<script>function isIE () {" +
                        "    var myNav = navigator.userAgent.toLowerCase();" +
                        "    return (myNav.indexOf('msie') != -1) ? parseInt(myNav.split('msie')[1]) : false;" +
                        "}</script>";


            webBrowser2.DocumentCompleted += (p, q) =>
            {
                int ieversion = (int)webBrowser2.Document.InvokeScript("isIE");
                if (ieversion < webBrowser2.Version.Major)
                {
                    MessageBox.Show("IE version is incorrect, It should be set to newer version. Current ie:" + ieversion);
                    button2_Click(null, null);

                }
            };
            webBrowser2.DocumentText =
                a + "<div id='ali' style='position:absolute;top:0px;text-align:center;top:40%;width:100%;height:100%;background-color: white;z-index: 1999;'><h1>Quick Translation: Capslock</h1><h2>Developed By AliModaresi</h2><h3>My Translator version 4.0</h3></div>";

            webBrowser1.Url = new Uri("https://translate.google.com/");
            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;
            webBrowser1.ScriptErrorsSuppressed = true;




            var t = new System.Timers.Timer();
            t.Elapsed += timeElapsed;
            timeElapsed(null, null);
            t.Interval = 6 * 1000;
            t.Start();
            _hookCallback = new BasicHook.LowLevelKeyboardProc(HookCallback);
            hook = BasicHook.SetHook(_hookCallback);

            clipboardViewer.Register(this.Handle);
            clipboardViewer.CopyRecevier += s => {
                if (System.DateTime.Now - lastCopyRequestTime < TimeSpan.FromSeconds(2))
                    log(s);
            };

        }
        ClipboardViewer clipboardViewer = new ClipboardViewer();

        private void timeElapsed(object sender, ElapsedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                try
                {
                    BasicHook.UnhookWindowsHookEx(hook);
                    hook = BasicHook.SetHook(_hookCallback);
                }
                catch
                {

                    throw;
                }
            }));
        }

        public static void forceSetForegroundWindow(IntPtr hWnd, IntPtr mainThreadId)
        {
            try
            {
                var a = IntPtr.Zero;
                IntPtr foregroundThreadID = User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), out a);
                if (foregroundThreadID != mainThreadId)
                {
                    User32.AttachThreadInput(mainThreadId, foregroundThreadID, true);
                    User32.SetForegroundWindow(hWnd);
                    User32.AttachThreadInput(mainThreadId, foregroundThreadID, false);
                }
                else
                    User32.SetForegroundWindow(hWnd);
            }
            catch
            {

            }
        }
        static TextSelectionReader tsr = new TextSelectionReader();
        DateTime lastkey = DateTime.MinValue;
        private static BasicHook.LowLevelKeyboardProc _hookCallback;
        private IntPtr hook;
        private DateTime lastCopyRequestTime = System.DateTime.Now;

        private string getcurrentapp()
        {
            IntPtr hWnd = User32.GetForegroundWindow();
            IntPtr procId = new IntPtr();
            User32.GetWindowThreadProcessId(hWnd, out procId);
            var proc = Process.GetProcessById((int)procId);
            return proc.MainWindowTitle;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)BasicHook.WM_KEYUP)
            {
                Keys key = (Keys)Marshal.ReadInt32(lParam);
                Console.WriteLine("key up: " + key);
                if (key == Keys.CapsLock)//not necessary
                    return (IntPtr)1;

            }
            if (nCode >= 0 && wParam == (IntPtr)BasicHook.WM_KEYDOWN)
            {


                Keys key = (Keys)Marshal.ReadInt32(lParam);
                Console.WriteLine("key down: " + key);

                if (key == Keys.CapsLock)
                {
                    webBrowser1.Invoke((Action)delegate ()
                    {
                        try
                        {
                            //copyFromApp();
                            copyFromApp2();
                            
                        }
                        catch (Exception e) { Console.Error.WriteLine(e); }
                    });

                    return (IntPtr)1;
                }
            }
            return IntPtr.Zero;//BasicHook.CallNextHookEx(hook, nCode, wParam, lParam);
        }
        private void copyFromApp2()
        {
            lastCopyRequestTime = System.DateTime.Now;
            var f = User32.GetForegroundWindow();
            //User32.SetForegroundWindow(f);
            SendKeys.SendWait("^c");
            SendKeys.Flush();
            
        
        }
        private void copyFromApp()
        {
            //if (DateTime.Now.Subtract(lastkey).TotalMilliseconds < 400)
            //  return;
            //lastkey = DateTime.Now;
            String originalText = null;
            if (Clipboard.ContainsText())
            {
                //                    Thread.Sleep(1000);
                originalText = Clipboard.GetText(TextDataFormat.UnicodeText);
                Console.WriteLine("before copy clipboard contains:" + originalText);
            }
            var app = getcurrentapp();
            if (app.Contains("MyTranslator"))
            {
                hideintro();
                //MessageBox.Show("salam");
                return;
            }
            //Clipboard.SetText(" ");
            var f = User32.GetForegroundWindow();
            //User32.SetForegroundWindow(f);
            String newtext = originalText;
            bool valid = false;
            for (var i = 0; i < 10; i++)
            {
                SendKeys.SendWait("^c");
                SendKeys.Flush();
                if (Clipboard.ContainsText())
                {
                    valid = true;
                    newtext = Clipboard.GetText(TextDataFormat.UnicodeText);
                    if (newtext != originalText)
                        break;
                }
                Thread.Sleep(100);
            }
            if (valid)
                log(newtext);
            if (originalText != null)
                Clipboard.SetText(originalText, TextDataFormat.UnicodeText);  
            
        }

        private void Gkh_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.CapsLock)
            {
                e.Handled = true;
            }
        }

        private void Gkh_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.CapsLock)
                {
                    e.Handled = true;
                    var app = getcurrentapp();
                    if (app.Contains("MyTranslator"))
                    {
                        hideintro();
                        //MessageBox.Show("salam");
                        return;
                    }
                    //Clipboard.SetText(" ");
                    //ClipboardViewer.sendCopy(this.Handle, s => log(s));
                    copyFromApp2();

                    return;
                }


                if (DateTime.Now.Subtract(lastkey).TotalMilliseconds < 400)
                    return;
                lastkey = DateTime.Now;
                if (!checkBox1.Checked)
                    return;

                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)

                    if (Clipboard.ContainsText())
                    {
                        string text = Clipboard.GetText(TextDataFormat.UnicodeText);
                        log(text);
                    }
            }
            catch
            {
            }
        }

        private void toggleCapsLock()
        {
            try
            {
                SendKeys.Send("{CAPSLOCK}");

                if (Control.IsKeyLocked(Keys.CapsLock)) // Checks Capslock is on
                {
                    const int KEYEVENTF_EXTENDEDKEY = 0x1;
                    const int KEYEVENTF_KEYUP = 0x2;

                    User32.keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                    User32.keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                    User32.keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                    User32.keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                    User32.keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                    //    User32.keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,(UIntPtr)0);
                }
            }
            catch
            {
            }
        }

        
        public void Activate2()
        {
            Process currentProcess = Process.GetCurrentProcess();
            IntPtr hWnd = currentProcess.MainWindowHandle;
            if (hWnd != User32.InvalidHandleValue)
            {
                User32.SetForegroundWindow(hWnd);
                User32.ShowWindow(hWnd, User32.SW_MAXIMIZE);
            }
        }
        private void log(string s)
        {
            try
            {
                Console.WriteLine(s);
                hideintro();
                if (String.IsNullOrWhiteSpace(s))
                    return;
                webBrowser1.Document.InvokeScript("translate", new object[] { s.Replace('\r', ' ').Replace('\n', ' ') });

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
                forceSetForegroundWindow(Handle, new IntPtr(System.Threading.Thread.CurrentThread.ManagedThreadId));
                Activate2();
                TopMost = true;
                Activate();
                Focus();
                BringToFront();
                TopMost = false;
                DoMouseClick();
            }
            catch
            {
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public void DoMouseClick()
        {
            try
            {
                //Call the imported function with the cursor's current position
                int X = Cursor.Position.X;
                int Y = Cursor.Position.Y;
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);

            }
            catch (Exception e)
            {

            }
        }

        //...other code needed for the application
        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            try
            {
                webBrowser2.Visible = false;
                var hideElems = new string[] { "gb_Od", "gb_Wa", "notification-area", "input-button-container", "gp-footer", "feedback-link" };


                var script = String.Join(" ",
                    hideElems.Select(p => "var elem=document.getElementsByClassName('" + p + "')[0];if(elem&&elem.style)elem.style.display='none';"));
                //webBrowser1.DocumentText= webBrowser1.ObjectForScriptingDocumentText.Replace("</body>", script + "</body>");
                HtmlElement head = webBrowser1.Document.GetElementsByTagName("body")[0];
                HtmlElement styleEl = webBrowser1.Document.CreateElement("style");
                styleEl.InnerText = ".frame{height: 100vh !important}.page{max-width: 95%;}.gt-lc.gt-lc-mobile.show-as-one-card {position: absolute;top: 50vh;z-index: 999;}";
                head.AppendChild(styleEl);
                //HtmlElement scriptE2 = webBrowser1.Document.CreateElement("script");
                //scriptE2.SetAttribute("src", "https://www.scribens.com/scribens-integration.js")
                //head.AppendChild(scriptE2);
                HtmlElement scriptEl = webBrowser1.Document.CreateElement("script");
                scriptEl.InnerText = "function hideElements() { " + script + " }" +
                    "function translate(text){elem.value=text;}" +
                    "var elem=document.getElementById('source');" +
                    "elem.oninput=function(){elem.value=elem.value.split(String.fromCharCode(10)).join(' '); };";
                //scriptEl.InnerText += "translate(\""+scriptEl.InnerText+"\");";
                //element.text = "function sayHello() { alert('hello') }";
                head.AppendChild(scriptEl);
                




                webBrowser1.Document.InvokeScript("hideElements");
                webBrowser1.DocumentCompleted -= WebBrowser1_DocumentCompleted;

            }
            catch
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            hideintro();
        }

        private void hideintro()
        {
            webBrowser2.Visible = false;
            button1.Visible = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void MyTranslator_FormClosing(object sender, FormClosingEventArgs e)
        {
            BasicHook.UnhookWindowsHookEx(hook);
            Environment.Exit(0);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            var s = new ProcessStartInfo();
            s.Verb = "runas";
            s.FileName = System.IO.Path.GetFullPath(System.Reflection.Assembly.GetExecutingAssembly().Location);
            s.Arguments = "setIEVersion";
            // Utils.SetIEVersion();
            s.UseShellExecute = true;
            Process.Start(s);
            Thread.Sleep(1000);
            Environment.Exit(0);

        }

        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}
