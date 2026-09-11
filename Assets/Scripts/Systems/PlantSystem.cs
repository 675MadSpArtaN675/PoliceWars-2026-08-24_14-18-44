using System;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlantSystem : MonoBehaviour
{
    public static PlantSystem Instance { private set; get; }

    [SerializeField]
    [InspectorName("Slots of unit count")]
    private uint slots_count = 5;

    private GameObject[] _units;
    private int _choosen_slot = -1;

    private void Awake()
    {
        Debug.Log("Initializing a plant system");
        if (Instance == null)
        {
            Instance = this;
        }

        _units = new GameObject[slots_count];
        Array.Fill(_units, null);

        _units[0] = Resources.Load<GameObject>("Objects/NPC/TestPlant");
    }

    public void Plant(Vector3 position)
    {
        if (_choosen_slot < slots_count && _choosen_slot >= 0 && _units[_choosen_slot] != null) {
            Debug.Log($"Instancing object on slot: {_choosen_slot}");
            Instantiate(_units[_choosen_slot], position, Quaternion.Euler(Vector3.zero));
        }

        ClearChoose();
    }

    public void ChooseSlot(int slot_index)
    {
        Debug.Log($"Choosing a slot with index: {slot_index}");
        if (slot_index >= 0 && slot_index < slots_count) {
            _choosen_slot = slot_index;
            Debug.Log("Slot is choosen.");
        }
    }

    public void ChooseFirstSlot()
    {
        _choosen_slot = 0;
    }
    public void ChooseLastSlot()
    {
        _choosen_slot = (int) slots_count - 1;
    }

    public void SetUnitToSlot(int slot_index, GameObject unit)
    {
        if (slot_index > 0 && slot_index < slots_count)
        {
            _units[slot_index] = unit;
        }
    }

    public bool IsFilled()
    {
        bool is_empty = true;
        foreach (GameObject unit in _units)
        {
            if (unit != null)
            {
                is_empty = false;
                break;
            }
        }

        return is_empty;
    }

    public void AddUnit(GameObject unit)
    {
        bool added = false;
        if (!_units.Contains(unit)) {
            for(int i = 0; i < slots_count; i++)
            {
                GameObject exists_unit = _units[i];

                if (exists_unit == null && exists_unit != unit)
                {
                    _units[i] = unit;
                    added = true;
                    break;
                }
            }
        }
    }

    public int GetChoosenSlotNum()
    {
        return _choosen_slot;
    }

    public void ClearChoose()
    {
        _choosen_slot = -1;
    }

    public void ClearSlot(int slot_index)
    {
        if (slot_index > 0 && slot_index < slots_count)
        {
            _units[slot_index] = null;
        }
    }

    public void ClearSlots()
    {
        if (slots_count > 0)
        {
            Array.Fill(_units, null);
        }
    }
}
