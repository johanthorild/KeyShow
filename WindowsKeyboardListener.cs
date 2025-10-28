using System;
using System.Runtime.InteropServices;
using Avalonia.Input;

namespace KeyShow;

#if WINDOWS
public class WindowsKeyboardListener : IKeyboardListener
{
    public event Action<KeyInfo>? OnKeyPressed;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

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
        UnhookWindowsHookEx(_hookId);
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
            GetModuleHandle(curModule.ModuleName), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);

            // Konvertera virtual key code till sträng
            string keyName = VkCodeToString(vkCode);

            var keyInfo = new KeyInfo
            {
                KeyName = keyName,
                Modifiers = GetModifiers()
            };

            OnKeyPressed?.Invoke(keyInfo);
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private Avalonia.Input.KeyModifiers GetModifiers()
    {
        Avalonia.Input.KeyModifiers mods = Avalonia.Input.KeyModifiers.None;
        if ((GetKeyState(VK_SHIFT) & 0x8000) != 0) mods |= Avalonia.Input.KeyModifiers.Shift;
        if ((GetKeyState(VK_CONTROL) & 0x8000) != 0) mods |= Avalonia.Input.KeyModifiers.Control;
        if ((GetKeyState(VK_MENU) & 0x8000) != 0) mods |= Avalonia.Input.KeyModifiers.Alt;
        return mods;
    }

    private string VkCodeToString(int vkCode)
    {
        // Bokstäver och siffror
        if (vkCode >= 0x30 && vkCode <= 0x5A)
            return ((char)vkCode).ToString();

        // Modifier-tangenter
        return vkCode switch
        {
            0x10 => "SHIFT",
            0xA0 => "SHIFT",      // LeftShift
            0xA1 => "SHIFT",      // RightShift
            0x11 => "CTRL",
            0xA2 => "CTRL",       // LeftCtrl
            0xA3 => "CTRL",       // RightCtrl
            0x12 => "ALT",
            0xA4 => "ALT",        // LeftAlt
            0xA5 => "ALT",        // RightAlt
            0x14 => "CAPS",       // Caps Lock
            0x1C => "ENTER",      // Numpad Enter
            0x2E => "DEL",
            0x2D => "INS",
            0x24 => "HOME",
            0x23 => "END",
            0x21 => "PGUP",
            0x22 => "PGDN",
            0x20 => "SPACE",
            0x09 => "TAB",
            0x1B => "ESC",
            0x0D => "ENTER",
            0x25 => "LEFT",
            0x26 => "UP",
            0x27 => "RIGHT",
            0x28 => "DOWN",
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

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    private const int VK_SHIFT = 0x10;
    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12; // ALT
}
#endif
