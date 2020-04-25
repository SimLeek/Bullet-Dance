using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;


public class ChangeScene : MonoBehaviour
{
    [Tooltip("These scenes will not be destroyed when switching levels.")]
    public List<string> menuScenes;
    public Camera menuCam;

    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    private void Awake()
    {
        int numScenes = SceneManager.sceneCount;
        if (numScenes > 1)
        {
            menuCam.enabled = false;
        }
        else
        {
            menuCam.enabled = true;
        }
    }

    public void Change(string targetSceneName)
    {
        var loaded = SceneManager.LoadSceneAsync(targetSceneName,LoadSceneMode.Single);
        loaded.completed += delegate (AsyncOperation a)
        {
            int numScenes = SceneManager.sceneCount;

            for (int i = 0; i < numScenes; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.name != targetSceneName)
                {
                    SceneManager.UnloadSceneAsync(s);
                }
            }
        };
    }

    public void ChangeButKeepCurrentInBackground(string targetSceneName)
    {
        Scene s = SceneManager.GetActiveScene();
        GameObject[] gameObjects = s.GetRootGameObjects();
        foreach (GameObject g in gameObjects)
        {
            if (g.name == "Main Camera")
            {
                g.GetComponent<Camera>().enabled = true;
                var cam = g.GetComponent<FastCam>();
                cam.StopAllCoroutines();
                Destroy(cam);
            }
        }

        
        
        int xPos = 0, yPos = 0;
        SetCursorPos(xPos, yPos);//Call this when you want to set the mouse position

        var loadAsync = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);

        loadAsync.completed += delegate (AsyncOperation a)
        {
            Scene sc2 = SceneManager.GetSceneByName(targetSceneName);
            SceneManager.SetActiveScene(sc2);
        };


        
    }

    public void PreviewLevel(string targetSceneName)
    {
        if (SceneManager.sceneCount > 1)
        {
            int numScenes = SceneManager.sceneCount;
            
            for(int i = 0; i<numScenes;i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.name == targetSceneName)
                {
                    menuCam.enabled = false;
                    return;
                }
                if (menuScenes.IndexOf(s.name) < 0 && s.name != targetSceneName)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
                    menuCam.enabled = true;
                }
                
            }
            
        }
        var load = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        load.completed += delegate (AsyncOperation a) {
            Scene s = SceneManager.GetSceneByName(targetSceneName);
            GameObject[] gameObjects = s.GetRootGameObjects();
            foreach(GameObject g in gameObjects)
            {
                if(g.name=="Main Camera")
                {
                    g.GetComponent<Camera>().enabled = true;
                    menuCam.enabled = false;
                    var cam = g.GetComponent<FastCam>();
                    Destroy(cam);
                }
            }
            int numScenes = SceneManager.sceneCount;
            for (int i = 0; i < numScenes; i++)
            {
                var sc = SceneManager.GetSceneAt(i);
                if (menuScenes.IndexOf(sc.name) < 0 && sc.name!= targetSceneName)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
                }
            }
        };
    }

    public void EndPreview(string targetSeceneName)
    {
        //SceneManager.UnloadSceneAsync(targetSeceneName);
    }
}
