using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DoubleTapDetect
{
    public double doubleTapCloseness = 0.2f;
    public double doubleTapTime = 0.2f;
    public double doubleTapTimeEnd = 0.08f;
    public bool doubleTapHold = false;

    private double direction = -999f;
    private double prevMagnitude = 0f;
    private double prevMagnitude2 = 0f;
    private double timeDiff = 0f;

    public bool DetectDirectional(InputTransform inp)
    {
        if (doubleTapHold)
        {
            //Debug.Log($"Double tap hold:");
            if (Math.Abs(inp.magnitude) < doubleTapCloseness) // back to 0
            {
                if (Time.time - timeDiff > doubleTapTimeEnd) // allow user to switch directions
                {
                    //Debug.Log($"Double tap hold: end");
                    doubleTapHold = false;
                    return false;
                }
            }
            else
            {
                timeDiff = Time.time;
            }

            //Debug.Log($"Double tap hold: yay");
            return true;
        }
        else
        {
            if (Math.Abs(prevMagnitude2) < doubleTapCloseness && Math.Abs(prevMagnitude) < doubleTapCloseness && Math.Abs(inp.magnitude - 1.0) < doubleTapCloseness) // 0 to 100
            {
                direction = inp.direction;
                prevMagnitude = 1.0;
                timeDiff = Time.time;
                //Debug.Log($"Double tap 1: {prevMagnitude}, {timeDiff}");
            }
            else if (Math.Abs(prevMagnitude - 1) < doubleTapCloseness && Math.Abs(inp.magnitude) < doubleTapCloseness && Time.time - timeDiff < doubleTapTime)  // 100 to 0
            {
                prevMagnitude2 = prevMagnitude;
                prevMagnitude = 0;
                //Debug.Log($"Double tap 2: {prevMagnitude2}, {prevMagnitude}");
            }
            else if (Math.Abs(prevMagnitude2 - 1) < doubleTapCloseness && Math.Abs(prevMagnitude) < doubleTapCloseness && Math.Abs(inp.magnitude - 1.0) < doubleTapCloseness && Time.time - timeDiff < doubleTapTime && Math.Abs(inp.direction - direction) < doubleTapCloseness) // 0 to 100 again
            {
                prevMagnitude = 0;
                prevMagnitude2 = 0;
                direction = -999f;
                timeDiff = Time.time;
                //Debug.Log($"Double tap 3: {prevMagnitude}, {prevMagnitude}, {timeDiff}");
                doubleTapHold = true;
                return true;
            }
            else if (Time.time - timeDiff > doubleTapTime)
            {
                prevMagnitude = 0;
                prevMagnitude2 = 0;
                direction = -999f;
                timeDiff = Time.time;
                //Debug.Log($"Double tap 4: {prevMagnitude}, {prevMagnitude}, {timeDiff}");
            }
        }

        return false;
    }

}