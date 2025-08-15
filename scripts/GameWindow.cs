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
    
    [Export] private Control _backgroundPanel;
    
    private StyleBoxFlat _backgroundPanelStyleBoxOverride;

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
            button.ActionMode = BaseButton.ActionModeEnum.Press;
            button.Pressed += () => StartGameWindowResize(edge);
        }

        _moveButton.MouseDefaultCursorShape = Control.CursorShape.Move;
        _moveButton.ActionMode = BaseButton.ActionModeEnum.Press;
        _moveButton.Pressed += StartGameWindowDrag;

        FocusEntered += () => SetBackgroundColor(Colors.Azure);
        FocusExited += () => SetBackgroundColor(Colors.Gray);
    }

    public override void _Process(double delta)
    {
        _idLabel.Text = $"ID: {GetWindowId()}";
        _posLabel.Text = $"Position: {GetPosition()}";
        _sizeLabel.Text = $"Size: {GetSize()}";
    }
    
    public void SetBackgroundColor(Color color)
    {
        if (_backgroundPanelStyleBoxOverride == null && !CreateBackgroundStyleBoxOverride()) 
        {
            GD.PrintErr($"[GameWindow: {Name}] Failed to create StyleBox Override for background panel.");
            return;
        }
        _backgroundPanelStyleBoxOverride!.BgColor = color;
    }
    
    private bool CreateBackgroundStyleBoxOverride()
    {
        var originalStyleBox = _backgroundPanel.GetThemeStylebox("panel");
        if (originalStyleBox == null)
        {
            GD.PrintErr($"[GameWindow: {Name}] Background panel does not have a 'panel' stylebox in its theme.");
            return false;
        }
        _backgroundPanelStyleBoxOverride = originalStyleBox.Duplicate() as StyleBoxFlat;
        if (_backgroundPanelStyleBoxOverride == null)
        {
            GD.PrintErr($"[GameWindow: {Name}] Failed to duplicate 'panel' stylebox as StyleBoxFlat.");
            return false;
        }
        _backgroundPanel.AddThemeStyleboxOverride("panel", _backgroundPanelStyleBoxOverride);
        return true;
    }

    private void StartGameWindowResize(DisplayServer.WindowResizeEdge edge) => StartResize(edge);

    private void StartGameWindowDrag() => StartDrag();
}