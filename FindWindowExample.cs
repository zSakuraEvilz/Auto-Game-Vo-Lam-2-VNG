class ExeLockFounder
{
    const uint WM_SETTEXT = 0x000C;
    delegate bool EnumDelegate(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string className, string lpszWindow);

    [DllImport("user32.dll")]
    static extern bool EnumThreadWindows(int dwThreadId, EnumDelegate lpfn, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPStr)] string lParam);

    private static IntPtr GetWindowByClassName(IEnumerable<IntPtr> windows, string className)
    {
        foreach (var window in windows)
        {
            var sb = new StringBuilder(256);
            GetClassName(window, sb, sb.Capacity);
            if (sb.ToString() == className)
                return window;
        }
        return IntPtr.Zero;
    }

    static IEnumerable<IntPtr> EnumerateProcessWindowHandles(Process process)
    {
        var handles = new List<IntPtr>();
        foreach (ProcessThread thread in process.Threads)
            EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
        return handles;
    }

    private readonly IntPtr _editHandle;

    public ExeLockFounder()
    {
        var processes = Process.GetProcessesByName("Setup");
        var proc = Array.Find(processes, x => string.Equals(x.MainWindowTitle, "ExeLock", StringComparison.OrdinalIgnoreCase));

        var windows = EnumerateProcessWindowHandles(proc);
        var hWnd = GetWindowByClassName(windows, "TFormPassDialog");
        _editHandle = FindWindowEx(hWnd, IntPtr.Zero, "TEdit", null);
    }

    public void SendText(string message)
    {
        SendMessage(_editHandle, WM_SETTEXT, IntPtr.Zero, message);
    }
}
