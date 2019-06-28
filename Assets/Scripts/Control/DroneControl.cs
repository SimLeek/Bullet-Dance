using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DroneControl : MonoBehaviour
{
    public bool draw_gizmos = true;

    public float placer_radius_min = 1.0f;
    public float placer_radius_max=20f;
    public float placer_radius_increment_size=0.5f;
    public float placer_degrees = 200;
    public Vector3 placer_displacement=Vector3.zero;

    public GameObject placer_cursor;
    private GameObject placer_cursor_instance;
    private bool is_selected;

    private GameObject hemisphereobj;
    private MeshCollider hemisphere;
    public int hemisphere_lat = 24;
    public int hemisphere_lon = 24;

    void DeleteCursor()
    {
        if (placer_cursor_instance != null)
        {
            DestroyImmediate(placer_cursor_instance);
        }
    }

    void DeleteHemisphere()
    {
        if (hemisphereobj != null)
        {
            DestroyImmediate(hemisphereobj);
        }
    }

    void CreateCursor(Vector3 location, Quaternion rotation)
    {
        if (placer_cursor != null && placer_cursor_instance == null)
        {
            placer_cursor_instance = Instantiate(placer_cursor, location, rotation);
            placer_cursor_instance.transform.parent = transform;
        }
    }

    void CreateSphere(Vector3 location, Quaternion rotation, float radius, float degrees)
    {
        if (hemisphere == null)
        {
            if (hemisphereobj == null)
            {
                hemisphereobj = new GameObject();
                hemisphereobj.transform.parent = transform;
            }
            hemisphere = hemisphereobj.AddComponent<MeshCollider>();

            hemisphere.sharedMesh = new Mesh();
            hemisphere.convex = false;
            
            //hemisphere.sharedMesh = HemiSphere.Create(1.0f, hemisphere_lat, hemisphere_lon, degrees, degrees);
            
        }
        if (hemisphereobj != null)
        {
            hemisphereobj.transform.localRotation = rotation;
            hemisphereobj.transform.localPosition = location;
            hemisphereobj.transform.localScale = Vector3.one * radius;
        }
    }

    void OnDrawGizmos()
    {
        // Draw always-visible gizmos here

        if (is_selected)
        {
            is_selected = false; // Will change back to true if still selected
            return;
        }
        DeleteCursor();
        DeleteHemisphere();
    }

    private void OnDrawGizmosSelected()
    {
        if (draw_gizmos)
        {
            Gizmos.color = new Color(0,0,.5f);
            Gizmos.DrawWireSphere(transform.position+placer_displacement, placer_radius_min);

            Gizmos.color = new Color(.5f, .5f, 1);
            Gizmos.DrawWireSphere(transform.position + placer_displacement, placer_radius_max);
            
            Vector3 left_vec = (Quaternion.AngleAxis(-placer_degrees/2f, Vector3.up)*transform.rotation) * Vector3.forward;
            Vector3 right_vec = (Quaternion.AngleAxis(placer_degrees / 2f, Vector3.up) * transform.rotation) * Vector3.forward;

            /*Gizmos.color = new Color(1, 0, 1);
            Gizmos.DrawRay(
                transform.position + placer_displacement + left_vec * placer_radius_min,
                left_vec * (placer_radius_max-placer_radius_min)
                );
            Gizmos.DrawRay(
                transform.position + placer_displacement + right_vec * placer_radius_min,
                right_vec * (placer_radius_max - placer_radius_min)
                );*/
            //var p_deg_180 = Mathf.Min(placer_degrees, 180);
            Quaternion rot;
            if (placer_degrees <= 180)
            {
                rot = Quaternion.Euler(0, -((180 - placer_degrees) / 180) * placer_degrees, 0);
            }
            else
            {
                rot = Quaternion.Euler(0, -((180 - placer_degrees) / 180)/4 * placer_degrees, 0);
            } 
            //var rot = Quaternion.identity;
            CreateSphere(transform.position + placer_displacement, transform.rotation*rot, placer_radius_min + placer_radius_increment_size, placer_degrees);
            Gizmos.DrawWireMesh(hemisphere.sharedMesh, hemisphereobj.transform.localPosition, hemisphereobj.transform.localRotation, hemisphereobj.transform.localScale);

            CreateCursor(transform.position + Vector3.up + Vector3.right + Vector3.forward, Quaternion.identity);
        }
        else
        {
            DeleteCursor();
            DeleteHemisphere();
        }
    }
}
