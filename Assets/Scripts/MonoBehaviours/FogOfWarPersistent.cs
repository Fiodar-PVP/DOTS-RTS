using UnityEngine;

public class FogOfWarPersistent : MonoBehaviour
{
    [SerializeField] private RenderTexture fogOfWarRenderTexture;
    [SerializeField] private RenderTexture fogOfWarPersistentRenderTexture;
    [SerializeField] private RenderTexture fogOfWarCacheRenderTexture;
    [SerializeField] private Material multiplyMaterial;

    private void Start()
    {
        Graphics.Blit(fogOfWarRenderTexture, fogOfWarPersistentRenderTexture);
        Graphics.Blit(fogOfWarRenderTexture, fogOfWarCacheRenderTexture);
    }

    private void Update()
    {
        Graphics.Blit(fogOfWarRenderTexture, fogOfWarPersistentRenderTexture, multiplyMaterial, 0);
        Graphics.CopyTexture(fogOfWarPersistentRenderTexture, fogOfWarCacheRenderTexture);
    }
}
