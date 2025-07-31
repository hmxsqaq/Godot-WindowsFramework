using Godot;

namespace windows_framework.scripts;

public partial class GameButton : Button
{
    [Export]
    private float _scaleFactor = 1.1f;
    [Export]
    private double _animationDuration = 0.2;

    private Vector2 _originalScale;
    private Vector2 _targetScale;
    
    private Tween _tween;
    
    public override void _Ready()
    {
        PivotOffset = Size / 2;
        
        _originalScale = Scale;
        _targetScale = _originalScale * _scaleFactor;

        MouseEntered += () => AnimateScale(_targetScale);
        MouseExited += () => AnimateScale(_originalScale);
    }

    private void AnimateScale(Vector2 targetScale)
    {
        _tween?.Kill();
        _tween = GetTree().CreateTween();
        _tween.TweenProperty(this, "scale", targetScale, _animationDuration)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Cubic);
    }
}