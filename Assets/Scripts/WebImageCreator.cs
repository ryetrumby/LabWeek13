using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebImageManager : MonoBehaviour
{
    private Texture2D cache1;
    private Texture2D cache2;
    private Texture2D cache3;

    [Header("Image URLs")]
    [SerializeField] private string img1 = "https://thehowler.org/wp-content/uploads/2018/01/roll-safe-meme-1.jpg";
    [SerializeField] private string img2 = "https://i1.sndcdn.com/artworks-JHaetN7jbSBk3txE-6RuPnQ-t1080x1080.jpg";
    [SerializeField] private string img3 = "https://pbs.twimg.com/media/GuK0lO7XoAEGtzf.jpg";

    

    public IEnumerator DownloadImg(string url, Action<Texture2D> callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError("Image download error: " + request.error);
            callback(null);
            yield break;
        }

        Texture2D tex = DownloadHandlerTexture.GetContent(request);
        callback(tex);
    }
    public IEnumerator GetImg1(Action<Texture2D> callback)
    {
        if (cache1 != null)
        {
            callback(cache1);
            yield break;
        }

        yield return DownloadImg(img1, tex =>
        {
            cache1 = tex;
            callback(tex);
        });
    }

    public IEnumerator GetImg2(Action<Texture2D> callback)
    {
        if (cache2 != null)
        {
            callback(cache2);
            yield break;
        }

        yield return DownloadImg(img2, tex =>
        {
            cache2 = tex;
            callback(tex);
        });
    }

    public IEnumerator GetImg3(Action<Texture2D> callback)
    {
        if (cache3 != null)
        {
            callback(cache3);
            yield break;
        }

        yield return DownloadImg(img3, tex =>
        {
            cache3 = tex;
            callback(tex);
        });
    }
}
