using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class Level
    {
        public string levelName;
        public string stars;
        public bool unlocked;
        public bool shown;

        public Button.ButtonClickedEvent onClick;
        public UnityEvent onMouseOver;
        public UnityEvent onMouseExit;
    }
    public GameObject button;
    public Transform space;
    public List<Level> levelList;
    

    void FillList()
    {
        foreach(var l in levelList)
        {
            if (l.shown)
            {
                GameObject buttonObj = Instantiate(button) as GameObject;

                LevelButton buttonCls = buttonObj.GetComponent<LevelButton>();
                buttonCls.levelName = l.levelName;
                buttonCls.unlocked = l.unlocked;
                buttonCls.stars = l.stars;
                buttonCls.onClick = l.onClick;
                buttonCls.onMouseOver = l.onMouseOver;
                buttonCls.onMouseExit = l.onMouseExit;

                buttonObj.transform.SetParent(space);

                buttonCls.updateinfo();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        FillList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
