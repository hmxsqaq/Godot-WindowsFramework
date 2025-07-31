using System;
using Godot;
using Godot.Collections;

namespace windows_framework.scripts;

public partial class GameWindow : Window
{
    [Export] private Label _idLabel;
    [Export] private Label _posLabel;
    [Export] private Label _sizeLabel;
    
    [Export] private Dictionary<DisplayServer.WindowResizeEdge, Button> _resizeButtons = new();
    [Export] private Button _moveButton;

    private bool _isOperating = false;
    
    public override void _Ready()
    {
        CloseRequested += QueueFree;

        foreach (var (edge, button) in _resizeButtons)
        {
            if (button == null)
            {
                GD.PrintErr($"[GameWindow: {Name}] Resize button for edge {edge} is not assigned in the inspector.");
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

            button.ButtonDown += () => GameWindowStartResize(edge);
        }

        _moveButton.MouseDefaultCursorShape = Control.CursorShape.Move;
        _moveButton.ButtonDown += GameWindowStartDrag;
    }

    public override void _Process(double delta)
    {
        _idLabel.Text = $"ID: {GetWindowId()}";
        _posLabel.Text = $"Position: {GetPosition()}";
        _sizeLabel.Text = $"Size: {GetSize()}";
    }
    
    private void GameWindowStartResize(DisplayServer.WindowResizeEdge edge) => StartResize(edge);

    private void GameWindowStartDrag() => StartDrag();

    private void StopOperating() => _isOperating = false;
}