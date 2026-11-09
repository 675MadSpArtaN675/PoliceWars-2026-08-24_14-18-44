using System.Collections;
using UnityEngine;

public enum GameStatus
{
    Entering,
    Preparing,
    Playing,
    Win,
    Lose,
}

public class GameController : MonoBehaviour
{
    public delegate void onEventStart(GameStatus old_status, GameStatus new_status);

    public static GameController Instance { get; private set; }
    public bool DebugMode;
    public bool isGamePaused { get; private set; }
    public bool IsSpawnBlocked = false;

    [InspectorName("Player's team")]
    [SerializeField]
    private Team _pl_team = Team.Policeman;

    public Team PlayerTeam { get => _pl_team; set => _pl_team = value; }

    [InspectorName("Prepare time")]
    public uint PrepareTime;

    [InspectorName("On game status changed")]
    public onEventStart onStatusChange;

    private GameStatus _status;

    private void Awake()
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
            onStatusChange?.Invoke(status, _status);

            StartCoroutine(DelayToStartGame(status));

            return;
        }
        else {
            _status = GameStatus.Playing;
        }

        onStatusChange?.Invoke(status, _status);
    }

    protected IEnumerator DelayToStartGame(GameStatus old_status)
    {
        yield return new WaitForSecondsRealtime(PrepareTime);

        _status = GameStatus.Playing;

        onStatusChange?.Invoke(old_status, _status);
    }

    public void WinGame()
    {
        Debug.Log("Game won!");
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