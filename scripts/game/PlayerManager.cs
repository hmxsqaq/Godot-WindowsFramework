using System.Collections.Generic;
using System.Linq;
using Godot;
using windows_framework.scripts.game_window;
using windows_framework.scripts.utility;

namespace windows_framework.scripts.game;

public partial class PlayerManager : Node
{
	#region Singleton

	public static PlayerManager Instance { get; private set; }

	public override void _EnterTree()
	{
		if (Instance != null)
		{
			GD.PrintErr("[PlayerManager]: Instance already exists. Destroying the new instance.");
			QueueFree();
			return;
		}
		Instance = this;
	}

	#endregion

	public BaseWindow ParentWindow { get; private set; }
	public Rect2 PlayerRect => new(_playerPosition, _playerSize);

	private Vector2 _playerPosition;
	private Vector2 _playerSize;
	private int _playerSpeed = 200;

	public override void _Process(double delta)
	{
		Vector2 inputDirection = Vector2.Zero;
		if (Input.IsActionPressed("player_right")) inputDirection.X = 1;
		if (Input.IsActionPressed("player_left")) inputDirection.X = -1;
		if (Input.IsActionPressed("player_down")) inputDirection.Y = 1;
		if (Input.IsActionPressed("player_up")) inputDirection.Y = -1;
		if (inputDirection != Vector2.Zero)
		{
			inputDirection = inputDirection.Normalized();
			var movement = inputDirection * _playerSpeed * (float)delta;
			var newPosition = _playerPosition + movement;
			MoveTo(newPosition);
		}
	}

	public void ResetPlayer(BaseWindow parent, Vector2 newSize)
	{
		_playerPosition = parent.GetRect().GetCenter() - newSize / 2;
		_playerSize = newSize;
		SetParent(parent);
	}

	public void SetParent(BaseWindow newParent)
	{
		if (newParent == ParentWindow) return;
		if (ParentWindow != null)
		{
			ParentWindow.WindowMoved -= OnParentWindowMoved;
			ParentWindow.WindowResized -= OnParentWindowResized;
		}
		ParentWindow = newParent;
		if (newParent == null) return;
		ParentWindow.WindowMoved += OnParentWindowMoved;
		ParentWindow.WindowResized += OnParentWindowResized;
	}

	private void OnParentWindowMoved(Rect2I windowRectBeforeMoved)
	{
		if (!ParentWindow.IsPlayerFollowingMovement) return;
		var parentPosDelta = ParentWindow.GetRect().Position - windowRectBeforeMoved.Position;
		MoveTo(_playerPosition + parentPosDelta);
	}

	private void OnParentWindowResized(Rect2I windowRectBeforeResized)
	{
		if (!ParentWindow.IsPlayerFollowingResizing) return;

		var parentOriginalSize = windowRectBeforeResized.Size;
		var parentNewSize = ParentWindow.GetRect().Size;
		var parentSizeScale = new Vector2(
			(float)parentNewSize.X / parentOriginalSize.X,
			(float)parentNewSize.Y / parentOriginalSize.Y);

		var originalParentCenter = windowRectBeforeResized.GetCenter();
		var newParentCenter = ParentWindow.GetRect().GetCenter();
		var toPlayer = PlayerRect.GetCenter() - originalParentCenter;
		toPlayer = new Vector2(toPlayer.X * parentSizeScale.X, toPlayer.Y * parentSizeScale.Y);
		_playerSize = new Vector2(_playerSize.X * parentSizeScale.X, _playerSize.Y * parentSizeScale.Y);
		MoveTo(newParentCenter + toPlayer - _playerSize / 2);
	}

	private void MoveTo(Vector2 newPosition)
	{
		var unwalkableRects = GetUnwalkableRects();
		foreach (var unwalkableRect in unwalkableRects
			         .Where(unwalkableRect => PlayerRect.Intersects(unwalkableRect)))
		{
			// collided
			var intersection = PlayerRect.Intersection(unwalkableRect);
			var offset = Vector2.Zero;
			var playerCenter = PlayerRect.GetCenter();
			var unwalkableCenter = unwalkableRect.GetCenter();
			if (intersection.Size.X < intersection.Size.Y)
			{
				offset.X = playerCenter.X < unwalkableCenter.X
					? -intersection.Size.X
					: intersection.Size.X;
			}
			else
			{
				offset.Y = playerCenter.Y < unwalkableCenter.Y
					? -intersection.Size.Y
					: intersection.Size.Y;
			}
			newPosition += offset;
		}
		_playerPosition = newPosition;
	}

	private List<Rect2I> GetUnwalkableRects()
	{
		List<Rect2I> walkableRects = [];
		foreach (var window in WindowManager.Instance.ManagedWindows)
		{
			if (window.HasBehavior(BehaviorType.Walkable))
			{
				walkableRects.Add(window.GetRect());
				continue;
			}
			// unwalkable
			foreach (var walkableRect in walkableRects.ToList())
			{
				var remainingRects = walkableRect.Subtract(window.GetRect());
				walkableRects.Remove(walkableRect);
				walkableRects.AddRange(remainingRects);
			}
		}

		var screenRect = new Rect2I(DisplayServer.ScreenGetPosition(), DisplayServer.ScreenGetSize());
		return screenRect.Subtract(walkableRects);
	}
}
