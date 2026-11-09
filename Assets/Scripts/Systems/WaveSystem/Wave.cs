using System;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public struct Wave
{
    public int Steps;
    public bool IsFinal;
    public bool IsGetUnitsByPercents;

    public int MinUnitCount;
    public int MaxUnitCount;

    public float SecondsBeforeWave;
    public float SecondsAfterWave;

    public List<GameObject> UnitsOfWave;
    public List<int> UnitPercents;
    public List<int> WeigthsOfUnits;
}
