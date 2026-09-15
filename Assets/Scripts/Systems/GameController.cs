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
    public delegate void onEventStart(GameStatus old_status, GameStatus new_status);

    public static GameController Instance { get; private set; }

    [SerializeField]
    [InspectorName("Player's team")]
    private Team _pl_team = Team.Policeman;
    public Team PlayerTeam { get => _pl_team; set => _pl_team = value; }

    [InspectorName("Prepare time")]
    public uint PrepareTime;

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

        if (PrepareTime > 0) {
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