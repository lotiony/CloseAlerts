using System;
using System.Windows.Forms;
using System.Threading;
namespace CloseAlerts
{
    static class Program
    {
		public static EventWaitHandle s_ewh;
		static bool bRun = true;
		public static Form s_FrmMain = null;

		/// <summary>
		/// 해당 애플리케이션의 주 진입점입니다.
		/// </summary>
		[STAThread]
        static void Main()
        {
			try
			{
				s_ewh = EventWaitHandle.OpenExisting("OneInstance.ThisProgram");
				if (s_ewh != null)
				{
					s_ewh.Set();
					return;
				}
			}
			catch { }

			if (s_ewh == null)
			{
				s_ewh = new EventWaitHandle(false, EventResetMode.AutoReset, "OneInstance.ThisProgram");
			}

			Thread thread = new Thread(WaitInstanceEvent);
			thread.IsBackground = true;
			thread.Start();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			s_FrmMain = new FrmMain();
			Application.Run(s_FrmMain);

			s_FrmMain = null;
			bRun = false;
			s_ewh.Set();
        }

		static void WaitInstanceEvent(object state)
		{
			while (bRun)
			{
				s_ewh.WaitOne();
				if (s_FrmMain == null)
				{
					continue;
				}

				FrmMain.s_ctxThread.Post(new SendOrPostCallback((object thisState) =>
				{
					((FrmMain)s_FrmMain).ShowMyWindow();
				}
				), null);

			}
		}

	}
}
