using UnityEngine;

public class ArtAppreciationArt : MonoBehaviour
{
    public Texture2D[] Art;
    public Material PostProcessMaterial;
    public AudioClip Audio;

#if UNITY_EDITOR
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, PostProcessMaterial);
    }
#endif
}