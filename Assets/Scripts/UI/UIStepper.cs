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
    public class UIStepper : MonoBehaviour
    {
        private float _value = 0;
        public float increment = 1;
        public float startValue = 0;

        public TMP_InputField field;
        public Button increase;
        public Button decrease;

        [Serializable]
        public class OnChangeEvent : UnityEvent<float> { }

        [SerializeField]
        private OnChangeEvent m_OnValueChanged = new OnChangeEvent();

        public OnChangeEvent onValueChanged
        {
            get { return m_OnValueChanged; }
            set { m_OnValueChanged = value; }
        }

        public float value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public string displayValue
        {
            get
            {
                return _value.ToString();
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            _value = startValue;
            field.text = _value.ToString();

            increase.onClick.AddListener(Increase);
            decrease.onClick.AddListener(Decrease);
            field.onValueChanged.AddListener(ChangeInInput);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ChangeInInput(string type)
        {
            try
            {
                _value = float.Parse(field.text);
            }
            catch
            {
                _value = 0;
            }
            if (onValueChanged != null)
                onValueChanged.Invoke(_value);
            //Debug.Log("Change");
        }

        void Increase()
        {
            _value += increment;
            field.text = _value.ToString();
        }

        void Decrease()
        {
            _value -= increment;
            field.text = _value.ToString();
        }

    }
}



