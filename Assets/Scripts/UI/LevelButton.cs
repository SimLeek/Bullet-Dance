using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LevelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string levelName;
    public string stars;
    public bool unlocked;
    public Button.ButtonClickedEvent onClick;
    public UnityEvent onMouseOver;
    public UnityEvent onMouseExit;


    // Start is called before the first frame update
    void Start()
    {
        updateinfo();
    }

    public void updateinfo()
    {
        var label = transform.GetChild(0).GetChild(0).gameObject;
        label.GetComponent<Text>().text = levelName;

        var achievement = transform.GetChild(1).GetChild(0).gameObject;
        stars = stars.Replace('0', '☆');
        stars = stars.Replace('1', '★');
        achievement.GetComponent<Text>().text = stars;

        if (unlocked)
        {
            GetComponent<Button>().onClick = onClick;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseOver.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseExit.Invoke();
    }
}
