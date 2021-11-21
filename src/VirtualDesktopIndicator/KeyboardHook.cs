using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualDesktopIndicator
{
    public class KeyboardHook
    {
        private static readonly InterceptKeys.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static Action _left;
        private static Action _right;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private const int VK_WIN = 91;
        private const int VK_SHIFT = 160;
        private const int VK_CTRL = 162;
        private const int VK_LEFT = 37;
        private const int VK_RIGHT = 39;

        private static bool winPressed = false;
        private static bool ctrlPressed = false;
        private static bool shiftPressed = false;


        public static void Attach(Action left, Action right)
        {
            _hookID = InterceptKeys.SetHook(_proc);
            _left = left;
            _right = right;
        }

        public static void Detach()
        {
            if (_hookID != IntPtr.Zero)
                InterceptKeys.UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                if (lParam.vkCode == VK_WIN)
                    winPressed = true;
                else if (lParam.vkCode == VK_CTRL)
                    ctrlPressed = true;
                else if (lParam.vkCode == VK_SHIFT)
                    shiftPressed = true;
                else if (lParam.vkCode == VK_LEFT && winPressed && ctrlPressed && shiftPressed)
                {
                    Thread thread = new Thread(() => _left());
                    thread.Start();
                    thread.Join();
                    return (IntPtr)1;
                }
                else if (lParam.vkCode == VK_RIGHT && winPressed && ctrlPressed && shiftPressed)
                {
                    Thread thread = new Thread(() => _right());
                    thread.Start();
                    thread.Join();
                    return (IntPtr)1;
                }
            }
            else if (nCode >= 0 && (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP))
            {
                if (lParam.vkCode == VK_WIN)
                    winPressed = false;
                else if (lParam.vkCode == VK_CTRL)
                    ctrlPressed = false;
                else if (lParam.vkCode == VK_SHIFT)
                    shiftPressed = false;
            }

            return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, ref lParam);
        }
    }
}
