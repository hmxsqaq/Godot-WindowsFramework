using Godot;
using Godot.Collections;
using windows_framework.scripts.game_window.behaviors;

namespace windows_framework.scripts.game_window;

[GlobalClass]
public partial class WindowConfig : Resource
{
    [Export] public string Title { get; set; } = "New Window";
    [Export] public Vector2I Position { get; set; } = new(100, 100);
    [Export] public Vector2I MinSize { get; set; } = new(100, 100);
    [Export] public Vector2I Size { get; set;} = new(500, 500);

    [Export] public bool IsStartWindow { get; set; } = false;
    [Export] public bool PlayerCanFollowMovement { get; set; } = false; // If true, player will follow the window's movement when the window is the parent
    [Export] public bool PlayerCanFollowResizing { get; set; } = false; // If true, player will follow the window's resizing when the window is the parent

    [ExportGroup("Behavior")]
    [Export] public Dictionary<BehaviorType, bool> Behaviors { get; set; } = new()
    {
        { BehaviorType.Movable, true },
        { BehaviorType.Resizable, true },
        { BehaviorType.Passable, true },
        { BehaviorType.UnBlockable, false},
        { BehaviorType.Walkable, true},
        { BehaviorType.WindowInfo, false }
    };
}
