using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class WeatherMain
{
    public float tmp;
}

[Serializable]
public class WeatherDescription
{
    public string main;
    public string description;
}

[Serializable]
public class WeatherResponse
{
    public WeatherMain main;
    public WeatherDescription[] weather;
    public int timezone;
}

public class WeatherManager : MonoBehaviour
{
    [SerializeField] private string apiKey = "010c094e88d2608e19352aa422671f18";

    [Tooltip("City names with country code")]
    [SerializeField]
    private string[] cities =
    {
        "Orlando,US",
        "London,GB",
        "Tokyo,JP",
        "Sydney,AU",
        "Rio De Janeiro,BR"
    };

    [Header("Scene References")]
    [SerializeField] private Light sunLight;

    [Header("Skybox Materials (Fantasy Skybox FREE)")]
    [SerializeField] private Material skyboxDay;
    [SerializeField] private Material skyboxNight;
    [SerializeField] private Material skyboxSunny;
    [SerializeField] private Material skyboxRainy;
    [SerializeField] private Material skyboxSnowy;
    [SerializeField] private Material skyboxSunrise;
    [SerializeField] private Material skyboxSunset;
    [SerializeField] private Material skyboxCloudy;

    [Header("Sun Intensity Settings")]
    [SerializeField] private float dayIntensity = 1.2f;
    [SerializeField] private float nightIntensity = 0.1f;
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.5f, 0.6f, 1.0f);

    private int currentCityIndex = 0;

    private const string baseUrl = "https://api.openweathermap.org/data/2.5/weather";

    private void Start()
    {
        if (sunLight == null)
        {
            sunLight = FindObjectOfType<Light>();
        }

        // Start on first city
        RequestWeatherForCurrentCity();
    }

    public void SetCityByIndex(int index)
    {
        if (index < 0 || index >= cities.Length)
        {
            Debug.LogWarning("City index out of range");
            return;
        }

        Debug.Log("Switching city to: " + cities[index] + " (index " + index + ")");
        currentCityIndex = index;
        RequestWeatherForCurrentCity();
    }

    public void NextCity()
    {
        currentCityIndex++;
        if (currentCityIndex >= cities.Length)
        {
            currentCityIndex = 0;
        }
        RequestWeatherForCurrentCity();
    }

    public void PreviousCity()
    {
        currentCityIndex--;
        if (currentCityIndex < 0)
        {
            currentCityIndex = cities.Length - 1;
        }
        RequestWeatherForCurrentCity();
    }

    private void RequestWeatherForCurrentCity()
    {
        string city = cities[currentCityIndex];
        string url = baseUrl + "?q=" + UnityWebRequest.EscapeURL(city) + "&appid=" + apiKey + "&mode=json";

        Debug.Log("Weather URL: " + url);

        StartCoroutine(GetWeatherJSON(url, OnWeatherDataLoaded));
    }

    private IEnumerator GetWeatherJSON(string url, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("Weather request error: " + request.error);
            }
            else
            {
                callback(request.downloadHandler.text);
            }
        }
    }

    private void OnWeatherDataLoaded(string json)
    {
        Debug.Log("Weather JSON: " + json);

        WeatherResponse response = JsonUtility.FromJson<WeatherResponse>(WrapJsonForUnity(json));

        if (response == null)
        {
            Debug.LogError("Failed to parse weather JSON");
            return;
        }

        string condition = response.weather != null && response.weather.Length > 0
            ? response.weather[0].main
            : "Clear";

        float tempKelvin = response.main != null ? response.main.tmp : 293.15f;
        float tempCelsius = tempKelvin - 273.15f;

        DateTime utcNow = DateTime.UtcNow;
        TimeSpan offset = TimeSpan.FromSeconds(response.timezone);
        DateTime localTime = utcNow + offset;

        Debug.Log("Condition: " + condition + " TempC: " + tempCelsius + " LocalTime: " + localTime);

        UpdateSkybox(condition, localTime);
        UpdateSun(tempCelsius, localTime);

    }

    private string WrapJsonForUnity(string rawJson)
    {
        return rawJson;
    }

    private void UpdateSkybox(string condition, DateTime localTime)
    {
        bool isNight = localTime.Hour < 6 || localTime.Hour >= 18;
        Material targetSkybox = skyboxDay;

        string conditionUpper = condition.ToUpperInvariant();

        if (conditionUpper.Contains("RAIN") || conditionUpper.Contains("DRIZZLE") || conditionUpper.Contains("THUNDER"))
        {
            targetSkybox = skyboxRainy != null ? skyboxRainy : skyboxCloudy;
        }
        else if (conditionUpper.Contains("SNOW"))
        {
            targetSkybox = skyboxSnowy != null ? skyboxSnowy : skyboxDay;
        }
        else if (conditionUpper.Contains("CLOUD"))
        {
            targetSkybox = skyboxCloudy != null ? skyboxCloudy : skyboxDay;
        }
        else if (conditionUpper.Contains("CLEAR"))
        {
            targetSkybox = skyboxSunny != null ? skyboxSunny : skyboxDay;
        }

        if (isNight)
        {
            if (localTime.Hour >= 5 && localTime.Hour < 7 && skyboxSunrise != null)
            {
                targetSkybox = skyboxSunrise;
            }
            else if (localTime.Hour >= 18 && localTime.Hour < 20 && skyboxSunset != null)
            {
                targetSkybox = skyboxSunset;
            }
            else if (skyboxNight != null)
            {
                targetSkybox = skyboxNight;
            }
        }

        if (targetSkybox != null)
        {
            RenderSettings.skybox = targetSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }

    private void UpdateSun(float tempCelsius, DateTime localTime)
    {
        if (sunLight == null)
        {
            return;
        }

        bool isNight = localTime.Hour < 6 || localTime.Hour >= 20;

        float targetIntensity = isNight ? nightIntensity : dayIntensity;

        float t = Mathf.InverseLerp(-10f, 40f, tempCelsius);
        Color warmColor = new Color(1.0f, 0.9f, 0.7f);
        Color coldColor = new Color(0.7f, 0.8f, 1.0f);
        Color tempColor = Color.Lerp(coldColor, warmColor, t);

        Color baseColor = isNight ? nightColor : dayColor;
        Color finalColor = Color.Lerp(baseColor, tempColor, 0.5f);

        sunLight.intensity = targetIntensity;
        sunLight.color = finalColor;
    }

    public string GetCurrentCityName()
    {
        if (currentCityIndex < 0 || currentCityIndex >= cities.Length)
        {
            return "Unknown";
        }

        return cities[currentCityIndex];
    }
}