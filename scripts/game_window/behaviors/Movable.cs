using Godot;

namespace windows_framework.scripts.game_window.behaviors;

public partial class Movable : Behavior
{
    [Export] private Button _moveButton;
    [Export] private float _moveSpeed = 10;
    
    private bool _isDragging = false;
    
    private Vector2I _offset = Vector2I.Zero;
    
    protected override bool OnInitialize(BaseWindow window)
    {
        if (_moveButton == null)
        {
            GD.PrintErr($"[Movable: {Name}] Move button is not assigned in the inspector.");
            return false;
        }
        
        _moveButton.MouseDefaultCursorShape = Control.CursorShape.Move;
        _moveButton.ButtonDown += () =>
        {
            _isDragging = true;
            _offset = DisplayServer.MouseGetPosition() - GameWindow.GetPosition();
        };
        
        _moveButton.ButtonUp += () => _isDragging = false;
        return true;
    }

    public override void _Process(double delta)
    {
        if (!IsInitialized)
        {
            GD.PrintErr($"[Movable: {Name}] Movable is not initialized.");
            return;
        }

        if (!_isDragging) return;
        var targetPosition = DisplayServer.MouseGetPosition() - _offset;
        targetPosition = (Vector2I)(GameWindow.Position + (Vector2)(targetPosition - GameWindow.Position) * _moveSpeed * (float)delta);
        GameWindow.MoveTo(targetPosition);
    }
}