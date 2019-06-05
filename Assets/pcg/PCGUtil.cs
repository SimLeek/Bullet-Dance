using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class PCGUtil
{
    public static Vector3Int[] FaceDirectionsArrayI = 
        {
        new Vector3Int(1,0,0),
        new Vector3Int(0,1,0),
        new Vector3Int(0,0,1),
        new Vector3Int(-1,0,0),
        new Vector3Int(0,-1,0),
        new Vector3Int(0,0,-1),
        };

    public static void Populate<T>(this T[,,] arr, T value)
    {
        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++)
            {
                for (int k = 0; k < arr.GetLength(2); k++)
                {
                    arr[i, j, k] = value;
                }
            }
        }
    }

    public class DenseGrid<T>
    {
        public T[,,] vals;

        public DenseGrid(int x, int y, int z, T init)
        {
            vals = new T[x, y, z];
            PCGUtil.Populate<T>(vals, init);
        }

        public T this[Vector3Int xyz]
        {
            get { return vals[xyz.x, xyz.y, xyz.z]; }
            set { vals[xyz.x, xyz.y, xyz.z] = value; }
        }

        public void assign(T val)
        {
            PCGUtil.Populate<T>(vals, val);
        }
    }

    public class SparseGrid<T>
    {
        public Dictionary<Vector3Int, T> vals;

        public T this[Vector3Int xyz]
        {
            get { return vals[xyz]; }
            set { vals[xyz] = value; }
        }
    }


    public class AxisAlignedBoxInt
    {
        public Vector3Int Min, Max;

        public float Width { get { return Max.x - Min.x; } }
        public float Height { get { return Max.y - Min.y; } }
        public float Depth { get { return Max.z - Min.z; } }

        public AxisAlignedBoxInt(Vector3Int _start, Vector3Int _end)
        {
            if (_start.x <= _end.x && _start.y <= _end.y && _start.z <= _end.z)
            {
                Min = _start;
                Max = _end;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException("min must be less than max in all dimensions");
            }
                

        }

        public bool Contains(Vector3Int val)
        {
            if (val.x>=Min.x&& val.y >= Min.y && val.z >= Min.z &&
                val.x <= Max.x && val.y <= Max.y && val.z <= Max.z)
            {
                return true;
            }
            return false;
        }
    }

    public class AxisAlignedBox
    {
        public Vector3 Min, Max;

        public float Width { get { return Max.x - Min.x; } }
        public float Height { get { return Max.y - Min.y; } }
        public float Depth { get { return Max.z - Min.z; } }

        public AxisAlignedBox(Vector3 _start, Vector3 _end)
        {
            if (_start.x <= _end.x && _start.y <= _end.y && _start.z <= _end.z)
            {
                Min = _start;
                Max = _end;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException("min must be less than max in all dimensions");
            }

        }

        public bool Contains(Vector3 val)
        {
            if (val.x >= Min.x && val.y >= Min.y && val.z >= Min.z &&
                val.x <= Max.x && val.y <= Max.y && val.z <= Max.z)
            {
                return true;
            }
            return false;
        }
    }


}
