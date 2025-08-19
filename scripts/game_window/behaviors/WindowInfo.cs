using Godot;

namespace windows_framework.scripts.game_window.behaviors;

public partial class WindowInfo : Behavior
{
    [Export] private Label _idLabel;
    [Export] private Label _posLabel;
    [Export] private Label _sizeLabel;
    
    public override void _Process(double delta)
    {
        _idLabel.Text = $"ID: {GameWindow.GetWindowId()}";
        _posLabel.Text = $"Position: {GameWindow.GetPosition()}";
        _sizeLabel.Text = $"Size: {GameWindow.GetSize()}";
    }

    protected override bool OnInitialize(BaseWindow window) => true;
}