using Godot;

namespace windows_framework.scripts.game_window;

[GlobalClass]
public partial class WindowConfig : Resource
{
    [Export] public string Title { get; set; } = "New Window";
    [Export] public Vector2 MinSize { get; set; } = new Vector2(100, 100);
    
    
    [ExportGroup("Behavior")]
    [Export] public bool Movable { get; set; } = true;
    [Export] public bool Resizable { get; set; } = true;
    [Export] public bool Closeable { get; set; } = true;
    [Export] public bool Focusable { get; set; } = true;
}