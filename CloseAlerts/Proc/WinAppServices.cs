using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HWND = System.IntPtr;

namespace CloseAlerts.Proc
{
    static class WinAppServices
    {
        public enum HtsControls
        {
            가장매매 = 1,
            위험고지 = 2,
            일괄주문 = 3,
            사이렌오류 = 4,
            선택된주문 = 5,
            잘못된인수 = 6
        }

        public static void SendOrder(IntPtr Hwnd)
        {
            NativeWin32.SetForegroundWindow(Hwnd);
            Thread.Sleep(50);

            NativeWin32.SendMessage(Hwnd, NativeWin32.BM_CLICK, 0, 1);
            Thread.Sleep(50);
        }
        public static void SendOrderESC(IntPtr Hwnd)
        {
            NativeWin32.SetForegroundWindow(Hwnd);
            Thread.Sleep(50);

            string keystring = "{ESC}";
            System.Windows.Forms.SendKeys.Send(keystring);
        }

        public static string GetWindowInfo(IntPtr hWnd)
        {
            StringBuilder strbTitle = new StringBuilder(255);
            NativeWin32.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
            string strTitle = strbTitle.ToString();

            StringBuilder sClass = new StringBuilder(100);
            NativeWin32.GetClassName(hWnd, sClass, 100);
            string strClass = sClass.ToString();

            return $"{sClass}{strbTitle}";
        }

        public static string GetHandleText(IntPtr hWnd)
        {
            StringBuilder strbTitle = new StringBuilder(255);
            NativeWin32.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
            return strbTitle.ToString();
        }

        public static long FindSubHandle(HtsControls control)
        {
            IntPtr nDeshWndHandle = NativeWin32.GetDesktopWindow();

            var allChildWindows = new WindowHandleInfo(nDeshWndHandle).GetAllChildHandles();

            foreach (var handle in allChildWindows)
            {
                string windowInfo = GetWindowInfo(handle);

                switch (control)
                {
                    case HtsControls.가장매매:
                        if (windowInfo == "#32770주문 오류" && NativeWin32.IsWindowVisible(handle))
                        {
                            IntPtr phw = handle;
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_CHILD);
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);
                            return phw.ToInt64();
                        }
                        break;

                    case HtsControls.일괄주문:
                        if (windowInfo == "#32770거부메시지 확인" && NativeWin32.IsWindowVisible(handle))
                        {
                            IntPtr phw = handle;
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_CHILD);
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_CHILD);
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_CHILD);
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);
                            return phw.ToInt64();
                        }
                        break;


                    case HtsControls.위험고지:
                        if (windowInfo == "#32770해외선물옵션호가주문 위험고지" && NativeWin32.IsWindowVisible(handle))
                        {
                            IntPtr phw = handle;
                            for (int i = 1; i <= 2; i++)
                                phw = NativeWin32.GetWindow(phw, NativeWin32.GW_CHILD);

                            for (int i = 1; i <= 3; i++)
                                phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);

                            return phw.ToInt64();
                        }
                        break;

                    case HtsControls.사이렌오류:
                        if (windowInfo == "#32770" && NativeWin32.IsWindowVisible(handle))
                        {
                            IntPtr phw = handle;
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_CHILD);
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);

                            windowInfo = GetWindowInfo(phw);
                            if (windowInfo == "Button거부 알림소리")
                            {
                                return handle.ToInt64();
                            }
                        }
                        break;

                    case HtsControls.선택된주문:
                        if (windowInfo == "#32770안내" && NativeWin32.IsWindowVisible(handle))
                        {
                            IntPtr phw = handle;
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_CHILD);

                            windowInfo = GetWindowInfo(phw);
                            if (windowInfo.Contains("Static선택된 주문"))
                            {
                                phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);
                                phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);      // Button확인

                                return phw.ToInt64();
                            }
                        }
                        break;

                    case HtsControls.잘못된인수:
                        if (windowInfo == "#32770영웅문Global" && NativeWin32.IsWindowVisible(handle))
                        {
                            IntPtr phw = handle;
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_CHILD);
                            var tmpHwnd = phw.ToInt64();    // 일단 확인 버튼 핸들 먼저 할당  // Button확인

                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);
                            phw = NativeWin32.GetWindow(phw, NativeWin32.GW_HWNDNEXT);      

                            windowInfo = GetWindowInfo(phw);
                            if (windowInfo.Contains("Static잘못된 인수"))        // 정확한 창이 맞는지 검증해서 리턴한다.
                            {
                                return tmpHwnd;
                            }
                        }
                        break;

                }

            }

            return 0;
        }
    }
}
