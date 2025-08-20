using System.Collections.Generic;
using System.Linq;
using Godot;
using windows_framework.scripts.game_window.behaviors;

namespace windows_framework.scripts.game_window;

public partial class WindowManager : Node
{
    public static WindowManager Instance { get; private set; }

    private readonly PackedScene _baseWindowScene = GD.Load<PackedScene>("res://scenes/game_window.tscn");
    
    private readonly Dictionary<BehaviorType, PackedScene> _behaviorScenes = new()
    {
        { BehaviorType.Movable, GD.Load<PackedScene>("res://scenes/behaviors/movable.tscn") },
        { BehaviorType.Resizable, GD.Load<PackedScene>("res://scenes/behaviors/resizable.tscn") },
        { BehaviorType.WindowInfo, GD.Load<PackedScene>("res://scenes/behaviors/window_info.tscn") },
        { BehaviorType.Passable, GD.Load<PackedScene>("res://scenes/behaviors/passable.tscn") }
    };
    
    private readonly List<BaseWindow> _managedWindows = [];
    private List<BaseWindow> _unpassableWindows = [];
    
    private bool _isUnpassableWindowsDirty = false;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            GD.PrintErr("[GameWindowManager]: Instance already exists. Destroying the new instance.");
            QueueFree();
            return;
        }
        Instance = this;
    }

    public BaseWindow CreateWindow(WindowConfig windowConfig)
    {
        if (_baseWindowScene == null) 
        {
            GD.PrintErr("[GameWindowManager]: Base window scene is null.");
            return null;
        }
        
        if (windowConfig == null) 
        {
            GD.PrintErr("[GameWindowManager]: WindowConfig is null.");
            return null;
        }
        
        var newWindow = _baseWindowScene.Instantiate<BaseWindow>();
        if (newWindow == null)
        {
            GD.PrintErr("[GameWindowManager]: Failed to instantiate BaseWindow.");
            return null;
        }
        
        newWindow.FocusEntered += () => OnWindowFocused(newWindow);
        newWindow.FocusExited += () => GD.Print($"[GameWindowManager]: Window {newWindow.GetWindowId()} lost focus.");
        newWindow.CloseRequested += () => OnWindowCloseRequested(newWindow);

        foreach (var behavior in windowConfig.Behaviors)
        {
            if (!behavior.Value) continue;
            if (!_behaviorScenes.TryGetValue(behavior.Key, out var behaviorScene))
            {
                GD.PrintErr($"[GameWindowManager]: Behavior {behavior.Key} not found.");
                continue;
            }
            var behaviorInstance = behaviorScene.Instantiate<Behavior>();
            if (behaviorInstance == null)
            {
                GD.PrintErr($"[GameWindowManager]: Failed to instantiate behavior {behavior.Key}.");
                continue;
            }
            newWindow.AddBehavior(behavior.Key, behaviorInstance);
        }
        
        newWindow.SetTitle(windowConfig.Title);
        newWindow.SetMinSize(windowConfig.MinSize);
        
        GetTree().Root.AddChild(newWindow);
        newWindow.Popup(new Rect2I(windowConfig.Position, windowConfig.Size));
        _managedWindows.Add(newWindow);
        _isUnpassableWindowsDirty = true;
        
        return newWindow;
    }

    public void SetWindowRect(BaseWindow window, Rect2I targetRect, DisplayServer.WindowResizeEdge? activeEdge = null)
    {
        if (window == null)
        {
            GD.PrintErr("[GameWindowManager]: Window is null.");
            return;
        }
        
        if (window.GetRect() == targetRect) return;
        
        FlushUnpassableWindows();
        if (_unpassableWindows.Count == 0)
        {
            window.SetPosition(targetRect.Position);
            window.SetSize(targetRect.Size);
            return;
        }

        foreach (var unpassableWindow in _unpassableWindows)
        {
            if (unpassableWindow == window) continue;
            
            var unpassableRect = unpassableWindow.GetRect();
            if (!targetRect.Intersects(unpassableRect)) continue;

            if (activeEdge.HasValue)
            {
                // Handle resizing
                switch (activeEdge.Value)
                {
                    case DisplayServer.WindowResizeEdge.Top:
                        var newY = unpassableRect.End.Y;
                        targetRect.Size -= new Vector2I(0, newY - targetRect.Position.Y);
                        targetRect.Position = new Vector2I(targetRect.Position.X, newY);
                        break;
                    case DisplayServer.WindowResizeEdge.Left: 
                        var newX = unpassableRect.End.X;
                        targetRect.Size -= new Vector2I(newX - targetRect.Position.X, 0);
                        targetRect.Position = new Vector2I(newX, targetRect.Position.Y);
                        break;
                    case DisplayServer.WindowResizeEdge.Right:
                        targetRect.Size = new Vector2I(unpassableRect.Position.X - targetRect.Position.X, targetRect.Size.Y);
                        break;
                    case DisplayServer.WindowResizeEdge.Bottom:
                        targetRect.Size = new Vector2I(targetRect.Size.X, unpassableRect.Position.Y - targetRect.Position.Y);
                        break;
                    case DisplayServer.WindowResizeEdge.TopLeft:
                    case DisplayServer.WindowResizeEdge.TopRight:
                    case DisplayServer.WindowResizeEdge.BottomLeft:
                    case DisplayServer.WindowResizeEdge.BottomRight:
                    case DisplayServer.WindowResizeEdge.Max:
                    default:
                        GD.PrintErr($"[GameWindowManager]: Unsupported resize edge: {activeEdge.Value}");
                        break;
                }
            }
            else
            {
                // MTV calculation
                var intersection = targetRect.Intersection(unpassableRect);
                var offset = Vector2I.Zero;
                var targetCenter = targetRect.GetCenter();
                var unpassableCenter = unpassableRect.GetCenter();
                if (intersection.Size.X < intersection.Size.Y)
                {
                    offset.X = targetCenter.X < unpassableCenter.X 
                        ? -intersection.Size.X 
                        : intersection.Size.X;
                }
                else
                {
                    offset.Y = targetCenter.Y < unpassableCenter.Y 
                        ? -intersection.Size.Y 
                        : intersection.Size.Y;
                }
                targetRect.Position += offset;
            }
        }
        
        window.SetPosition(targetRect.Position);
        window.SetSize(targetRect.Size);
    }
    
    private void FlushUnpassableWindows()
    {
        if (!_isUnpassableWindowsDirty) return;
        
        _unpassableWindows = _managedWindows
            .Where(w => !w.HasBehavior(BehaviorType.Passable))
            .ToList();
        
        _isUnpassableWindowsDirty = false;
        GD.Print($"[GameWindowManager]: Unpassable windows flushed. Count: {_unpassableWindows.Count}");
    }
    
    private void OnWindowFocused(BaseWindow window)
    {
        if (!_managedWindows.Contains(window)) return;

        _managedWindows.Remove(window);
        _managedWindows.Add(window);
        GD.Print($"[GameWindowManager]: Window {window.GetWindowId()} get focused.");
        PrintWindowsOrder();
    }
    
    private void OnWindowCloseRequested(BaseWindow window)
    {
        _managedWindows.Remove(window);
        window.QueueFree();
        _isUnpassableWindowsDirty = true;
        GD.Print($"[GameWindowManager]: Window {window.GetWindowId()} closed. Remaining: {_managedWindows.Count}");
        PrintWindowsOrder();
    }
    
    private void PrintWindowsOrder()
    {
        var order = string.Join(" <- ", _managedWindows.Select(w => w.GetWindowId()));
        GD.Print($"[GameWindowManager]: Windows order: {order}");
    }
}