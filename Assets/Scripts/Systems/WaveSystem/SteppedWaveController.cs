using UnityEngine;

public class SteppedWaveController : WaveController
{
    public float WaitBetweenStepsInSeconds = 1.0f;
    protected int _step_counter = 0;

    public override void Reset()
    {
        base.Reset();
        StepReset();
    }

    public void StepReset()
    {
        _step_counter = 0;
    }

    protected override void SwitchWave(ref Wave _now_wave)
    {
        if (_step_counter >= _now_wave.Steps)
        {
            IsWaitBeforeWave = true;
            IsWaitAfterWave = true;

            if (_now_wave.IsFinal)
            {
                return;
            }

            SetNowWave(_wave_counter + 1);
            StepReset();

            _before_wait = Waves[_wave_counter].SecondsBeforeWave;
            _after_wait = Waves[_wave_counter].SecondsAfterWave;
        }
        else
        {
            IsWaitBeforeWave = false;
            IsWaitAfterWave = false;

            _before_wait = WaitBetweenStepsInSeconds;
            _after_wait = 0.0f;

            _step_counter++;
        }
    }
}