using System.Collections.Generic;
using System.Linq;
using Godot;
using windows_framework.scripts.game_window;
using windows_framework.scripts.game_window.behaviors;
using windows_framework.scripts.utility;

namespace windows_framework.scripts.player;

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

	public Rect2 CurrentPlayerRect => new(_playerPosition, _playerSize);

	private BaseWindow _parentWindow;
	private Vector2 _playerPosition;
	private Vector2 _playerSize;
	private int _playerSpeed = 20;

	public void SetParent(BaseWindow parentWindow, Vector2 startSize)
	{
		if (_parentWindow != null)
		{
			_parentWindow.WindowMoved -= OnParentWindowMoved;
			_parentWindow.WindowResized -= OnParentWindowResized;
		}
		_playerPosition = parentWindow.GetRect().GetCenter() - startSize / 2;
		_playerSize = startSize;
		_parentWindow = parentWindow;
		_parentWindow.WindowMoved += OnParentWindowMoved;
		_parentWindow.WindowResized += OnParentWindowResized;
	}

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

			_playerPosition = newPosition; // temp
		}
	}

	private void OnParentWindowMoved(Vector2I newPosition)
	{

	}

	private void OnParentWindowResized(Rect2I newRect, DisplayServer.WindowResizeEdge activeEdge)
	{

	}

	private List<Rect2I> GetUnwalkableAreas()
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

		var screenRect = new Rect2I(Vector2I.Zero, DisplayServer.ScreenGetSize());
		return screenRect.Subtract(walkableRects);
	}
}
