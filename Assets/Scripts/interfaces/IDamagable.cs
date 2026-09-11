using UnityEditor;
using UnityEngine;

public interface IDamagable
{
    void SetHealth(int value);
    void AddHealth(int value);
    void SubHealth(int value);

    int GetHealth();
    bool IsInvulerable();
}