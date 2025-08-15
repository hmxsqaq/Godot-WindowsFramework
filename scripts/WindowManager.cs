using System.Collections.Generic;
using System.Linq;
using Godot;

namespace windows_framework.scripts;

public partial class WindowManager : Node
{
    public static WindowManager Instance { get; private set; }

    private readonly List<GameWindow> _managedWindows = [];

    public override void _Ready() => Instance = this;

    public void LaunchNewWindow(PackedScene windowScene, Vector2I position, Vector2I size) => LaunchNewWindow(windowScene, new Rect2I(position, size));

    public void LaunchNewWindow(PackedScene windowScene, Rect2I rect)
    {
        if (windowScene == null)
        {
            GD.PrintErr("[GameWindowManager]: GameWindowScene is not set.");
            return;
        }
        
        var newWindow = windowScene.Instantiate<GameWindow>();
        if (newWindow == null)
        {
            GD.PrintErr("[GameWindowManager]: Failed to instantiate GameWindow.");
            return;
        }
        
        newWindow.FocusEntered += () => OnWindowFocused(newWindow);
        newWindow.FocusExited += () => GD.Print($"[GameWindowManager]: Window {newWindow.GetWindowId()} lost focus.");
        newWindow.CloseRequested += () => OnWindowCloseRequested(newWindow);
        
        _managedWindows.Add(newWindow);
        GetTree().Root.AddChild(newWindow);
        newWindow.Popup(rect);
    }

    private void OnWindowFocused(GameWindow window)
    {
        if (!_managedWindows.Contains(window)) return;

        _managedWindows.Remove(window);
        _managedWindows.Add(window);
        GD.Print($"[GameWindowManager]: Window {window.GetWindowId()} get focused.");
        PrintWindowsOrder();
    }
    
    private void OnWindowCloseRequested(GameWindow window)
    {
        _managedWindows.Remove(window);
        window.QueueFree();
        GD.Print($"[GameWindowManager]: Window {window.GetWindowId()} closed. Remaining: {_managedWindows.Count}");
        PrintWindowsOrder();
    }

    private void PrintWindowsOrder()
    {
        var order = string.Join(" <- ", _managedWindows.Select(w => w.GetWindowId()));
        GD.Print($"[GameWindowManager]: Windows order: {order}");
    }
}