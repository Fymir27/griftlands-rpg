using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuCursor : MonoBehaviour
{   
    public enum FollowDirection
    {
        Vertical,
        Horizontal
    }

    [SerializeField]
    FollowDirection followDirection;   

    public void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            var selectedGo = EventSystem.current.currentSelectedGameObject;
            UpdatePosition(selectedGo);
        }
    }
   

    public void UpdatePosition(GameObject selectedObject)
    {
        var position = transform.position;
        switch (followDirection)
        {
            case FollowDirection.Vertical:
                position.y = selectedObject.transform.position.y;
                break;
            case FollowDirection.Horizontal:
                position.x = selectedObject.transform.position.x;
                break;
        }
        transform.position = position;
    }
}
