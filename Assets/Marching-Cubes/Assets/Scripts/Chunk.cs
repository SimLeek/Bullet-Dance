using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Chunk : MonoBehaviour {
    public Vector3Int coord;

    [HideInInspector]
    public Mesh mesh;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    bool generateCollider;

    [HideInInspector]
    public AsyncGPUReadbackRequest? asyncOp0;
    [HideInInspector]
    public AsyncGPUReadbackRequest? asyncOp1;
    [HideInInspector]
    public AsyncGPUReadbackRequest? asyncOp2;
    [HideInInspector]
    public AsyncGPUReadbackRequest? asyncOp3;


    public void DestroyOrDisable () {
        if (Application.isPlaying) {
            mesh.Clear ();
            gameObject.SetActive (false);
        } else {
            DestroyImmediate (gameObject, false);
        }
    }

    public void UpdateColliders()
    {
        if (generateCollider)
        {
            // force update
            meshCollider.enabled = false;
            meshCollider.enabled = true;
        }
    }

    // Add components/get references in case lost (references can be lost when working in the editor)
    public void SetUp (Material mat, bool generateCollider) {
        this.generateCollider = generateCollider;

        meshFilter = GetComponent<MeshFilter> ();
        meshRenderer = GetComponent<MeshRenderer> ();
        meshCollider = GetComponent<MeshCollider> ();

        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        if (meshRenderer == null) {
            meshRenderer = gameObject.AddComponent<MeshRenderer> ();
        }

        if (meshCollider == null && generateCollider) {
            meshCollider = gameObject.AddComponent<MeshCollider> ();
            meshCollider.cookingOptions = MeshColliderCookingOptions.None;
        }
        if (meshCollider != null && !generateCollider) {
            DestroyImmediate (meshCollider);
        }

        mesh = meshFilter.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh ();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshFilter.sharedMesh = mesh;
        }

        if (generateCollider) {
            if (meshCollider.sharedMesh == null) {
                meshCollider.sharedMesh = mesh;
            }
            // force update
            //meshCollider.enabled = false;
            //meshCollider.enabled = true;
        }

        

        meshRenderer.material = mat;
    }
}