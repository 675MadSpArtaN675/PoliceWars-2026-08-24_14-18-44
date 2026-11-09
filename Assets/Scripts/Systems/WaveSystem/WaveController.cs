using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveController : MonoBehaviour, IWaveController
{
    public delegate void WaveEvent(ref Wave _wave);
    public static WaveController Instance { get; private set; }
    public const int DEFAULT_COUNT = 25;

    [SerializeField]
    public List<Wave> Waves = new();
    public bool IsWinOnFinalWave = true;
    public bool IsWaitBeforeWave = true;
    public bool IsWaitAfterWave = true;

    public WaveEvent OnWaveStarted;
    public WaveEvent OnWaveFinalEnded;
    public WaveEvent OnWaveStepped;

    protected float _before_wait = 0.0f;
    protected float _after_wait = 0.0f;
    protected int _wave_counter = 0;

    protected bool is_can_spawn;
    protected bool is_finished;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        is_can_spawn = true;
        is_finished = false;

        _before_wait = Waves[_wave_counter].SecondsBeforeWave;
        _after_wait = Waves[_wave_counter].SecondsAfterWave;

        if (IsWinOnFinalWave)
        {
            OnWaveFinalEnded += (ref Wave wave) => GameController.Instance.WinGame();
        }
    }

    private void FixedUpdate()
    {
        GameController _game = GameController.Instance;
        if (_game != null && !_game.IsGamePaused() && !_game.IsSpawnBlocked && _game.GetGameStatus() == GameStatus.Playing) {
            SpawnController _spawn = SpawnController.Instance;

            if (_spawn != null && is_can_spawn && !_spawn.IsHasAliveEnemies())
            {
                Debug.Log($"Wave. Is spawn started: {is_can_spawn}. Is has not enemies: {!_spawn.IsHasAliveEnemies()}");
                StopSpawn();
                StartCoroutine(Step());
            }
        }
    }

    public void StartSpawn()
    {
        Debug.Log("Wave. Spawn started");
        if (_wave_counter < Waves.Count) {
            is_can_spawn = true;
            is_finished = false;
        }
    }

    public void StopSpawn()
    {
        is_can_spawn = false;
        is_finished = true;
    }

    public bool IsCanSpawn()
    {
        GameController _game = GameController.Instance;

        return is_can_spawn && !_game.IsGamePaused() && !_game.IsSpawnBlocked && !is_finished;
    }

    public virtual IEnumerator Step()
    {
        if (_wave_counter < Waves.Count || !is_finished) {
            Debug.Log($"Start wave num: {_wave_counter}.");
            Wave _now_wave = Waves[_wave_counter];

            if (IsWaitBeforeWave) {
                yield return new WaitForSecondsRealtime(_before_wait);
            }

            OnWaveStarted?.Invoke(ref _now_wave);
            ActionInWave(ref _now_wave);
            SwitchWave(ref _now_wave);

            if (IsWaitAfterWave) {
                yield return new WaitForSecondsRealtime(_after_wait);
            }

            if (_wave_counter < Waves.Count) {
                is_can_spawn = true;
            }
        }
    }

    public void SetNowWave(int wave_num)
    {
        if (wave_num >= 0 && wave_num <= Waves.Count)
        {
            _wave_counter = wave_num;
        }
    }

    public int GetNowWave()
    {
        return _wave_counter;
    }

    protected virtual void ActionInWave(ref Wave _now_wave)
    {
        Debug.Log($"Wave. Is units by percents: {_now_wave.IsGetUnitsByPercents}");
        if (_now_wave.IsGetUnitsByPercents)
        {
            int count = GetUnitCount(_now_wave.MinUnitCount, _now_wave.MaxUnitCount);

            List<GameObject> _units = _now_wave.UnitsOfWave;
            List<int> _percents = _now_wave.UnitPercents.ToList();

            Debug.Log($"Wave. Start spawning units by percents. Unit count to spawn: {count}. Units prototype: {_units.Count}.");
            AddZerosForPercents(ref _percents, _units.Count);
            SpawnCountUnits(ref _units, ref _percents, count);
        }
    }

    protected void SpawnCountUnits(ref List<GameObject> _units, ref List<int> _percents, int count)
    {
        int count_of_lines = SpawnController.sortingLayersForLines.Count();
        System.Random random = new System.Random();
        for (int i = 0; i < _units.Count; i++)
        {
            int percent = _percents[i];
            int[] parts_counts = GetCountUnitsForLines(percent, count_of_lines, count);

            int prev_num = 0;
            foreach (int p_count in parts_counts)
            {
                int line_num = 0;
                while (prev_num == line_num)
                {
                    line_num = random.Next(1, count_of_lines);
                }

                prev_num = line_num;

                Debug.Log($"Spawning unit '{_units[i].name}'. Line num: {line_num}. Part count: {p_count}.");
                SpawnController.Instance?.SpawnEnemy(_units[i], line_num, p_count);
            }
        }
    }

    private int[] GetCountUnitsForLines(int _percent, int count_of_lines, int units_count)
    {
        System.Random random = new System.Random();
        int count_of_unit = (int) Math.Ceiling(units_count * ((double) _percent / 100));
        int divider_on_spawners = random.Next(1, count_of_lines);
        int[] weights = new int[divider_on_spawners];

        if (count_of_unit <= 1)
        {
            return new[]{count_of_unit};
        }

        Debug.Log($"Wave. Unit %: {(double) _percent / 100}. Unit count: {count_of_unit}. Line count: {count_of_lines}. Full unit count: {units_count}. Divider: {divider_on_spawners}");
        int weights_max = 100;
        for (int i = 0; i < divider_on_spawners; i++)
        {
            int start = (int) Math.Ceiling((double) weights_max / 2);

            if (start > weights_max)
            {
                start = 0;
            }

            double percents = (double) random.Next(start, weights_max) / 100;
            int weight = (int) Math.Ceiling(count_of_unit * percents);
            count_of_unit -= weight;

            weights[i] = weight;
            weights_max -= weight;
        }

        int sum_of_weights = weights.Sum();
        if (sum_of_weights < count_of_unit)
        {
            weights[0] += count_of_unit - sum_of_weights;
        }

        Debug.Log($"Wave. Calculated weights: {weights.Select((int c) => c.ToString()).Aggregate((p, x) => p + " " + x).Trim()}");

        return weights;
    }

    private void AddZerosForPercents(ref List<int> _percents, int units_count)
    {
        int unit_count_and_percent_diff = Math.Abs(units_count - _percents.Count);

        if (unit_count_and_percent_diff > 0)
        {
            int[] remains_percents = new int[unit_count_and_percent_diff];
            Array.Fill(remains_percents, 0);

            _percents.AddRange(remains_percents);
        }
    }

    protected int GetUnitCount(int min_count, int max_count)
    {
        System.Random random = new System.Random();

        if (max_count > 0)
        {
            if (min_count > max_count)
            {
                min_count = 1;
            }
        }
        else
        {
            min_count = DEFAULT_COUNT / 2;
            max_count = DEFAULT_COUNT;
        }


        return random.Next(min_count, max_count);
    }

    protected virtual void SwitchWave(ref Wave _now_wave)
    {
        if (_wave_counter < Waves.Count)
        {
            if (_now_wave.IsFinal)
            {
                SetNowWave(_wave_counter + 1);
                Debug.Log($"Wave. End wave: {_wave_counter}");

                is_finished = true;
                OnWaveFinalEnded?.Invoke(ref _now_wave);
                if (IsWinOnFinalWave)
                {
                }
                return;
            }

            SetNowWave(_wave_counter + 1);

            _before_wait = Waves[_wave_counter].SecondsBeforeWave;
            _after_wait = Waves[_wave_counter].SecondsAfterWave;

            OnWaveStepped?.Invoke(ref _now_wave);
        }
    }

    public virtual void Reset()
    {
        Debug.Log("Reset waves!");
        _wave_counter = 0;
    }

}