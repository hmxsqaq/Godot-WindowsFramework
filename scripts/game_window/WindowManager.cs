using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Godot;
using windows_framework.scripts.game_window.behaviors;

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

    #region Recourses
    
    private readonly PackedScene _baseWindowScene = GD.Load<PackedScene>("res://scenes/game_window.tscn");

    private readonly Dictionary<BehaviorType, PackedScene> _behaviorScenes = new()
    {
        { BehaviorType.Movable, GD.Load<PackedScene>("res://scenes/behaviors/movable.tscn") },
        { BehaviorType.Resizable, GD.Load<PackedScene>("res://scenes/behaviors/resizable.tscn") },
        { BehaviorType.WindowInfo, GD.Load<PackedScene>("res://scenes/behaviors/window_info.tscn") },
        { BehaviorType.Passable, GD.Load<PackedScene>("res://scenes/behaviors/passable.tscn") },
        { BehaviorType.UnBlockable, GD.Load<PackedScene>("res://scenes/behaviors/unblockable.tscn") }
    };
    
    #endregion
    
    private readonly List<BaseWindow> _managedWindows = [];

    private bool _isWindowListDirty = true;

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
        _isWindowListDirty = true;
        
        return newWindow;
    }

    public void SetWindowRect(BaseWindow targetWindow, Rect2I targetRect, DisplayServer.WindowResizeEdge? activeEdge = null)
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
        foreach (var window in _managedWindows.Where(window => window != targetWindow))
        {
            if (window.HasBehavior(BehaviorType.UnBlockable))
            {
                GD.Print("[GameWindowManager]: Found unpassable window, subtracting its rects.");
                foreach (var unpassableRect in unpassableRects.ToList())
                {
                    GD.Print($"[GameWindowManager]: Subtracting rect: - Pos: {unpassableRect.Position}, Size: {unpassableRect.Size}");
                    var subtractedRects = SubtractRect(unpassableRect, window.GetRect());
                    unpassableRects.Remove(unpassableRect);
                    unpassableRects.AddRange(subtractedRects);
                }
                continue;
            }
            if (window.HasBehavior(BehaviorType.Passable)) continue;
            unpassableRects.Add(window.GetRect());
        }

        var debugMsg = unpassableRects.Aggregate("[GameWindowManager]: Unpassable rects:\n", (current, rect) => current + $"- Pos: {rect.Position}, Size: {rect.Size}\n");
        GD.Print(debugMsg);
        
        foreach (var unpassableRect in unpassableRects.Where(unpassableRect => targetRect.Intersects(unpassableRect)))
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

    private List<Rect2I> SubtractRect(Rect2I original, Rect2I subtract)
    {
        List<Rect2I> result = [];
        if (!original.Intersects(subtract))
        {
            result.Add(original);
            return result;
        }
        var intersection = original.Intersection(subtract);
        // top
        if (original.Position.Y < intersection.Position.Y)
        {
            Rect2I topRect = new(
                original.Position,
                new Vector2I(original.Size.X, intersection.Position.Y - original.Position.Y)
            );
            result.Add(topRect);
        }
        // bottom
        if (original.End.Y > intersection.End.Y)
        {
            Rect2I bottomRect = new(
                new Vector2I(original.Position.X, intersection.End.Y),
                new Vector2I(original.Size.X, original.End.Y - intersection.End.Y)
            );
            result.Add(bottomRect);
        }
        // left
        if (original.Position.X < intersection.Position.X)
        {
            Rect2I leftRect = new(
                new Vector2I(original.Position.X, intersection.Position.Y),
                new Vector2I(intersection.Position.X - original.Position.X, intersection.Size.Y)
            );
            result.Add(leftRect);
        }
        // right
        if (original.End.X > intersection.End.X)
        {
            Rect2I rightRect = new(
                new Vector2I(intersection.End.X, intersection.Position.Y),
                new Vector2I(original.End.X - intersection.End.X, intersection.Size.Y)
            );
            result.Add(rightRect);
        }
        return result;
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
        _isWindowListDirty = true;
        GD.Print($"[GameWindowManager]: Window {window.GetWindowId()} closed. Remaining: {_managedWindows.Count}");
        PrintWindowsOrder();
    }
    
    private void PrintWindowsOrder()
    {
        var order = string.Join(" <- ", _managedWindows.Select(w => w.GetWindowId()));
        GD.Print($"[GameWindowManager]: Windows order: {order}");
    }
}