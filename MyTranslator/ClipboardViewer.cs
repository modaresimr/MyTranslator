using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyTranslator
{
    class ClipboardViewer
    {

        public delegate void CopyRecevied(String text);
        public event CopyRecevied CopyRecevier;
        IntPtr _ClipboardViewerNext;
        public ClipboardViewer()
        {

        }
        public void Register(IntPtr whandle)
        {
            _ClipboardViewerNext = User32.SetClipboardViewer(whandle);
        }
        public void UnRegister(IntPtr whandle)
        {
            User32.ChangeClipboardChain(whandle, _ClipboardViewerNext);
        }

        public void WndProc(ref Message m)
        {
            switch ((Win32.Msgs)m.Msg)
            {
                //
                // The WM_DRAWCLIPBOARD message is sent to the first window 
                // in the clipboard viewer chain when the content of the 
                // clipboard changes. This enables a clipboard viewer 
                // window to display the new content of the clipboard. 
                //
                case Win32.Msgs.WM_DRAWCLIPBOARD:

                    Debug.WriteLine("WindowProc DRAWCLIPBOARD: " + m.Msg, "WndProc");

                    GetClipboardData();

                    //
                    // Each window that receives the WM_DRAWCLIPBOARD message 
                    // must call the SendMessage function to pass the message 
                    // on to the next window in the clipboard viewer chain.
                    //
                    User32.SendMessage(_ClipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    break;


                //
                // The WM_CHANGECBCHAIN message is sent to the first window 
                // in the clipboard viewer chain when a window is being 
                // removed from the chain. 
                //
                case Win32.Msgs.WM_CHANGECBCHAIN:
                    Debug.WriteLine("WM_CHANGECBCHAIN: lParam: " + m.LParam, "WndProc");

                    // When a clipboard viewer window receives the WM_CHANGECBCHAIN message, 
                    // it should call the SendMessage function to pass the message to the 
                    // next window in the chain, unless the next window is the window 
                    // being removed. In this case, the clipboard viewer should save 
                    // the handle specified by the lParam parameter as the next window in the chain. 

                    //
                    // wParam is the Handle to the window being removed from 
                    // the clipboard viewer chain 
                    // lParam is the Handle to the next window in the chain 
                    // following the window being removed. 
                    if (m.WParam == _ClipboardViewerNext)
                    {
                        //
                        // If wParam is the next clipboard viewer then it
                        // is being removed so update pointer to the next
                        // window in the clipboard chain
                        //
                        _ClipboardViewerNext = m.LParam;
                    }
                    else
                    {
                        User32.SendMessage(_ClipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    }
                    break;

                default:
                    //
                    // Let the form process the messages that we are
                    // not interested in
                    //

                    break;

            }

        }
        /// <summary>
		/// Show the clipboard contents in the window 
		/// and show the notification balloon if a link is found
		/// </summary>
		private void GetClipboardData()
        {
            //
            // Data on the clipboard uses the 
            // IDataObject interface
            //
            IDataObject iData = new DataObject();
            string strText = "clipmon";

            try
            {
                iData = Clipboard.GetDataObject();
            }
            catch (System.Runtime.InteropServices.ExternalException externEx)
            {
                // Copying a field definition in Access 2002 causes this sometimes?
                Debug.WriteLine("InteropServices.ExternalException: {0}", externEx.Message);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            /*
            // 
            // Get RTF if it is present
            //
            if (iData.GetDataPresent(DataFormats.Rtf))
            {
                ctlClipboardText.Rtf = (string)iData.GetData(DataFormats.Rtf);

                if (iData.GetDataPresent(DataFormats.Text))
                {
                    strText = "RTF";
                }
            }
            else*/
            {
                // 
                // Get Text if it is present
                //
                if (iData.GetDataPresent(DataFormats.Text))
                {
                    var text = (string)iData.GetData(DataFormats.Text);
                    CopyRecevier?.Invoke(text);

                    strText = "Text";

                    Debug.WriteLine((string)iData.GetData(DataFormats.Text));
                }/*
                else
                {
                    //
                    // Only show RTF or TEXT
                    //
                    ctlClipboardText.Text = "(cannot display this format)";
                }*/
            }


        }
    }



}
