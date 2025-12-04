using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeatherUIController : MonoBehaviour
{
    [SerializeField] private WeatherManager wManager;
    [SerializeField] private TMP_Dropdown cDropdown;

    private void Start()
    {
        if (wManager == null)
        {
            wManager = FindObjectOfType<WeatherManager>();
        }

        if (cDropdown != null)
        {
            cDropdown.onValueChanged.AddListener(OnCityChanged);
        }
    }

    private void OnCityChanged(int index)
    {
        if (wManager != null)
        {
            wManager.SetCityByIndex(index);
        }
    }
}
