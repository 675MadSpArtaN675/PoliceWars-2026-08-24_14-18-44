using UnityEngine;

public class ChoosableUnit : MonoBehaviour
{
    public bool IsCanBeChoose = true;

    private void OnMouseDown()
    {
        if (IsCanBeChoose)
        {
            Choose();
        }
    }

    public void Choose()
    {
        PlantSystem _system = PlantSystem.Instance;

        if (_system != null && _system.IsReadyToPlace()) {
            _system.SelectExistsUnit(gameObject);
        }
        else if (_system != null && !_system.IsDeleteMode)
        {
            _system.SelectExistsUnit(gameObject);
        }
    }
}