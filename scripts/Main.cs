using Godot;
using windows_framework.scripts.game_window;
using windows_framework.scripts.game_window.behaviors;
using windows_framework.scripts.player;

namespace windows_framework.scripts;

public partial class Main : Control
{
	[Export] private Button _launchButton;
	
	[Export] private LineEdit _titleLineEdit;
	[Export] private SpinBox _positionXSpinBox;
	[Export] private SpinBox _positionYSpinBox;
	[Export] private SpinBox _sizeXSpinBox;
	[Export] private SpinBox _sizeYSpinBox;
	[Export] private CheckBox _passableCheckBox;
	[Export] private CheckBox _unblockableCheckBox;

	private bool _isFirstWindow = true;

	public override void _Ready()
	{
		_launchButton.Pressed += () =>
		{
			var windowConfig = new WindowConfig()
			{
				Title = _titleLineEdit.Text,
				Position = new Vector2I((int)_positionXSpinBox.Value, (int)_positionYSpinBox.Value),
				Size = new Vector2I((int)_sizeXSpinBox.Value, (int)_sizeYSpinBox.Value)
			};
			windowConfig.Behaviors[BehaviorType.Movable] = true;
			windowConfig.Behaviors[BehaviorType.Resizable] = true;
			windowConfig.Behaviors[BehaviorType.WindowInfo] = true;
			windowConfig.Behaviors[BehaviorType.Passable] = _passableCheckBox.ButtonPressed;
			windowConfig.Behaviors[BehaviorType.UnBlockable] = _unblockableCheckBox.ButtonPressed;
			windowConfig.Behaviors[BehaviorType.Walkable] = true;
			
			var window = WindowManager.Instance.CreateWindow(windowConfig);

			if (!_isFirstWindow) return;
			_isFirstWindow = false;
			PlayerManager.Instance.SetParent(window, new Vector2(50, 50));
		};
	}
}
