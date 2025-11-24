using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using GameAnalyticsSDK;

public class WeatherManager : MonoBehaviour
{
    [Header("Skybox Info")]
    public Material skyboxSunny;
    public Material skyboxCloudy;
    public Material skyboxRainy;
    public Material skyboxSnowy;
    public Material skyboxNight;

    [Header("Sun Settings")]
    public Light sunlight;

    [Header("City Selection")]
    public string[] cities = new string[]
    {
        "Anchorage,US",
        "Little Rock,US",
        "Bismarck,US",
        "Salt Lake City,US",
        "Orlando,US"
    };

    public TMP_Dropdown cityDropdown;

    private string apiKey = "8f018a847f6fb5d4bb886449be23effa";

    private string apiUrl = "https://api.openweathermap.org/data/2.5/weather?q={0}&appid={1}&units=metric";

    private void Start()
    {
        cityDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    public IEnumerator GetWeatherJSON(string city, Action<string> callback)
    {
        string url = string.Format(apiUrl, city, apiKey);

        Debug.Log("Requesting URL: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"Network problem: {request.error}");
            }
            else if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Response error: {request.responseCode}");
            }
            else
            {
                callback(request.downloadHandler.text);
            }
        }
    }

    private void ApplySkybox(WeatherResponse weather)
    {
        string condition = weather.weather[0].main;

        if (IsNight(weather))
        {
            RenderSettings.skybox = skyboxNight;
            return;
        }

        switch (condition)
        {
            case "Clear":
                RenderSettings.skybox = skyboxSunny;
                break;

            case "Clouds":
                RenderSettings.skybox = skyboxCloudy;
                break;

            case "Rain":
                RenderSettings.skybox = skyboxRainy;
                break;

            case "Snow":
                RenderSettings.skybox = skyboxSnowy;
                break;

            default:
                RenderSettings.skybox = skyboxSunny;
                break;
        }
    }

    private void UpdateSun(WeatherResponse weather)
    {
        bool isNight = IsNight(weather);

        if (isNight)
        {
            sunlight.intensity = 0.05f;
            sunlight.color = new Color(0.1f, 0.1f, 0.3f);
            return;
        }

        string condition = weather.weather[0].main;

        switch (condition)
        {
            case "Clear":
                sunlight.intensity = 1.2f;
                sunlight.color = Color.white;
                break;

            case "Clouds":
                sunlight.intensity = 0.6f;
                sunlight.color = new Color(0.9f, 0.95f, 1f);
                break;

            case "Rain":
                sunlight.intensity = 0.25f;
                sunlight.color = new Color(0.75f, 0.8f, 0.9f);
                break;

            case "Snow":
                sunlight.intensity = 0.8f;
                sunlight.color = new Color(0.95f, 0.95f, 1f);
                break;

            default:
                sunlight.intensity = 1.2f;
                sunlight.color = Color.white;
                break;
        }
    }

    private bool IsNight(WeatherResponse weather)
    {
        long nowUTC = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        long cityTime = nowUTC + weather.timezone;

        return cityTime < weather.sys.sunrise || cityTime > weather.sys.sunset;
    }

    public void CityWeather(int index)
    {
        if (index < 0 || index >= cities.Length)
        {
            Debug.LogError("Invalid city index");
            return;
        }

        string city = cities[index];
        Debug.Log($"CityWeather called with index: {index}, city: {city}");
        StartCoroutine(GetWeatherJSON(city, OnWeatherDataReceived));
    }

    public void OnDropdownChanged(int index)
    {
        Debug.Log("Dropdown index selected: " + index);

        string cityName = cities[index];
        GameAnalytics.NewDesignEvent("City/Selected/" + cityName);

        CityWeather(index);
    }

    public void OnWeatherDataReceived(string json)
    {
        WeatherResponse weather = JsonUtility.FromJson<WeatherResponse>(json);

        Debug.Log("JSON received:\n" + json);
        Debug.Log("City: " + weather.name);
        Debug.Log("Temp: " + weather.main.temp);
        Debug.Log("Feels like: " + weather.main.feels_like);

        GameAnalytics.NewDesignEvent("Weather/Loaded/" + weather.name);

        ApplySkybox(weather);
        UpdateSun(weather);
    }
}

[Serializable]
public class WeatherResponse
{
    public WeatherInfo[] weather;
    public MainInfo main;
    public WindInfo wind;
    public CloudInfo clouds;
    public SysInfo sys;
    public string name; //city name
    public int timezone;
}

[Serializable]
public class WeatherInfo
{
    public string main;
    public string description;
}

[Serializable]
public class MainInfo
{
    public float temp;
    public float feels_like;
    public float temp_min;
    public float temp_max;
    public int humidity;
}

[Serializable]
public class WindInfo
{
    public float speed;
    public float deg;
}

[Serializable]
public class CloudInfo
{
    public int all;
}

[Serializable]
public class SysInfo
{
    public string country;
    public long sunrise;
    public long sunset;
}
