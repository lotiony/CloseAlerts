using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static CloseAlerts.Proc.WinAppServices;

namespace CloseAlerts.Proc
{
    public class Monitoring
    {
        const string statusFormat = "작동중 ( {0:%d}일 {0:%h}시간 {0:%m}분 {0:%s}초 )";
        const string monitorFormat = "가장매매 [{0:d}] / 주문거부 [{1:d}] / 주문오류 [{2:d}]\r\n기타알럿창 [{3:d}]";
        
        string statusString { get { return string.Format(monitorFormat, _close[0], _close[1], _close[2], _close[3]); } }

        DateTime StartDate;
        int[] _close = { 0, 0, 0, 0 };

        Timer runTimer;
        Timer monTimer;


        public delegate void deleUpdate(string status);
        public event deleUpdate UpdateStatus;
        public event deleUpdate UpdateMonitor;

        public Monitoring()
        {
            StartDate = DateTime.Now;

            runTimer = new Timer();
            runTimer.Interval = 1000;
            runTimer.Elapsed += RunTimer_Elapsed;

            monTimer = new Timer();
            monTimer.Interval = 500;
            monTimer.Elapsed += MonTimer_Elapsed;
        }

        public void Start()
        {
            runTimer.Start();
            monTimer.Start();

            UpdateMonitor(statusString);
        }
        public void Stop()
        {
            runTimer.Stop();
            monTimer.Stop();
        }


        private void MonTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeSpan t = DateTime.Now - StartDate;
            string status = string.Format(statusFormat, t);
            UpdateStatus(status);
        }

        private void RunTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var handle = WinAppServices.FindSubHandle(HtsControls.가장매매);
            if (handle > 0)
            {
                WinAppServices.SendOrder((IntPtr)handle);
                _close[0]++;
                UpdateMonitor(statusString);
            }

            var handle2 = WinAppServices.FindSubHandle(HtsControls.일괄주문);
            if (handle2 > 0)
            {
                WinAppServices.SendOrder((IntPtr)handle2);
                _close[1]++;

                UpdateMonitor(statusString);
            }

            var handle3 = WinAppServices.FindSubHandle(HtsControls.사이렌오류);
            if (handle3 > 0)
            {
                WinAppServices.SendOrderESC((IntPtr)handle3);
                _close[2]++;

                UpdateMonitor(statusString);
            }

            var handle4 = WinAppServices.FindSubHandle(HtsControls.선택된주문);
            if (handle4 > 0)
            {
                WinAppServices.SendOrder((IntPtr)handle4);
                _close[3]++;

                UpdateMonitor(statusString);
            }

            var handle5 = WinAppServices.FindSubHandle(HtsControls.잘못된인수);
            if (handle5 > 0)
            {
                WinAppServices.SendOrder((IntPtr)handle5);
                _close[3]++;

                UpdateMonitor(statusString);
            }

            //var handle3 = WinAppServices.FindSubHandle(HtsControls.주문거부);
            //if (handle3 > 0)
            //{
            //    WinAppServices.SendOrder((IntPtr)handle3);
            //}
        }

    }
}
