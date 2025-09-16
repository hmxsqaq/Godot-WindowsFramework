using System.Collections.Generic;
using Godot;
using windows_framework.scripts.game_window.behaviors;
using windows_framework.scripts.ui;

namespace windows_framework.scripts.game_window;

public partial class BaseWindow : Window
{
    [Export] private BgStyleSetter _bgStyle;

    [Signal] public delegate void WindowResizedEventHandler(Rect2I windowRectBeforeResized);
    [Signal] public delegate void WindowMovedEventHandler(Rect2I windowRectBeforeMoved);

    public WindowConfig Config { get; set; } // the config used to create this window

    private Dictionary<BehaviorType, Behavior> Behaviors { get; } = new();
    
    public override void _Ready()
    {
        if (_bgStyle == null)
            GD.PrintErr($"[BaseWindow: {Name}] BgStyle is not assigned in the inspector.");
        else
        {
            FocusEntered += () => _bgStyle.SetBorderColor(new Color(1f, 1f, 1f));
            FocusExited += () => _bgStyle.SetBorderColor(new Color(0.3f, 0.3f, 0.3f));
        }
    }
    
    public Rect2I GetRect() => new(Position, Size);

    public void MoveTo(Vector2I position)
    {
        Rect2I originalRect = GetRect();
        WindowManager.Instance.SetWindowRect(this, new Rect2I(position, Size));
        EmitSignalWindowMoved(originalRect);
    }

    public void ResizeTo(Rect2I rect, DisplayServer.WindowResizeEdge activeEdge)
    {
        Rect2I originalRect = GetRect();
        WindowManager.Instance.SetWindowRect(this, rect, activeEdge);
        EmitSignalWindowResized(originalRect);
    }

    public void AddBehavior(BehaviorType type, Behavior behavior)
    {
        if (Behaviors.ContainsKey(type))
        {
            GD.PrintErr($"[BaseWindow: {Name}] Behavior {type} already exists.");
            return;
        }

        if (!behavior.Initialize(this))
        {
            GD.PrintErr($"[BaseWindow: {Name}] Failed to initialize behavior {type}.");
            return;
        }
        
        Behaviors[type] = behavior;
        AddChild(behavior);
        GD.Print($"[BaseWindow: {Name}] Successfully added behavior {type}.");
    }
    
    public void RemoveBehavior(BehaviorType type) 
    {
        if (!Behaviors.Remove(type, out var behavior))
        {
            GD.PrintErr($"[BaseWindow: {Name}] Behavior {type} not found.");
            return;
        }

        behavior.QueueFree();
        GD.Print($"[BaseWindow: {Name}] Successfully removed behavior {type}.");
    }
    
    public Behavior GetBehavior(BehaviorType type) => Behaviors.GetValueOrDefault(type);
    
    public bool HasBehavior(BehaviorType type) => Behaviors.ContainsKey(type);
}
