using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Threading;
using System.ComponentModel;
using CloseAlerts.Proc;

namespace CloseAlerts
{
    public partial class FrmMain : Form
    {
        public static SynchronizationContext s_ctxThread;
        private bool ExitAllow = false;
        BackgroundWorker bg1;
        Monitoring _m;

        const string APP_NAME = "Conbe_CloseAlert";

        public FrmMain()
        {
            InitializeComponent();
            s_ctxThread = WindowsFormsSynchronizationContext.Current;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void 끝내기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            this.Hide();
            Chk_AutoStart();
            Chk_Running();

            StartWork();
        }

        private void StartWork()
        {
            bg1 = new BackgroundWorker();
            bg1.WorkerReportsProgress = true;
            bg1.WorkerSupportsCancellation = true;
            bg1.ProgressChanged += Bg_ProgressChanged;
            bg1.DoWork += Bg_DoWork;
            bg1.RunWorkerCompleted += Bg_RunWorkerCompleted;

            bg1.RunWorkerAsync();
        }

        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _m.Stop();
        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            _m = new Monitoring();
            _m.UpdateStatus += _m_UpdateStatus;
            _m.UpdateMonitor += _m_UpdateMonitor;
            _m.Start();

            while (!bg1.CancellationPending)
            {
            }
        }

        private void _m_UpdateMonitor(string status)
        {
            bg1.ReportProgress(1, status);
        }

        private void _m_UpdateStatus(string status)
        {
            bg1.ReportProgress(0, status);
        }

        private void Bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
                lbl_Status.Text = e.UserState.ToString();
            else if (e.ProgressPercentage == 1)
                lbl_Monitor.Text = e.UserState.ToString();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ExitAllow)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void chk_AutoStart_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_AutoStart.Checked)
            {
                AutoStartReg();
            }
            else
            {
                AutoStartDel();
            }
        }


        public void ShowMyWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Exit()
        {
            if (MessageBox.Show("종료하시겠습니까?", "종료 확인", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                bg1.CancelAsync();
                ExitAllow = true;
                Application.Exit();
            }
        }

        private void Chk_Running()
        {
            string appEXEname = Process.GetCurrentProcess().ProcessName + ".exe";
        }


        private void Chk_AutoStart()
        {
            // Registry 에서 Sub Key를 가져온다
            rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // 만약 AutoRestart 값이 없으면
            if (rkApp.GetValue(APP_NAME) == null)
            {
                // 노 체크
                chk_AutoStart.Checked = false;
            }
            else  // 값이 있으면
            {
                // 체크
                chk_AutoStart.Checked = true;
            }
        }

        RegistryKey rkApp;
        // 오토스타트 등록
        public void AutoStartReg()
        {
            if (rkApp == null)
            {
                rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            }

            rkApp.SetValue(APP_NAME, Application.ExecutablePath.ToString());
        }

        // 오토스타트 삭제
        public void AutoStartDel()
        {
            if (rkApp == null)
            {
                rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            }
            rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rkApp.DeleteValue(APP_NAME, false);
        }

    }
}
