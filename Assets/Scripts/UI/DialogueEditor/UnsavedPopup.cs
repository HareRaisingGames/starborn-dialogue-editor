using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UnsavedPopup : MonoBehaviour
{
    static UnsavedPopup _instance = null;
    public static UnsavedPopup instance
    {
        get
        {
            if (_instance == null)
            {
                if (FindObjectOfType<UnsavedPopup>() == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("Prefabs/AreYouSure");
                    if (prefab != null)
                    {
                        Instantiate(prefab, Vector3.zero, Quaternion.identity).name = "AreYouSure";
                    }
                    else
                    {
                        GameObject manager = new GameObject("AreYouSure");
                        _instance = manager.AddComponent<UnsavedPopup>();
                    }
                }
                _instance = FindObjectOfType<UnsavedPopup>();

            }

            return _instance;
        }
    }
    public TMP_Text message;

    public Action yesCallback;
    public Action noCallback;
    public Action openCallback;


    private void Awake()
    {
        _instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void Open(string message, Action onYes = null, Action onNo = null)
    {
        instance.message.text = message;
        instance.yesCallback = onYes;
        instance.noCallback = onNo;
    }

    public static void Close()
    {
        Destroy(_instance.gameObject);
        _instance = null;
    }

    public void Yes()
    {
        yesCallback?.Invoke();
    }

    public void No()
    {
        noCallback?.Invoke();
    }
}
