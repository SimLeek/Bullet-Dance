using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RotateSkyBox : MonoBehaviour
{
    public Material sky_material;

    public float rps;
    
    public bool periodic_exposure;
    public float eps;
    public Vector2 exposure_min_max = new Vector2(1,8);


    private void Start()
    {
        RenderSettings.skybox = sky_material;
    }

    // Update is called once per frame
    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rps);
        if (periodic_exposure)
        {
            RenderSettings.skybox.SetFloat("_Exposure", exposure_min_max.x + (float)(Math.Sin(Time.time * eps)+1.0)/2.0f * (exposure_min_max.y- exposure_min_max.x));
        }
        //GetComponent<Skybox>().transform.Rotate(axis, rps);
    }
}
