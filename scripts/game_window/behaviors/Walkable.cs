using Godot;
using windows_framework.scripts.game;

namespace windows_framework.scripts.game_window.behaviors;

public partial class Walkable : Behavior
{
    [Export] private Control _player;
    [Export] private Control _goal;

    protected override bool OnInitialize(BaseWindow window)
    {
        if (_player == null)
        {
            GD.PrintErr($"[Walkable: {Name}] Player Control is not assigned in the inspector.");
            return false;
        }

        _player.Visible = false;

        GameWindow.FocusEntered += () =>
        {
            if (GameWindow.GetRect().Intersects((Rect2I)PlayerManager.Instance.PlayerRect))
                PlayerManager.Instance.SetParent(GameWindow);
        };

        return true;
    }

    public override void _Process(double delta)
    {
        HandlePlayer();
    }

    private void HandlePlayer()
    {
        if (_player == null)
        {
            GD.PrintErr($"[Walkable: {Name}] Player Control is not assigned in the inspector.");
            return;
        }

        // player renderer
        var playerGlobalRect = PlayerManager.Instance.PlayerRect;
        var windowRect = GameWindow.GetRect();
        var isPlayerInside = windowRect.Intersects((Rect2I)playerGlobalRect);
        if (isPlayerInside)
        {
            _player.Position = playerGlobalRect.Position - windowRect.Position;
            _player.Size = playerGlobalRect.Size;
        }
        _player.Visible = isPlayerInside;

        // parent check
        if (!GameWindow.HasFocus()) return;
        if (!isPlayerInside && PlayerManager.Instance.ParentWindow == GameWindow)
            PlayerManager.Instance.SetParent(null);
        if (Input.IsActionJustReleased("mouse_left_button") && isPlayerInside &&
            PlayerManager.Instance.ParentWindow != GameWindow)
            PlayerManager.Instance.SetParent(GameWindow);
    }

    private void HandleGoal()
    {
        if (_goal == null)
        {
            GD.PrintErr($"[Walkable: {Name}] Goal Control is not assigned in the inspector.");
            return;
        }

        var goalGlobalRect = GameManager.Instance.GoalRect;
        var windowRect = GameWindow.GetRect();
        var isGoalInside = windowRect.Intersects(goalGlobalRect);
        if (!isGoalInside) return;
        _goal.Position = goalGlobalRect.Position - windowRect.Position;
        _goal.Visible = true;
    }
}
