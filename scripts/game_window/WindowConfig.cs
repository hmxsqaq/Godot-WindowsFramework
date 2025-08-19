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

    [ExportGroup("Behavior")]
    [Export] public Dictionary<BehaviorType, bool> Behaviors { get; set; } = new()
    {
        { BehaviorType.Movable, true },
        { BehaviorType.Resizable, true },
        { BehaviorType.WindowInfo, false },
        { BehaviorType.Passable, false }
    };
}