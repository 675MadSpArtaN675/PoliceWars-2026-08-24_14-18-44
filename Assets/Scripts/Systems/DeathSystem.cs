using UnityEngine;

public class DeathSystem : MonoBehaviour
{
    public delegate void OnEntityDeathAction(IDamagable entity, DamageSystem.DamageNote damage);

    public static DeathSystem Instance {get; private set;}

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
    }

    public void KillUnit(GameObject unit, DamageSystem.DamageNote last_damage)
    {
        Debug.Log($"Unit to destroy: {unit}. Name: {unit?.name ?? "None"}");
        IDamagable _unit = unit.GetComponentInParent<IDamagable>();

        if (_unit != null)
        {
            _unit.OnDeathActions?.Invoke(_unit, last_damage);

            DestroyUnit(unit);
        }
    }

    public void DestroyUnit(GameObject unit)
    {
        Debug.Log($"Unit to destroy: {unit}. Name: {unit.name}");
        IDamagable _unit = unit.GetComponentInParent<IDamagable>();

        if (_unit != null)
        {
            GameObject _obj_to_destroy = (_unit as MonoBehaviour).gameObject;
            _obj_to_destroy.SetActive(false);

            Destroy(_obj_to_destroy);
        }
    }
}