using System.Collections.Generic;
using System.Linq;
using Godot;
using windows_framework.scripts.game_window.behaviors;
using windows_framework.scripts.utility;

namespace windows_framework.scripts.game_window;

public partial class WindowManager : Node
{
    #region Singleton

    public static WindowManager Instance { get; private set; }

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

    #endregion

    #region Resources
    
    private readonly PackedScene _baseWindowScene = GD.Load<PackedScene>("res://scenes/game_window.tscn");

    private readonly Dictionary<BehaviorType, PackedScene> _behaviorScenes = new()
    {
        { BehaviorType.Movable, GD.Load<PackedScene>("res://scenes/behaviors/movable.tscn") },
        { BehaviorType.Resizable, GD.Load<PackedScene>("res://scenes/behaviors/resizable.tscn") },
        { BehaviorType.WindowInfo, GD.Load<PackedScene>("res://scenes/behaviors/window_info.tscn") },
        { BehaviorType.Passable, GD.Load<PackedScene>("res://scenes/behaviors/passable.tscn") },
        { BehaviorType.UnBlockable, GD.Load<PackedScene>("res://scenes/behaviors/unblockable.tscn") },
        { BehaviorType.Walkable, GD.Load<PackedScene>("res://scenes/behaviors/walkable.tscn") },
    };
    
    #endregion

    public List<BaseWindow> ManagedWindows { get; } = [];

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

        newWindow.Config = windowConfig;
        newWindow.SetTitle(windowConfig.Title);
        newWindow.SetMinSize(windowConfig.MinSize);
        newWindow.IsPlayerFollowingMovement = windowConfig.PlayerCanFollowMovement;
        newWindow.IsPlayerFollowingResizing = windowConfig.PlayerCanFollowResizing;

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
        
        GetTree().Root.AddChild(newWindow);
        Vector2I screenPosition = DisplayServer.ScreenGetPosition();
        newWindow.Popup(new Rect2I(windowConfig.Position + screenPosition, windowConfig.Size));
        ManagedWindows.Add(newWindow);
        
        return newWindow;
    }

    public void SetWindowRect(BaseWindow targetWindow, Rect2I targetRect,
        DisplayServer.WindowResizeEdge? activeEdge = null)
    {
        if (targetWindow == null)
        {
            GD.PrintErr("[GameWindowManager]: Window is null.");
            return;
        }
        
        if (targetWindow.GetRect() == targetRect) return;

        if (targetWindow.HasBehavior(BehaviorType.UnBlockable))
        {
            targetWindow.SetPosition(targetRect.Position);
            targetWindow.SetSize(targetRect.Size);
            return;
        }

        List<Rect2I> unpassableRects = [];
        var isTargetWindowPassable = targetWindow.HasBehavior(BehaviorType.Passable);
        foreach (var window in ManagedWindows.Where(window => window != targetWindow))
        {
            if (window.HasBehavior(BehaviorType.UnBlockable))
            {
                foreach (var unpassableRect in unpassableRects.ToList())
                {
                    var remainingRects = unpassableRect.Subtract(window.GetRect());
                    unpassableRects.Remove(unpassableRect);
                    unpassableRects.AddRange(remainingRects);
                }
                continue;
            }
            if (isTargetWindowPassable && window.HasBehavior(BehaviorType.Passable)) continue;
            unpassableRects.Add(window.GetRect());
        }

        // GD.Print(unpassableRects.Aggregate("[GameWindowManager]: Unpassable rects:\n", (current, rect) => current + $"- Pos: {rect.Position}, Size: {rect.Size}\n"));
        
        foreach (var unpassableRect in unpassableRects
                     .Where(unpassableRect => targetRect.Intersects(unpassableRect)))
        {
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

        targetWindow.SetPosition(targetRect.Position);
        targetWindow.SetSize(targetRect.Size);
    }
    
    private void OnWindowFocused(BaseWindow window)
    {
        if (!ManagedWindows.Contains(window)) return;

        ManagedWindows.Remove(window);
        ManagedWindows.Add(window);
        GD.Print($"[GameWindowManager]: Window {window.GetWindowId()} get focused.");
        PrintWindowsOrder();
    }
    
    private void OnWindowCloseRequested(BaseWindow window)
    {
        ManagedWindows.Remove(window);
        window.QueueFree();
        GD.Print($"[GameWindowManager]: Window {window.GetWindowId()} closed. Remaining: {ManagedWindows.Count}");
        PrintWindowsOrder();
    }
    
    private void PrintWindowsOrder()
    {
        var order = string.Join(" <- ", ManagedWindows.Select(w => w.GetWindowId()));
        GD.Print($"[GameWindowManager]: Windows order: {order}");
    }
}
