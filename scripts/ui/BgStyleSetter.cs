using Godot;

namespace windows_framework.scripts.ui;

public partial class BgStyleSetter : Panel
{
	private StyleBoxFlat _styleBox;

	public override void _Ready() => InstantiateStyleBox();

	private void InstantiateStyleBox()
	{
		if (_styleBox != null) return;
		var template = GetThemeStylebox("panel");
		if (template is not StyleBoxFlat)
		{
			GD.PrintErr($"[BgStyleSetter: {Name}] Theme stylebox 'panel' is not a StyleBoxFlat.");
			return;
		}
		_styleBox = (StyleBoxFlat)template.Duplicate();
		AddThemeStyleboxOverride("panel", _styleBox);
	}

	public void SetColor(Color newColor) => _styleBox.BgColor = newColor;

	public void SetBorderColor(Color newColor) => _styleBox.BorderColor = newColor;
}
