using System.Collections.Generic;
using Godot;
using windows_framework.scripts.game_window.behaviors;

namespace windows_framework.scripts.game_window;

public partial class BaseWindow : Window
{
    [Export] private Panel _backgroundPanel;
    [Export] private Color _focusedColor = new(0.2f, 0.2f, 0.2f);
    [Export] private Color _unfocusedColor = new(0.3f, 0.3f, 0.3f);

    private Dictionary<BehaviorType, Behavior> Behaviors { get; } = new();
    
    public override void _Ready()
    {
        if (_backgroundPanel == null)
        {
            GD.PrintErr($"[BaseWindow: {Name}] Background panel is not assigned in the inspector.");
        }
        else
        {
            _backgroundPanel.FocusEntered += () => _backgroundPanel.Modulate = _focusedColor;
            _backgroundPanel.FocusExited += () => _backgroundPanel.Modulate = _unfocusedColor;
        }
    }
    
    public Rect2I GetRect() => new(Position, Size);
    
    public void MoveTo(Vector2I position) => WindowManager.Instance.MoveWindow(this, position);

    public void ResizeTo(Vector2I size)
    {
        SetSize(size);
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