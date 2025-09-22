using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class Draggable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
    protected bool canDrag = false;
    protected bool isHovering;

    Vector2 offset;
    // Start is called before the first frame update

    // Update is called once per frame
    public virtual void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            canDrag = true;
            offset = Input.mousePosition - transform.position;
        }

        if (Input.GetMouseButtonUp(0))
        {
            canDrag = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        //Debug.Log("Mouse entered " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        //Debug.Log("Mouse exited " + gameObject.name);
    }

    public void OnDrag(PointerEventData data)
    {
        if (canDrag)
            transform.position = data.position - offset;
        Cursor.lockState = CursorLockMode.Confined;

        //Debug.Log("Pointer: " + data.position + " Mouse: " + Input.mousePosition);
    }
}
