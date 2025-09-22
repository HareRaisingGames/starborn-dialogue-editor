using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using TMPro;

namespace UnityEngine.UI
{
    public class Checkbox : MonoBehaviour
    {
        private bool _value = false;
        public Color checkedColor = Color.gray;
        public Color uncheckedColor = Color.white;
        Color color;
        public Image checkmark;
        Button m_button;
        [Serializable]
        public class OnChangeEvent : UnityEvent<bool> { }

        [SerializeField]
        private OnChangeEvent m_OnValueChanged = new OnChangeEvent();

        public bool isChecked
        {
            get
            {
                color = _value ? checkedColor : uncheckedColor;
                return _value;
            }
            set
            {
                color = value ? checkedColor : uncheckedColor;
                _value = value;
            }
        }

        public OnChangeEvent onValueChanged
        {
            get { return m_OnValueChanged; }
            set { m_OnValueChanged = value; }
        }
        // Start is called before the first frame update
        void Start()
        {
            if (GetComponent<Button>() == null)
                checkmark.gameObject.AddComponent<Button>();
            m_button = checkmark.GetComponent<Button>();
            m_button.onClick.AddListener(SetCheck);
            color = isChecked ? checkedColor : uncheckedColor;
            checkmark.color = color;
        }

        void SetCheck()
        {
            _value = !_value;
            color = _value ? checkedColor : uncheckedColor;
            checkmark.color = color;
            if (onValueChanged != null)
                onValueChanged.Invoke(_value);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}