using System;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK;
using UnityEngine;
using UnityEngine.Networking;

public class ImageManager : MonoBehaviour
{
    [Header("Billboard URLs")]
    public GameObject[] billboards;
    public string[] imageUrls;

    private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();

    private void Start()
    {
        for (int i = 0; i < billboards.Length; i++)
        {
            int index = i;
            StartCoroutine(DownloadImage(imageUrls[index], billboards[index]));
        }
    }

    public IEnumerator GetWebImage(string url, Action<Texture2D> callback)
    {
        if (imageCache.ContainsKey(url))
        {
            callback(imageCache[url]);
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download image: {request.error}");
                callback(null);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                imageCache[url] = texture;
                callback(texture);
            }
        }
    }

    private IEnumerator DownloadImage(string url, GameObject billboard)
    {
        yield return StartCoroutine(GetWebImage(url, (texture) => ApplyImage(billboard, texture)));
    }

    public void ApplyImage(GameObject billboard, Texture2D texture)
    {
        if (texture == null) return;

        Renderer renderer = billboard.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.mainTexture = texture;
        }

        GameAnalytics.NewDesignEvent("Image/Applied/" + billboard.name);
    }
}
