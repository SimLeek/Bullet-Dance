using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    public Vector3 respawn_point = Vector3.zero;
    public string respawn_button = "Respawn";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(respawn_button))
        {
            transform.position = respawn_point;
        }
    }

}
