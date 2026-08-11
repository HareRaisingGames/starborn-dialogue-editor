using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Required for UI event interfaces
using UnityEngine.UI;

public class ImageButton : Image, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // Tracks hover state
    public bool isHovering { get; private set; }

    // Triggered automatically when mouse enters the Image boundaries
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        Debug.Log("Mouse entered image: " + gameObject.name);
        
        // Add your hover logic here (e.g., change color, play audio, scale up)
    }

    // Triggered automatically when mouse leaves the Image boundaries
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        Debug.Log("Mouse left image: " + gameObject.name);
        
        // Revert your hover logic here
    }

    //Detect if a click occurs
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //Output to console the clicked GameObject's name and the following message. You can replace this with your own actions for when clicking the GameObject.
        Debug.Log(name + " Game Object Clicked!");
    }
}
