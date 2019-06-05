using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralNoiseProject;
using MarchingCubesProject;
using static Proceedural3d;
using System;

public class CaveGen : MonoBehaviour
{

    public MARCHING_MODE mode = MARCHING_MODE.CUBES;
    public Material cave_material;
    public string seed = "";
    List<GameObject> meshes = new List<GameObject>();
    public float surface_level = 0.0f;
    public int width = 32;
    public int height = 32;
    public int length = 32;

    public float time_limit = 1.0f;

    private INoise perlin_noise_func;
    private FractalNoise fractal_noise_func;
    private float[] voxels;
    private Marching marching;
    public bool generate=true;
    private List<Vector3> verts;

    public void Init()
    {
        DeleteGenerated();

        perlin_noise_func = new PerlinNoise(GetSeed(), 2.0f);
        fractal_noise_func = new FractalNoise(perlin_noise_func, 3, 2.0f);

        if (mode == MARCHING_MODE.TETRAHEDRON)
            marching = new MarchingTertrahedron();
        else
            marching = new MarchingCubes();

        marching.Surface = surface_level;

        verts = new List<Vector3>();

        generate = false;


    }


    public void OnDrawGizmos()
    {
        if (generate)
        {
            Init();
            voxels = FuncToVoxels(GetMoonBowlFunc(), width, height, length, true);
            marching.Generate(voxels, width, height, length, verts, new List<int>());
            SplitAddMesh(verts);
        }
        
    }

    public int GetSeed()
    {
        if (seed == "")
        {
            seed = Time.time.ToString();
        }
        return seed.GetHashCode();
    }


    public Func<float, float, float, float> GetMoonBowlFunc(float bowl_percentage=.5f)
    {
        if (perlin_noise_func != null && fractal_noise_func!=null)
        {
            var sphere_func = BowlGen(bowl_percentage);
            ;
            return new Func<float, float, float, float>((float fx, float fy, float fz) =>
            {
                float sph = sphere_func(fx, fy, fz);
                return fractal_noise_func.Sample3D(fx, fy, fz) + sph * 2.0f;
            });
        }
        return GetPlaneFunc();
    }

    public Func<float, float, float, float> GetPlaneFunc()
    {
        return (float a, float b, float c) => { return 0; };
    }

    public float[] FuncToVoxels(Func<float, float, float, float> func_3d, int width , int height, int length, bool obey_time_limit=false)
    {
        float[] voxels = new float[width * height * length];
        float t_start = Time.time;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    if(Time.time - t_start > time_limit && obey_time_limit)
                    {
                        return new float[0];
                    }
                    float fx = x / (width - 1.0f);
                    float fy = y / (height - 1.0f);
                    float fz = z / (length - 1.0f);

                    int idx = x + y * width + z * width * height;
                    
                    voxels[idx] = func_3d(fx,fy,fz);
                }
            }
        }

        return voxels;
    }

    public void DeleteGenerated()
    {
        while (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
    }

    public void SplitAddMesh(List<Vector3> verts)
    {
        

        //A mesh in unity can only be made up of 65000 verts.
        //Need to split the verts between multiple meshes.

        int maxVertsPerMesh = 60000; //must be divisible by 3, ie 3 verts == 1 triangle
        int numMeshes = verts.Count / maxVertsPerMesh + 1;

        for (int i = 0; i < numMeshes; i++)
        {

            List<Vector3> splitVerts = new List<Vector3>();
            List<int> splitIndices = new List<int>();

            for (int j = 0; j < maxVertsPerMesh; j++)
            {
                int idx = i * maxVertsPerMesh + j;

                if (idx < verts.Count)
                {
                    splitVerts.Add(verts[idx]);
                    splitIndices.Add(j);
                }
            }

            if (splitVerts.Count == 0) continue;

            Mesh mesh = new Mesh();
            mesh.SetVertices(splitVerts);
            mesh.SetTriangles(splitIndices, 0);
            mesh.RecalculateBounds();
            NormalSolver.RecalculateNormals(mesh, 60);
            mesh.RecalculateTangents();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshCollider>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = cave_material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.GetComponent<MeshCollider>().sharedMesh = mesh;
            go.GetComponent<MeshCollider>().convex = false;
            var pos = GetComponent<Transform>().position;
            var scale = GetComponent<Transform>().localScale;
            go.transform.localPosition = new Vector3(pos.x - width * scale.x / 2f, pos.y - height * scale.y / 2f, pos.z - length * scale.z / 2f);
            go.transform.localScale = scale;


            meshes.Add(go);
        }
    }

    void Start()
    {
        DeleteGenerated();
        Init();
        float[] voxels = FuncToVoxels(GetMoonBowlFunc(), width, height, length);
        marching.Generate(voxels, width, height, length, verts, new List<int>());
        SplitAddMesh(verts);
    }

}
