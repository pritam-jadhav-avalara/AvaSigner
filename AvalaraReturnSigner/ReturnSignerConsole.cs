using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Windows.Forms;
using System.Security.Principal;

namespace AvalaraReturnSigner
{
    public partial class ReturnSignerConsole : Form
    {

        ServiceHost Host;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;


        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public ReturnSignerConsole()
        {
            InitializeComponent();
        }
        private void ReturnSignerConsole_Load(object sender, EventArgs e)
        {
            LogInternal("Application String");
            HostService();
            txtLog.Text += "\r\nAvaSigner Started Sucessfully on port 8848";

        }

        void HostService()
        {

            try
            {
                Uri url = new Uri("http://localhost:8848/SignReturn");

                Host = new ServiceHost(typeof(AvalaraReturnSigner), url);

                LogInternal("created Host with :" + url.ToString());

                foreach (ServiceEndpoint EP in Host.Description.Endpoints)
                    EP.Behaviors.Add(new BehaviorAttribute());

                LogInternal("Endpoint attached to Host");

                txtLog.Text += "\r\nStaring AvaSigner";

                Host.Open();

                LogInternal("Host opened");
                LogInternal("Service started Sucessfully");
            }
            catch (Exception ex)
            {
                txtLog.Text += "\r\nError please contact Avalara Support Team";
                LogInternal("\r\nError please contact Avalara Support Team");
                LogInternal("Exception: " + ex.ToString());

            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (Host.State == CommunicationState.Opened)
                Host.Close();

            LogInternal("Host Closed");
            LogInternal("Closing Application");
            Application.Exit();
        }


        void LogInternal(string Message)
        {
            using (var logger = new StreamWriter("signerlogs.ava", true))
            {
                logger.WriteLine(DateTime.Now.ToString() + "     " + Message);
            }

        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            notify.Visible = true;
            notify.ShowBalloonTip(500, "AvaSigner", "AvaSigner running in background", ToolTipIcon.Info);
            this.Hide();
            this.WindowState = FormWindowState.Minimized;

        }

        private void notify_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        public bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}
