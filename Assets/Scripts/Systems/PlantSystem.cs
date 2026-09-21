using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PlantSystem : MonoBehaviour
{
    public static float UnitCheckupDistance = 5;
    public static Vector2 CheckupDirection = new Vector2(1, 0);
    public static PlantSystem Instance { private set; get; }

    public enum CollideLineLayer
    {
        BankLayer,
        AttackLine_1,
        AttackLine_2,
        AttackLine_3,
        AttackLine_4,
        AttackLine_5
    }

    public enum SortingLayerLine
    {
        Bank,
        Line_1,
        Line_2,
        Line_3,
        Line_4,
        Line_5
    }

    public struct UnitAdditionalSettings
    {
        public static void SetMasksForComponents(GameObject gameObj, UnitAdditionalSettings settings)
        {
            Debug.Log($"Object: {gameObj?.name ?? "None"}.");
            foreach (IGameUnit unit_base in gameObj.GetComponentsInChildren<IGameUnit>())
            {
                unit_base.ColliderLayer = settings.collideLineLayer;
            }

            foreach (Transform _trans in gameObj.GetComponentsInChildren<Transform>())
            {
                Debug.Log($"Mask unit: {settings.current_collide_id}:{settings.current_mask_num}");
                _trans.gameObject.layer = settings.current_collide_id;
            }

            foreach (SpriteRenderer _sprite in gameObj.GetComponentsInChildren<SpriteRenderer>())
            {
                _sprite.sortingLayerID = settings.sorting_layer_id;
                _sprite.sortingOrder = settings.sorting_order;
            }
        }

        public static UnitAdditionalSettings CreateInstance(CollideLineLayer collide_layer_id, string current_sorting_layer_name)
        {
            return new UnitAdditionalSettings(collide_layer_id, LayerMask.NameToLayer(collide_layer_id.ToString()), LayerMask.GetMask(collide_layer_id.ToString()), SortingLayer.NameToID(current_sorting_layer_name), 0);
        }

        public static UnitAdditionalSettings CreateInstance(CollideLineLayer current_name_mask, SortingLayerLine current_sorting_layer_name)
        {
            return CreateInstance(current_name_mask, current_sorting_layer_name.ToString());
        }

        public static UnitAdditionalSettings CreateInstance(CollideLineLayer current_collide_mask)
        {
            SortingLayerLine _line = (SortingLayerLine) current_collide_mask;

            return CreateInstance(current_collide_mask, _line.ToString());
        }

        public CollideLineLayer collideLineLayer;
        public int current_collide_id;
        public int current_mask_num;
        public int sorting_layer_id;
        public int sorting_order;

        public UnitAdditionalSettings(CollideLineLayer collideLineLayer_, int current_collide_id_, int current_mask_num_, int sorting_layer_id_, int sorting_order_)
        {
            collideLineLayer = collideLineLayer_;
            current_collide_id = current_collide_id_;
            current_mask_num = current_mask_num_;
            sorting_layer_id = sorting_layer_id_;
            sorting_order = sorting_order_;
        }

        public override string ToString()
        {
            return $"{current_collide_id}:{current_mask_num}:{sorting_layer_id}";
        }
    }

    private uint slots_count;

    [SerializeField]
    private bool _is_delete_mode = false;

    private GameObject[] _units = new GameObject[5];
    private int _choosen_slot = -1;


    [InspectorName("Slots of unit count")]
    public uint SlotsCount = 5;
    public bool IsDeleteMode { get => _is_delete_mode; set => _is_delete_mode = true; }
    public int ChoosenUnitSlot { get => _choosen_slot; set => _choosen_slot = value; }

    private void Start()
    {
        Debug.Log("Initializing a plant system");
        if (Instance == null)
        {
            Instance = this;
        }

        SlotsRecreate();
        SetUnitToSlot(0, Resources.Load<GameObject>("Objects/NPC/Police/TestPoliceman"));
    }

    public void SlotsRecreate()
    {
        slots_count = SlotsCount;

        _units = new GameObject[slots_count];
        Array.Fill(_units, null);
    }

    public GameObject PlaceUnit(Vector3 position, UnitAdditionalSettings? settings = null)
    {
        GameObject planted_unit = null;
        Debug.Log($"Planting on position: {position}. Slot: {_choosen_slot}. Is slot empty: {IsSlotEmpty(_choosen_slot)}.");
        if (IsValidSlot(_choosen_slot) && !IsSlotEmpty(_choosen_slot) && !_is_delete_mode) {
            Debug.Log($"Planting object on slot: {_choosen_slot}");

            planted_unit = Instantiate(GetUnit(_choosen_slot), position, Quaternion.Euler(Vector3.zero));

            if (settings.HasValue)
            {
                IGameUnit _unit_settings = planted_unit.GetComponent<IGameUnit>();
                _unit_settings.ColliderLayer = settings.Value.collideLineLayer;

                planted_unit.GetComponent<IGameUnit>()?.SetOptions(settings.Value);
            }

            planted_unit.SetActive(true);
        }

        ClearChoose();

        return planted_unit;
    }

    public void DeleteUnit(GameObject _finded_unit)
    {
        if (_is_delete_mode && _finded_unit != null) {
            Mover _mover = _finded_unit.GetComponentInParent<Mover>();

            if ((_mover != null && _mover.IsNotMovable()) || _mover == null)
            {
                DeathSystem.Instance.DestroyUnit(_finded_unit.gameObject);
            }
        }
    }

    public void ToggleDeleteMode()
    {
        if (GetDeleteModeStatus())
        {
            Debug.Log($"Status death mode: {GetDeleteModeStatus()}");
            DisableDeleteMode();
        }
        else
        {
            Debug.Log($"Status death mode: {GetDeleteModeStatus()}");
            EnableDeleteMode();
        }
    }

    public void EnableDeleteMode()
    {
        if (!_is_delete_mode)
        {
            SetDeleteModeStatus(true);
        }
    }

    public void DisableDeleteMode()
    {
        if (_is_delete_mode)
        {
            SetDeleteModeStatus(false);
        }
    }

    public void SetDeleteModeStatus(bool value)
    {
        _is_delete_mode = value;
    }

    public bool GetDeleteModeStatus()
    {
        return _is_delete_mode;
    }

    public void ChooseSlot(int slot_index)
    {
        Debug.Log($"Choosing a slot with index: {slot_index}");
        if (IsValidSlot(slot_index) && !_is_delete_mode) {
            _choosen_slot = slot_index;
            Debug.Log("Slot is choosen.");
            SetDeleteModeStatus(false);
        }
    }

    public void ChooseFirstSlot()
    {
        ChooseSlot(0);
    }
    public void ChooseLastSlot()
    {
        ChooseSlot(GetLastNonEmptySlot());
    }

    public void SetUnitToSlot(int slot_index, GameObject unit)
    {
        if (IsValidSlot(slot_index))
        {
            ITeamable teamable = unit?.GetComponent<ITeamable>();
            Debug.Log($"PlantSystem set unit to slot: {unit?.name ?? "None"}. Team: {teamable?.Team ?? Team.NoTeam}. Player team: {GameController.Instance.PlayerTeam}");
            if (teamable?.Team == GameController.Instance.PlayerTeam)
            {
                _units[slot_index] = unit;
                Debug.Log($"PlantSystem unit to slot: {_units[slot_index]?.name ?? "None"}.");
            }
        }
    }

    public GameObject GetUnit(int slot_index)
    {
        if (IsValidSlot(slot_index))
        {
            Debug.Log($"Unit slot name: {_units[slot_index]?.name ?? "None"}");
            return _units[slot_index];
        }

        return null;
    }

    public void AddUnit(GameObject unit)
    {
        if (!_units.Contains(unit)) {
            for(int i = 0; i < slots_count; i++)
            {
                GameObject exists_unit = _units[i];

                if (exists_unit == null && exists_unit != unit)
                {
                    SetUnitToSlot(i, unit);
                    break;
                }
            }
        }
    }

    public void ClearSlot(int slot_index)
    {
        SetUnitToSlot(slot_index, null);
    }

    public int GetChoosenSlotNum()
    {
        return _choosen_slot;
    }

    public void ClearChoose()
    {
        _choosen_slot = -1;
    }

    public void ClearSlots()
    {
        if (slots_count > 0)
        {
            Array.Fill(_units, null);
        }
    }

    public bool IsSlotEmpty(int slot_index)
    {
        return (IsValidSlot(slot_index) && _units[slot_index] == null) || !IsValidSlot(slot_index);
    }

    public bool IsAnySlotChoosen()
    {
        return ChoosenUnitSlot >= 0;
    }

    public bool IsValidSlot(int slot_index)
    {
        Debug.Log($"Index: {slot_index}. Slot count: {slots_count}. Is valid: {slot_index >= 0 && slot_index < slots_count}");
        return slot_index >= 0 && slot_index < slots_count;
    }

    public bool IsFilled()
    {
        return GetLastNonEmptySlot() == slots_count - 1;
    }

    public int GetLastNonEmptySlot()
    {
        int last_index = -1;
        foreach (GameObject unit in _units)
        {
            if (unit == null)
            {
                break;
            }

            last_index++;
        }
        return last_index;
    }
}
