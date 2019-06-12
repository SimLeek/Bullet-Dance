using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HemiSphere
{


    public static Mesh Create(float radius=1f, int nbLong=24, int nbLat=16, float lat_degrees=180, float lon_degrees = 180)
    {
        //https://wiki.unity3d.com/index.php/ProceduralPrimitives#C.23_-_Sphere
        var mesh = new Mesh();

        // Longitude |||
        // Latitude ---

        #region Vertices
        int hemi_nblat = Mathf.FloorToInt(nbLat * (Mathf.Min(lat_degrees,180f) / 180f));
        int hemi_nblon = Mathf.FloorToInt(nbLong * (Mathf.Min(lon_degrees, 360f) / 360f));

        Vector3[] vertices = new Vector3[(hemi_nblon + 1) * hemi_nblat + 1];
        
        float _pi = Mathf.PI;
        float _2pi = _pi * 2f;

        for (int lat = 0; lat < hemi_nblat; lat++)
        {
            float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= hemi_nblon; lon++)
            {
                float a2 = _2pi * (float) lon / nbLong;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                vertices[lon + lat * (hemi_nblon + 1)] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }
        vertices[vertices.Length - 1] = vertices[vertices.Length - 2];

        #endregion

        #region Normales		
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++)
            normales[n] = vertices[n].normalized;
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = Vector2.up;
        uvs[uvs.Length - 1] = Vector2.zero;
        for (int lat = 0; lat < hemi_nblat; lat++)
            for (int lon = 0; lon <= hemi_nblon; lon++)
                uvs[lon + lat * (hemi_nblon + 1)] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
        #endregion

        #region Triangles
        int nbFaces = vertices.Length-2;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        //Top Cap
        int i = 0;
        for (int lon = 0; lon < hemi_nblon; lon++)
        {
            triangles[i++] = lon + 2;
            triangles[i++] = lon + 1;
            triangles[i++] = 0;
        }

        //Middle
        for (int lat = 0; lat < hemi_nblat - 1; lat++)
        {
            for (int lon = 1; lon < hemi_nblon; lon++)
            {
                int current = lon-1 + lat * (hemi_nblon + 1) + 1;
                int next = current + hemi_nblon + 1;

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }
        }

        if (lat_degrees >= 180)
        {
            //Bottom Cap
            for (int lon = 0; lon < hemi_nblon; lon++)
            {
                triangles[i++] = vertices.Length - 1;
                triangles[i++] = vertices.Length - (lon + 2) - 1;
                triangles[i++] = vertices.Length - (lon + 1) - 1;
            }
        }
        
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        mesh.Optimize();

        return mesh;
    }
}