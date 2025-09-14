using Godot;
using windows_framework.scripts.player;

namespace windows_framework.scripts.game_window.behaviors;

public partial class Walkable : Behavior
{
    [Export] private Control _player;

    protected override bool OnInitialize(BaseWindow window)
    {
        if (_player == null)
        {
            GD.PrintErr($"[Walkable: {Name}] Player Control is not assigned in the inspector.");
            return false;
        }
        _player.Visible = false;
        return true;
    }

    public override void _Process(double delta)
    {
        var playerGlobalRect = PlayerManager.Instance.PlayerRect;
        var windowRect = GameWindow.GetRect();
        var isPlayerInside = windowRect.Intersects((Rect2I)playerGlobalRect);
        if (isPlayerInside)
        {
            _player.Position = playerGlobalRect.Position - windowRect.Position;
            _player.Size = playerGlobalRect.Size;
        }
        _player.Visible = isPlayerInside;
    }
}
