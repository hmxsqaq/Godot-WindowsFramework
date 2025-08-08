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

    public override void _Ready()
    {
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

            button.ButtonDown += () => StartGameWindowResize(edge);
        }

        _moveButton.MouseDefaultCursorShape = Control.CursorShape.Move;
        _moveButton.ButtonDown += StartGameWindowDrag;
    }

    public override void _Process(double delta)
    {
        _idLabel.Text = $"ID: {GetWindowId()}";
        _posLabel.Text = $"Position: {GetPosition()}";
        _sizeLabel.Text = $"Size: {GetSize()}";
    }

    private void StartGameWindowResize(DisplayServer.WindowResizeEdge edge) => StartResize(edge);

    private void StartGameWindowDrag() => StartDrag();
}