using System;
using System.Runtime.InteropServices;

namespace KeyShow
{
    public class WindowsKeyboardListener : IKeyboardListener
    {
        public event Action<KeyInfo>? OnKeyPressed;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;

        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;

        public WindowsKeyboardListener()
        {
            _proc = HookCallback;
        }

        public void Start()
        {
            _hookId = SetHook(_proc);
        }

        public void Stop()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }

        private readonly object _modsLock = new();
        private readonly System.Collections.Generic.HashSet<int> _modsDown = new();

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return CallNextHookEx(_hookId, nCode, wParam, lParam);

            var kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            int vkCode = (int)kb.vkCode;

            bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;
            bool isKeyUp = wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP;

            bool isModifier = vkCode == VK_SHIFT || vkCode == 0xA0 || vkCode == 0xA1 ||
                              vkCode == VK_CONTROL || vkCode == 0xA2 || vkCode == 0xA3 ||
                              vkCode == VK_MENU || vkCode == 0xA4 || vkCode == 0xA5;

            if (isKeyDown)
            {

                if (isModifier)
                {
                    lock (_modsLock)
                    {
                        if (_modsDown.Contains(vkCode))
                            return CallNextHookEx(_hookId, nCode, wParam, lParam);
                        _modsDown.Add(vkCode);
                    }

                    string keyName = VkCodeToString(vkCode, GetModifiers());
                    OnKeyPressed?.Invoke(new KeyInfo { KeyName = keyName, Modifiers = GetModifiers() });
                    return CallNextHookEx(_hookId, nCode, wParam, lParam);
                }


                var currentMods = GetModifiers();
                string keyName2 = VkCodeToString(vkCode, currentMods);
                if (string.IsNullOrWhiteSpace(keyName2))
                {
                    if (vkCode >= 0x41 && vkCode <= 0x5A)
                        keyName2 = ((char)vkCode).ToString().ToUpperInvariant();
                    else if (vkCode >= 0x30 && vkCode <= 0x39)
                        keyName2 = ((char)vkCode).ToString();
                    else if (vkCode >= 0x60 && vkCode <= 0x69)
                        keyName2 = "NUM" + (vkCode - 0x60).ToString();
                    else
                        keyName2 = $"VK_{vkCode}";
                }

                var keyInfo = new KeyInfo { KeyName = keyName2, Modifiers = currentMods };
                OnKeyPressed?.Invoke(keyInfo);
            }
            else if (isKeyUp)
            {

                if (isModifier)
                {
                    lock (_modsLock)
                    {
                        if (_modsDown.Contains(vkCode))
                            _modsDown.Remove(vkCode);
                    }
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        private Avalonia.Input.KeyModifiers GetModifiers()
        {
            Avalonia.Input.KeyModifiers mods = Avalonia.Input.KeyModifiers.None;
            if ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0) mods |= Avalonia.Input.KeyModifiers.Shift;
            if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0) mods |= Avalonia.Input.KeyModifiers.Control;
            if ((GetAsyncKeyState(VK_MENU) & 0x8000) != 0) mods |= Avalonia.Input.KeyModifiers.Alt;
            return mods;
        }

