using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BillboardImageLoader : MonoBehaviour
{
    [SerializeField] private WebImageManager wImgManager;
    [SerializeField] private int imgIndex = 1;

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void Start()
    {
        if (wImgManager == null)
        {
            wImgManager = FindObjectOfType<WebImageManager>();
        }

        if (wImgManager == null)
        {
            Debug.LogError("No WebImageManager found in scene");
            return;
        }

        switch (imgIndex)
        {
            case 0:
                StartCoroutine(wImgManager.GetImg1(OnImageLoaded));
                break;
            case 1:
                StartCoroutine(wImgManager.GetImg2(OnImageLoaded));
                break;
            case 2:
                StartCoroutine(wImgManager.GetImg3(OnImageLoaded));
                break;
            default:
                StartCoroutine(wImgManager.GetImg1(OnImageLoaded));
                break;
        }
    }

    private void OnImageLoaded(Texture2D tex)
    {
        if (tex == null)
        {
            return;
        }

        Material mat = rend.material;
        mat.mainTexture = tex;

        // flip textures vertically (loaded upside down womp womp)
        mat.mainTextureScale = new Vector2(1f, -1f);
        mat.mainTextureOffset = new Vector2(0f, 1f);
    }
}
