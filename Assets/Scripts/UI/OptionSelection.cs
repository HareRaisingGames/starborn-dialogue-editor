using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rabbyte;

public abstract class OptionSelection : MonoBehaviour
{
    public DialogueManager manager;
    bool isSelected;
    private void Awake()
    {
        manager = FindObjectOfType<DialogueManager>();
    }
    public virtual void Start()
    {
        
    }
    // Update is called once per frame
    public virtual void Update()
    {
        isSelected = gameObject.activeInHierarchy;
    }

    public abstract void Load(SimpleSBDFile sBDFile);
}
