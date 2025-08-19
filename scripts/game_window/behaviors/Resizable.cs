using Godot;
using Godot.Collections;

namespace windows_framework.scripts.game_window.behaviors;

public partial class Resizable : Behavior
{
    [Export] private Dictionary<DisplayServer.WindowResizeEdge, Button> _resizeButtons = new();

    protected override bool OnInitialize(BaseWindow window)
    {
        foreach (var (edge, button) in _resizeButtons)
        {
            if (button == null)
            {
                GD.PrintErr($"[Movable: {Name}] Resize button for edge {edge} is not assigned in the inspector.");
                continue;
            }
            
            button.MouseDefaultCursorShape = edge switch
            {
                DisplayServer.WindowResizeEdge.Top => Control.CursorShape.Vsize,
                DisplayServer.WindowResizeEdge.Left => Control.CursorShape.Hsize,
                DisplayServer.WindowResizeEdge.Right => Control.CursorShape.Hsize,
                DisplayServer.WindowResizeEdge.Bottom => Control.CursorShape.Vsize,
                _ => Control.CursorShape.Arrow
            };
            button.ActionMode = BaseButton.ActionModeEnum.Press;
            button.Pressed += () => window.StartResize(edge);
        }
        return true;
    }
}