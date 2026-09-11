using UnityEngine;

public class DeathSystem : MonoBehaviour
{
    public static DeathSystem Instance {get; private set;}

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
    }

    public void DestroyUnit(GameObject unit)
    {
        Debug.Log($"Unit to destroy: {unit}. Name: {unit.name}");
        IDamagable _unit = unit.GetComponentInParent<IDamagable>() ?? unit.GetComponent<IDamagable>();

        if (_unit != null)
        {
            GameObject _obj_to_destroy = (_unit as MonoBehaviour).gameObject;
            _obj_to_destroy.SetActive(false);

            Destroy(_obj_to_destroy);
        }
    }
}