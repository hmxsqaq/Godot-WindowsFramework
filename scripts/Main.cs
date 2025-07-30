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
			var windowTitle = _lineEdit.Text;
			var windowPos = new Vector2I((int)_positionXSpinBox.Value, (int)_positionYSpinBox.Value);
			var windowSize = new Vector2I((int)_sizeXSpinBox.Value, (int)_sizeYSpinBox.Value);
			
			var windowRect = new Rect2I(windowPos, windowSize);
			
			var newWindow = _gameWindow.Instantiate<GameWindow>();
			newWindow.SetTitle(windowTitle);
			GetTree().Root.AddChild(newWindow);
			newWindow.Popup(windowRect);
		};
	}
}
