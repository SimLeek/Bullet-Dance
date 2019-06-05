using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

static class Proceedural3d
{
    public static Func<float, float, float, float> SphereGen(float radius)
    {
        return (x, y, z) => (float)Math.Sqrt((x - .5) * (x - .5) + (y - .5) * (y - .5) + (z - .5) * (z - .5)) - radius;
    }

    public static Func<float,float,float,float> SphereGen(float radius, Vector3 center)
    {
        return (x, y, z) => (float)Math.Sqrt((x-.5-center.x) * (x - .5 - center.x) + (y - .5 - center.y) * (y - .5 - center.y) + (z - .5 - center.z) * (z - .5 - center.z)) - radius;
    }

    public static Func<float, float, float, float> BowlGen(float radius)
    {
        float bowl_func(float x, float y, float z)
        {
            double cubeAll = (x - .5) * (x - .5) + (y - .5) * (y > 0 ? -y - .5 : y - .5) + (z - .5) * (z - .5);
            return (float)Math.Sqrt(cubeAll>0?cubeAll:0) - radius;
        }
        return bowl_func;
    }
}
