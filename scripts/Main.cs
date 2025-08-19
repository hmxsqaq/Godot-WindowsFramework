using Godot;
using windows_framework.scripts.game_window;

namespace windows_framework.scripts;

public partial class Main : Control
{
	[Export] private WindowConfig _windowConfig;
	
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
			if (_windowConfig == null)
			{
				GD.PrintErr("[Main] WindowConfig is not assigned in the inspector.");
				return;
			}
			
			var window = WindowManager.Instance.CreateWindow(_windowConfig);
		};
	}
}
