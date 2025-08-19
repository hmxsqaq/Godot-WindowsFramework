using Godot;

namespace windows_framework.scripts.game_window.behaviors;

public enum BehaviorType
{
    Movable,
    Resizable,
    Passable,
    WindowInfo
}

public abstract partial class Behavior : Node
{
    protected BaseWindow GameWindow;
    protected bool IsInitialized = false;

    public bool Initialize(BaseWindow window)
    {
        if (IsInitialized)
        {
            GD.PrintErr($"[Behavior: {Name}] Initialize called multiple times. This behavior is already initialized.");
            return false;
        }

        if (window == null)
        {
            GD.PrintErr($"[Behavior: {Name}] Initialize called with null window.");
            return false;
        }

        GameWindow = window;
        IsInitialized = true;
        return OnInitialize(window);
    }
    
    protected abstract bool OnInitialize(BaseWindow window);
}