using System.Collections;

public interface IWaveController
{
    IEnumerator Step();
    public void Reset();
}