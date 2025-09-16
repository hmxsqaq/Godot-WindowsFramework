using Godot;
using Godot.Collections;
using windows_framework.scripts.game_window;

namespace windows_framework.scripts.game;

[GlobalClass]
public partial class LevelConfig : Resource
{
	[Export] public int LevelNumber { get; set; } = 1;
	[Export] public Vector2I PlayerSize { get; set; } = new(50, 50);
	[Export] public Rect2I GoalRect { get; set; }
	[Export] public Array<WindowConfig> WindowConfigs { get; set; }
}