        private string VkCodeToString(int vkCode, Avalonia.Input.KeyModifiers currentMods)
        {
            // If the key is a letter, handle it ourselves so Shift/Caps/Ctrl combos
            // produce a readable letter instead of control characters.
            if (vkCode >= 0x41 && vkCode <= 0x5A)
            {
                char ch = (char)vkCode;
                return ch.ToString().ToUpperInvariant();
            }

            // Numbers (top row) - when Ctrl is pressed we still want the digit shown.
            if (vkCode >= 0x30 && vkCode <= 0x39)
                return ((char)vkCode).ToString();

            // Attempt layout-aware translation for keys that may be layout-dependent
            // (OEM keys, punctuation). Use ToUnicodeEx with the current keyboard layout.
            try
            {
                var keyboardState = new byte[256];
                if (GetKeyboardState(keyboardState))
                {
                    // Ensure modifier bits reflect real-time physical state by using GetAsyncKeyState
                    // which is appropriate inside a global hook.
                    if ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)
                        keyboardState[VK_SHIFT] = 0x80;
                    if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0)
                        keyboardState[VK_CONTROL] = 0x80;
                    if ((GetAsyncKeyState(VK_MENU) & 0x8000) != 0)
                        keyboardState[VK_MENU] = 0x80;

                    uint scan = MapVirtualKey((uint)vkCode, 0);
                    var sb = new System.Text.StringBuilder(8);
                    IntPtr hkl = GetKeyboardLayout(0);
                    int res = ToUnicodeEx((uint)vkCode, scan, keyboardState, sb, sb.Capacity, 0, hkl);
                    if (res > 0)
                    {
                        var s = sb.ToString();
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            // If ToUnicodeEx produced any control characters (e.g. Ctrl+X -> U+0018)
                            // treat that as a non-printable result and fall back to our VK mapping.
                            bool hasControl = false;
                            foreach (var ch in s)
                            {
                                if (char.IsControl(ch))
                                {
                                    hasControl = true;
                                    break;
                                }
                            }
                            if (!hasControl)
                                return s.ToUpperInvariant();
                        }
                    }
                }
            }
            catch
            {
                // fall through to fallback mapping
            }

            // Direct mappings for common keys. This avoids relying exclusively on ToUnicode
            // which can sometimes be fragile in hook context; we already tried ToUnicodeEx above.
            // Letters
            if (vkCode >= 0x41 && vkCode <= 0x5A)
                return ((char)vkCode).ToString().ToUpperInvariant();

            // Numbers (top row)
            if (vkCode >= 0x30 && vkCode <= 0x39)
                return ((char)vkCode).ToString();

            // Numpad
            if (vkCode >= 0x60 && vkCode <= 0x69)
                return "NUM" + (vkCode - 0x60).ToString();

            // Function keys
            if (vkCode >= 0x70 && vkCode <= 0x87)
                return "F" + (vkCode - 0x6F).ToString();

            return vkCode switch
            {
                0x08 => "BACK",
                0x09 => "TAB",
                0x0D => "ENTER",
                0x10 => "SHIFT",
                0xA0 => "SHIFT",
                0xA1 => "SHIFT",
                0x11 => "CTRL",
                0xA2 => "CTRL",
                0xA3 => "CTRL",
                0x12 => "ALT",
                0xA4 => "ALT",
                0xA5 => "ALT",
                0x14 => "CAPS",
                0x20 => "SPACE",
                0x21 => "PGUP",
                0x22 => "PGDN",
                0x23 => "END",
                0x24 => "HOME",
                0x25 => "LEFT",
                0x26 => "UP",
                0x27 => "RIGHT",
                0x28 => "DOWN",
                0x1B => "ESC",
                0x2C => "PRINTSCREEN",
                0x2D => "INS",
                0x2E => "DEL",
                0x5B => "LWIN",
                0x5C => "RWIN",
                0x5D => "APPS",
                0x90 => "NUMLOCK",
                0x91 => "SCROLLLOCK",
                0x70 => "F1",
                0x71 => "F2",
                0x72 => "F3",
                0x73 => "F4",
                0x74 => "F5",
                0x75 => "F6",
                0x76 => "F7",
                0x77 => "F8",
                0x78 => "F9",
                0x79 => "F10",
                0x7A => "F11",
                0x7B => "F12",
                // OEM / punctuation keys (names are generic; layout-specific chars vary)
                0xBA => ";",
                0xBB => "=",
                0xBC => ",",
                0xBD => "-",
                0xBE => ".",
                0xBF => "/",
                0xC0 => "`",
                0xDB => "[",
                0xDC => "\\",
                0xDD => "]",
                0xDE => "'",
                _ => $"VK_{vkCode}"
            };
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12; // ALT
    }
}
