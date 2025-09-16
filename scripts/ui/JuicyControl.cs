using Godot;

namespace windows_framework.scripts.ui;

public partial class JuicyControl : Node
{
    [ExportGroup("Scale")]
    [Export] private float _scaleFactor = 1.1f;
    [Export] private double _animationDuration = 0.2;

    [ExportGroup("Color")]
    [Export] private Color _hoverColor = Colors.White;
    [Export] private Color _unhoverColor = new(1, 1, 1, 0.5f);

    private Vector2 _originalScale;
    private Vector2 _targetScale;
    private Tween _tween;
    private bool _mouseOver = false;

    private Control _target;
    
    public override void _Ready()
    {
        _target = GetParent<Control>();
        if (_target == null)
        {
            GD.PrintErr("JuicyControl must be a child of a Control node.");
            return;
        }

        _target.PivotOffset = _target.Size / 2;
        
        _originalScale = _target.Scale;
        _targetScale = _originalScale * _scaleFactor;
        _target.Modulate = _unhoverColor;

        _target.MouseEntered += () =>
        {
            _mouseOver = true;
            AnimateScale(_targetScale);
            _target.Modulate = _hoverColor;
        };

        _target.MouseExited += () =>
        {
            _mouseOver = false;
            AnimateScale(_originalScale);
            _target.Modulate = _unhoverColor;
        };
    }

    public override void _Input(InputEvent @event)
    {
        if (_mouseOver && @event is InputEventMouseMotion mouseEvent && mouseEvent.IsPressed())
        {
            _tween?.Kill();
            _target.Scale = _originalScale;
        }
    }

    private void AnimateScale(Vector2 targetScale)
    {
        _tween?.Kill();
        _tween = GetTree().CreateTween();
        _tween.TweenProperty(_target, "scale", targetScale, _animationDuration)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Cubic);
    }
}
