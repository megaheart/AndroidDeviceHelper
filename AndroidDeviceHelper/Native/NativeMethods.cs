using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
//using System.Windows.Input;
using AndroidDeviceHelper.Native;

namespace AndroidDeviceHelper
{
    internal class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
        internal static bool SendKey(ScanCodeShort[] scanCodeShorts)
        {
            INPUT[] inputs = new INPUT[scanCodeShorts.Length * 2];
            for(var i = 0; i < scanCodeShorts.Length; i++)
            {
                inputs[i] = new INPUT()
                {
                    type = (uint)InputType.Keyboard,
                    U = new InputUnion()
                    {
                        ki = new KEYBDINPUT()
                        {
                            wScan = scanCodeShorts[i],
                            dwFlags = (KEYEVENTF.KEYDOWN | KEYEVENTF.SCANCODE),
                            //wVk = VirtualKeyShort.KEY_1
                        }
                    }
                };
                inputs[i + scanCodeShorts.Length] = new INPUT()
                {
                    type = (uint)InputType.Keyboard,
                    U = new InputUnion()
                    {
                        ki = new KEYBDINPUT()
                        {
                            wScan = scanCodeShorts[i],
                            dwFlags = (KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE),
                            //wVk = VirtualKeyShort.KEY_1
                        }
                    }
                };
            }
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
            return true;
        }
        
    }
}
