using System.Runtime.InteropServices;

namespace Main;

public static class WinAPIWrapper
{
    // https://stackoverflow.com/questions/3571627/show-hide-the-console-window-of-a-c-sharp-console-application
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;

    public static void ToggleConsoleVisibility(bool flag)
    {
        var console = GetConsoleWindow();
        var intFlag = flag ? SW_SHOW : SW_HIDE;
        ShowWindow(console, intFlag);
    }
}