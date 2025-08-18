using Godot;

namespace windows_framework.scripts;

public partial class Main : Control
{
	[Export] private PackedScene _gameWindow;
	[Export] private Button _launchButton;
	[Export] private LineEdit _lineEdit;
	[Export] private SpinBox _positionXSpinBox;
	[Export] private SpinBox _positionYSpinBox;
	[Export] private SpinBox _sizeXSpinBox;
	[Export] private SpinBox _sizeYSpinBox;

	public override void _Ready()
	{
		_launchButton.Pressed += () =>
		{
			var position = new Vector2I((int)_positionXSpinBox.Value, (int)_positionYSpinBox.Value);
			var size = new Vector2I((int)_sizeXSpinBox.Value, (int)_sizeYSpinBox.Value);
			game_window.WindowManager.Instance.LaunchNewWindow(_gameWindow, position, size);
		};
	}
}
