using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PixelateEffect : MonoBehaviour
{
    public PostProcessVolume postProcessVolume;
    public Material pixelateMaterial;

    public float pixelSize = 8.0f;
    public Shader pixelateShader;
    
    private void Start()
    {
        //Shader pixelateShader = Shader.Find("Hidden/PixelateEffect");
        if (pixelateShader != null)
        {
            pixelateMaterial = new Material(pixelateShader);
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (pixelateMaterial != null)
        {
            pixelateMaterial.SetFloat("_PixelSize", pixelSize);
            Graphics.Blit(src, dest, pixelateMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}