using System.Collections.Generic;
using Godot;
using windows_framework.scripts.game_window;

namespace windows_framework.scripts.game;

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

    #region Resources

    private readonly List<LevelConfig> _levels = [];

    #endregion

    public Rect2I GoalRect { get; private set; }

    private int _currentLevelIndex = -1;

    [Signal] public delegate void LevelCompletedEventHandler(int completedLevelIndex, bool success = true);
    [Signal] public delegate void GameOverEventHandler();

    public override void _Ready()
    {
        LevelCompleted += OnLevelCompleted;
    }

    public override void _Process(double delta)
    {
        var playerRect = PlayerManager.Instance.PlayerRect;
        if (playerRect.Intersects(GoalRect))
            EmitSignalLevelCompleted(_currentLevelIndex, true);
    }

    public void LoadNextLevel()
    {
        int nextLevelIndex = _currentLevelIndex + 1;
        if (nextLevelIndex >= _levels.Count)
        {
            GD.Print("[GameManager]: No more levels to load.");
            EmitSignalGameOver();
            return;
        }
        LoadLevel(nextLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _levels.Count)
        {
            GD.PrintErr($"[GameManager]: Level index {levelIndex} is out of bounds.");
            return;
        }

        var levelConfig = _levels[levelIndex];
        if (levelConfig == null)
        {
            GD.PrintErr($"[GameManager]: LevelConfig at index {levelIndex} is null.");
            return;
        }

        _currentLevelIndex = levelIndex;

        GoalRect = levelConfig.GoalRect;

        bool hasStartWindow = false;
        var windowConfigs = levelConfig.WindowConfigs;
        foreach (var windowConfig in windowConfigs)
        {
            var windowInstance = WindowManager.Instance.CreateWindow(windowConfig);
            if (!windowConfig.IsStartWindow) continue;
            if (hasStartWindow)
                GD.PrintErr("[GameManager]: Multiple start windows found in level config.");
            PlayerManager.Instance.ResetPlayer(windowInstance, levelConfig.PlayerSize);
            hasStartWindow = true;
        }
    }

    private void OnLevelCompleted(int completedLevelIndex, bool success)
    {
        if (success)
        {
            GD.Print($"[GameManager]: Level {completedLevelIndex} completed successfully.");
            LoadNextLevel();
        }
        else
        {
            GD.Print($"[GameManager]: Level {completedLevelIndex} failed. Restarting level.");
            // reset current level
            LoadLevel(completedLevelIndex);
        }
    }
}
