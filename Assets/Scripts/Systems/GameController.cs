using Unity.VisualScripting;
using UnityEngine;

public enum GameStatus
{
    Preparing,
    Playing,
    Win,
    Lose,
}

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    public delegate void onEventStart(GameStatus old_status, GameStatus new_status);

    [InspectorName("Has prepare time")]
    public bool has_prepare_time;

    [InspectorName("On game status changed")]
    public onEventStart onStatusChange;

    public bool isGamePaused { get; private set; }

    private GameStatus _status;

    private void Start()
    {
        if (Instance == null) {
            Instance = this;
        }
    }
    public void StartGame()
    {
        GameStatus status = _status;

        if (has_prepare_time) {
            _status = GameStatus.Preparing;
        }
        else {
            _status = GameStatus.Playing;
        }

        onStatusChange?.Invoke(status, _status);
    }

    public void WinGame()
    {
        GameStatus status = _status;
        _status = GameStatus.Win;

        onStatusChange?.Invoke(status, _status);
    }

    public void LoseGame()
    {
        GameStatus status = _status;
        _status = GameStatus.Lose;

        onStatusChange?.Invoke(status, _status);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        isGamePaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        isGamePaused = false;
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }
    public GameStatus GetGameStatus()
    {
        return _status;
    }
}