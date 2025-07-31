using Godot;

namespace windows_framework.scripts;

public partial class GameWindow : Window
{
    [Export] private Label _idLabel;
    [Export] private Label _posLabel;
    [Export] private Label _sizeLabel;
    
    [Export] private Button _topButton;
    [Export] private Button _bottomButton;
    [Export] private Button _leftButton;
    [Export] private Button _rightButton;
    [Export] private Button _moveButton;

    public override void _Ready()
    {
        CloseRequested += QueueFree;

        if (_topButton == null || _bottomButton == null || _leftButton == null || _rightButton == null)
        {
            GD.PrintErr($"[GameWindow: {Name}] Resize buttons are not assigned in the inspector.");
            return;
        }
        
        _topButton.ButtonDown += () => StartResize(DisplayServer.WindowResizeEdge.Top);
        _bottomButton.ButtonDown += () => StartResize(DisplayServer.WindowResizeEdge.Bottom);
        _leftButton.ButtonDown += () => StartResize(DisplayServer.WindowResizeEdge.Left);
        _rightButton.ButtonDown += () => StartResize(DisplayServer.WindowResizeEdge.Right);
        _moveButton.ButtonDown += StartDrag;
    }

    public override void _Process(double delta)
    {
        _idLabel.Text = $"ID: {GetWindowId()}";
        _posLabel.Text = $"Position: {GetPosition()}";
        _sizeLabel.Text = $"Size: {GetSize()}";
    }
}