using System;
using Godot;
using Godot.Collections;

namespace windows_framework.scripts.game_window.behaviors;

public partial class Resizable : Behavior
{
    [Export] private Dictionary<DisplayServer.WindowResizeEdge, Button> _resizeButtons = new();
    
    private bool _isResizing = false;
    private DisplayServer.WindowResizeEdge _currentEdge;
    private Vector2I _mousePosition;

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

            button.ButtonDown += () =>
            {
                _isResizing = true;
                _currentEdge = edge;
                _mousePosition = DisplayServer.MouseGetPosition();
            };
            
            button.ButtonUp += () => _isResizing = false;
        }
        return true;
    }
    
    public override void _Process(double delta)
    {
        if (!IsInitialized)
        {
            GD.PrintErr($"[Resizable: {Name}] Resizable is not initialized.");
            return;
        }

        if (!_isResizing) return;
        var mouseDelta = DisplayServer.MouseGetPosition() - _mousePosition;
        var newSize = GameWindow.Size;
        var newPosition = GameWindow.Position;
        switch (_currentEdge)
        {
            case DisplayServer.WindowResizeEdge.Top:
                newSize.Y -= mouseDelta.Y;
                newPosition.Y += mouseDelta.Y;
                break;
            case DisplayServer.WindowResizeEdge.Left:
                newSize.X -= mouseDelta.X;
                newPosition.X += mouseDelta.X;
                break;
            case DisplayServer.WindowResizeEdge.Right:
                newSize.X += mouseDelta.X;
                break;
            case DisplayServer.WindowResizeEdge.Bottom:
                newSize.Y += mouseDelta.Y;
                break;
            case DisplayServer.WindowResizeEdge.TopLeft:
            case DisplayServer.WindowResizeEdge.TopRight:
            case DisplayServer.WindowResizeEdge.BottomLeft:
            case DisplayServer.WindowResizeEdge.BottomRight:
            case DisplayServer.WindowResizeEdge.Max:
            default:
                GD.PrintErr($"[Resizable: {Name}] Unknown resize edge: {_currentEdge}");
                return;
        }
        GameWindow.ResizeTo(new Rect2I(newPosition, newSize), _currentEdge);
        _mousePosition = DisplayServer.MouseGetPosition();
    }
}