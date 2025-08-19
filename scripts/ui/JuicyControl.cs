using Godot;

namespace windows_framework.scripts.ui;

public partial class JuicyControl : Node
{
    [Export] private float _scaleFactor = 1.1f;
    [Export] private double _animationDuration = 0.2;
    
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

        _target.MouseEntered += () =>
        {
            AnimateScale(_targetScale);
            _mouseOver = true;
        };

        _target.MouseExited += () =>
        {
            _mouseOver = false;
            AnimateScale(_originalScale);
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