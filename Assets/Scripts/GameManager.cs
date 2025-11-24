using GameAnalyticsSDK;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        GameAnalytics.Initialize();
    }

    public void CityChange(string cityName)
    {
        GameAnalytics.NewDesignEvent("City/Change/" + cityName);
        Debug.Log("Logged city change: " + cityName);
    }

    public void ImageLoaded(string imageName)
    {
        GameAnalytics.NewDesignEvent("Image/Loaded/" +  imageName);
        Debug.Log("Logged image loaded: " + imageName);
    }
}
