using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputTransform
{
    public double h;
    public double v;
    public double r;
    public double direction;
    public double magnitude;

    public InputTransform()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        r = Input.GetAxis("Rotate");

        direction = Math.Atan2(v, h);
        magnitude = Math.Max(Math.Abs(v), Math.Abs(h));

    }
}