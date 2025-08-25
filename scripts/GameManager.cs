using Godot;

namespace windows_framework.scripts;

public partial class GameManager : Node
{
    #region Singleton

    public static GameManager Instance { get; private set; }
    
    public override void _EnterTree()
    {
        if (Instance != null)
        {
            GD.PrintErr("[GameWindowManager]: Instance already exists. Destroying the new instance.");
            QueueFree();
            return;
        }
        Instance = this;
    }

    #endregion

    [Signal]
    public delegate void LevelCompleteEventHandler(bool isSuccess);
    
    [Signal]
    public delegate void LevelLoadEventHandler(int level);

    public override void _Ready()
    {
        LevelComplete += OnLevelComplete;
        LevelLoad += OnLevelLoad;
    }
    
    private void OnLevelComplete(bool isSuccess)
    {
        GD.PrintErr($"[GameWindowManager]: LevelComplete {isSuccess}");
    }

    private void OnLevelLoad(int level)
    {
        GD.PrintErr($"[GameWindowManager]: LevelLoad {level}");
    }
}