using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGameControls : MonoBehaviour
{
    private InputSystem_Actions _input;
    public static PlayerGameControls Instance { private set; get; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _input.Enable();

        _input.Player.SelectFirstSlot.started += OnSelectFirstSlot;
        _input.Player.SelectLastSlot.started += OnSelectLastSlot;

        _input.Player.DeleteMode.started += OnToggleDeleteMode;
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    private void OnSelectFirstSlot(InputAction.CallbackContext context)
    {
        Debug.Log("First slot is choosen!");
        PlantSystem.Instance.ChooseFirstSlot();
    }

    private void OnSelectLastSlot(InputAction.CallbackContext context)
    {
        PlantSystem.Instance.ChooseLastSlot();
    }

    private void OnToggleDeleteMode(InputAction.CallbackContext context)
    {
        PlantSystem.Instance.ToggleDeleteMode();
        Debug.Log($"Toggle delete mode. {PlantSystem.Instance.IsDeleteMode}");
    }
}
