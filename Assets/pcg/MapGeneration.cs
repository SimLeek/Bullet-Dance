using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{
    public int width;
    public int height;

    public string seed = "";
    public bool isNatural = true;

    [Range(0,100)]
    public int randomFillPercent;

    int[,] map;
    System.Random pseudoRandom;

    void Start()
    {
        if (seed == "")
        {
            seed = Time.time.ToString();
        }

        pseudoRandom = new System.Random(seed.GetHashCode());

        GenerateMap();
        
    }

    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
        for(int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

    }

    void RandomFillMap()
    {
        

        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 100;
                }
                else
                {
                    map[x, y] = pseudoRandom.Next(0, 100);
                }
            }
        }
    }

    void SmoothMap()
    {
        int fillPoint = randomFillPercent * 4;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighborWallTiles = GetSurroundingWallCount(x, y);
                

                if (neighborWallTiles > fillPoint)
                {
                    map[x, y] = pseudoRandom.Next(0, 100);
                }else if (neighborWallTiles < fillPoint && isNatural || neighborWallTiles<= fillPoint && !isNatural)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (neighborX != gridX || neighborY != gridY)
                    {
                        wallCount += map[neighborX, neighborY];
                    }
                }
                else
                {
                    wallCount+=100;
                }
                
            }
        }
        return wallCount;
    }

    private void OnDrawGizmos()
    {
        /*if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] > 0) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
