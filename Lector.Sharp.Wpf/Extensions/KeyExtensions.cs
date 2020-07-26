using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lector.Sharp.Wpf.Extensions
{
    public static class KeyExtensions
    {
        public static bool IsDigit(this Key key, LowLevelKeyboardListener keyboard)
        {
            if (keyboard.IsHardwareKeyDown(LowLevelKeyboardListener.VirtualKeyStates.VK_SHIFT) ||
                keyboard.IsHardwareKeyDown(LowLevelKeyboardListener.VirtualKeyStates.VK_CONTROL))
                return false;

            return key >= Key.D0 && key <= Key.D9 || key >= Key.NumPad0 && key <= Key.NumPad9;
        }

        public static bool IsCharacter(this Key key, LowLevelKeyboardListener keyboard)
        {
            if (keyboard.IsHardwareKeyDown(LowLevelKeyboardListener.VirtualKeyStates.VK_CONTROL))
                return false;

            return key >= Key.A && key <= Key.Z;
        }
    }
}