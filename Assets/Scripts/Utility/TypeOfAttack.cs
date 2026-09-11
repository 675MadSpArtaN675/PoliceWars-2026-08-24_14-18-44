using UnityEngine;

public enum TypeOfAttack
{
    [InspectorName("Не атакует")]
    Empty,

    [InspectorName("Ближний бой")]
    Melee,

    [InspectorName("Дальний бой")]
    Ranged,

    [InspectorName("Совмещённые")]
    MeleeAndRanged,

    [InspectorName("Контролируемое Самоуничтожение")]
    ControlledSelfDestruct,

    [InspectorName("Самоуничтожение")]
    SelfDestruct
}