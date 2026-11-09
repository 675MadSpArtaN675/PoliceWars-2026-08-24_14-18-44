using UnityEngine.UIElements;

using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public class UnitsChooseWindow : MonoBehaviour
{
    public Vector2Int CHOOSE_BUTTON_SIZE = new Vector2Int(150, 175);
    public Vector2Int CHOOSEN_BUTTON_SIZE = Vector2Int.zero;

    protected List<GameObject> _choosed_units;

    protected uint _max_unit_count;
    protected Dictionary<string, GameObject> _units_to_choose;

    protected Dictionary<string, VisualElement> _units_buttons;
    protected Dictionary<string, VisualElement> _choosen_units_buttons;

    protected PanelRenderer _document;
    protected VisualTreeAsset _button_of_unit;

    protected VisualElement _choosen_units_list;
    protected VisualElement _units_for_choosing_list;

    private int _versionUI;

    private void Start()
    {
        _document = GetComponent<PanelRenderer>();
        GameSystemsInitializator.Instance.OnInitEnded += () => _max_unit_count = GameSystemsInitializator.Instance.UnitSlotsCount;

        _button_of_unit = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Settings/UI/ButtonOfUnit.uxml");

        if (CHOOSEN_BUTTON_SIZE == Vector2Int.zero) {
            int width = Mathf.CeilToInt(CHOOSE_BUTTON_SIZE.x * 0.5f);
            int height = Mathf.CeilToInt(CHOOSE_BUTTON_SIZE.y * 0.5f);

            CHOOSEN_BUTTON_SIZE = new Vector2Int(width, height);
        }
    }

    private void OnEnable()
    {
        InitOfCollections();

        AddUnitToChooseListBackend(Resources.Load<GameObject>("Objects/NPC/Police/TestPoliceman"));
        AddUnitToChooseListBackend(Resources.Load<GameObject>("Objects/NPC/Police/TestMeleePoliceman"));

        _document = GetComponentInChildren<PanelRenderer>();
        _document.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        _document.UnregisterUIReloadCallback(OnUIReload);

        InitOfCollections();
    }

    protected virtual void InitOfCollections()
    {
        _units_buttons = new();
        _units_to_choose = new();

        _choosed_units = new();
        _choosen_units_buttons = new();

        _max_unit_count = GameSystemsInitializator.Instance?.UnitSlotsCount ?? 0;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement element, int version)
    {
        if (version == _versionUI) {
            return;
        }

        _versionUI = version;

        _choosen_units_list = element.Q<VisualElement>("choosen_units_row");
        _units_for_choosing_list = element.Q<VisualElement>("units_list");
        VisualElement _play_button = element.Q<VisualElement>("play_button");
        _play_button.RegisterCallback<ClickEvent>((_event) => OnPressPlay(_event, element));

        foreach (string id_of_unit in _units_to_choose.Keys)
        {
            AddUnitToChooseListVisual(id_of_unit, CHOOSE_BUTTON_SIZE.x, CHOOSE_BUTTON_SIZE.y);
        }
    }

    protected void OnPressPlay(ClickEvent _event, VisualElement element)
    {
        GameSystemsInitializator gsi = GameSystemsInitializator.Instance;
        int units_count = _choosed_units.Count;

        if (units_count > 0) {
            element.visible = false;

            GameObject[] choosed_units = _choosed_units.ToArray();

            if (gsi != null) {
                gsi.UnitSlotsCount = (uint) units_count;
                gsi.StartUnits = choosed_units;

                gsi.Init();

                GameController.Instance?.StartGame();
            }
        }
    }

    public void Open()
    {
        if (_document != null && _document.GetComponent<UIDocument>() is UIDocument _ui && _ui != null)
        {
            _ui.rootVisualElement.visible = true;
        }
    }

    public void AddUnitToChooseList(GameObject _obj)
    {
        if (_obj != null) {
            string id_of_unit = AddUnitToChooseListBackend(_obj);
            AddUnitToChooseListVisual(id_of_unit, CHOOSE_BUTTON_SIZE.x, CHOOSE_BUTTON_SIZE.y);
        }
    }

    public void RemoveUnitFromChoose(string id_unit)
    {
        RemoveUnitFromChooseListBackend(id_unit);
        RemoveUnitFromChooseListVisual(id_unit);
    }

    public List<GameObject> GetAccessedToChooseUnits()
    {
        return _units_to_choose.Values.ToList();
    }

    public void ChooseUnitToSlot(string unit_id)
    {
        if (_choosed_units.Count < _max_unit_count) {
            ChooseUnitNoVisual(unit_id);
            AddChoosenUnitToVisibleListVisual(unit_id, CHOOSEN_BUTTON_SIZE.x, CHOOSEN_BUTTON_SIZE.y);
        }
    }

    public void UnchooseUnitFromSlot(string unit_id)
    {
        UnchooseUnitNoVisual(unit_id);
        RemoveUnitFromChoosenListVisual(unit_id);
    }

    protected void ChooseUnitNoVisual(string unit_id)
    {
        if (_units_to_choose.ContainsKey(unit_id))
        {
            GameObject _unit = _units_to_choose[unit_id];

            if (_unit != null && !_choosed_units.Contains(_unit))
            {
                _choosed_units.Add(_unit);
            }
        }
    }

    protected void UnchooseUnitNoVisual(string unit_id)
    {
        if (_units_to_choose.ContainsKey(unit_id))
        {
            int index = GetUnitIndex(unit_id);

            if (index >= 0)
            {
                _choosed_units.RemoveAt(index);
            }
        }
    }

    public List<GameObject> GetChoosedUnits()
    {
        return _choosed_units;
    }


    protected int GetUnitIndex(string to_find_unit_id)
    {
        return _choosed_units.FindIndex((GameObject _unit) => UnitIndexFindPredicate(_unit, to_find_unit_id));
    }

    protected bool UnitIndexFindPredicate(GameObject _obj, string to_find_unit_id)
    {
        if (_obj.GetComponent<BattleBrain>() is BattleBrain _brain)
        {
            return _brain.IDOfUnit == to_find_unit_id;
        }

        return false;
    }

    protected string AddUnitToChooseListBackend(GameObject _obj)
    {
        if (_obj != null && _obj.GetComponentInChildren<BattleBrain>(true) is BattleBrain _brain && _brain != null)
        {
            string id_of_unit = _brain.IDOfUnit;

            if (id_of_unit.Length > 0)
            {
                _units_to_choose.Add(id_of_unit, _obj);
            }

            return id_of_unit;
        }

        return null;
    }

    protected void AddUnitToChooseListVisual(string unit_id, int width, int height)
    {
        if (_units_to_choose.ContainsKey(unit_id) && !_units_buttons.ContainsKey(unit_id)) {
            VisualElement _element = _button_of_unit.Instantiate();
            _element.style.width = width;
            _element.style.height = height;
            _element.RegisterCallback<ClickEvent>((event_name) => AddChoosenUnitToVisibleListHandler(event_name, unit_id, CHOOSEN_BUTTON_SIZE.x, CHOOSEN_BUTTON_SIZE.y));

            _units_buttons.Add(unit_id, _element);
            _units_for_choosing_list.Add(_element);
        }
    }

    protected void RemoveUnitFromChooseListBackend(string unit_id)
    {
        if (_units_to_choose.ContainsKey(unit_id))
        {
            _units_to_choose.Remove(unit_id);
        }
    }

    protected void RemoveUnitFromChooseListVisual(string unit_id)
    {
        if (_units_buttons.ContainsKey(unit_id))
        {
            VisualElement _element = _units_buttons[unit_id];

            _units_buttons.Remove(unit_id);
            _units_for_choosing_list.Remove(_element);

            _element.RemoveFromHierarchy();
        }
    }

    protected VisualElement GetUnitFromChooseListVisual(string unit_id)
    {
        if (_units_buttons.ContainsKey(unit_id))
        {
            return _units_buttons[unit_id];
        }

        return null;
    }


    protected void AddChoosenUnitToVisibleListHandler(ClickEvent _event, string unit_id, int width, int height)
    {
        if (_event.target is VisualElement && _choosed_units.Count < _max_unit_count)
        {
            AddChoosenUnitToVisibleListVisual(unit_id, width, height);
        }
    }

    protected void RemoveChoosenUnitToVisibleListHandler(ClickEvent _event, string unit_id)
    {
        if (_event.target is VisualElement)
        {
            RemoveUnitFromChoosenListVisual(unit_id);
        }
    }


    protected void AddChoosenUnitToVisibleListVisual(string unit_id, int width, int height)
    {
        if (!_choosen_units_buttons.ContainsKey(unit_id)) {
            VisualElement _button_voited = _units_buttons[unit_id];
            _button_voited.SetEnabled(false);

            VisualElement _button = _button_of_unit.Instantiate();
            _button.style.width = width;
            _button.style.height = height;
            _button.RegisterCallback<ClickEvent>((event_name) => RemoveChoosenUnitToVisibleListHandler(event_name, unit_id));

            ChooseUnitNoVisual(unit_id);

            _choosen_units_list.Add(_button);
            _choosen_units_buttons.Add(unit_id, _button);
        }
    }

    protected void RemoveUnitFromChoosenListVisual(string unit_id)
    {
        if (_choosen_units_buttons.ContainsKey(unit_id)){
            VisualElement parent_in_full_list = _units_buttons[unit_id];
            parent_in_full_list.SetEnabled(true);

            UnchooseUnitNoVisual(unit_id);

            VisualElement _delete_button = _choosen_units_buttons[unit_id];
            _choosen_units_buttons.Remove(unit_id);

            _delete_button.RemoveFromHierarchy();
        }
    }

    protected VisualElement GetUnitFromChoosenListVisual(string unit_id)
    {
        if (_choosen_units_buttons.ContainsKey(unit_id))
        {
            return _choosen_units_buttons[unit_id];
        }

        return null;
    }
}
