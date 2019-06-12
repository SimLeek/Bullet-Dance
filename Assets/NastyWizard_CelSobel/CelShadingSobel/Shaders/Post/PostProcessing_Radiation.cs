using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessing_Radiation : MonoBehaviour
{

    private Material sobelMat;

    [Range(0, 3)]
    public float RadiationLevel = 1;


    void Start()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
        sobelMat = new Material(Shader.Find("RadShads/RadiationShader"));
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        sobelMat.SetFloat("_ResX", Screen.width * 1);
        sobelMat.SetFloat("_ResY", Screen.height * 1);
        sobelMat.SetFloat("_RadLevel", RadiationLevel);
        Graphics.Blit(source, destination, sobelMat);
    }
}
