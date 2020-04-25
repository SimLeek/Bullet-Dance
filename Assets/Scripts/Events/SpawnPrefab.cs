using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    public void SpawnAtSpawnerTransform(GameObject toSpawn)
    {
        var obj = Instantiate(toSpawn,transform.position, transform.rotation);
        obj.transform.localScale *= 10;
    }
}
