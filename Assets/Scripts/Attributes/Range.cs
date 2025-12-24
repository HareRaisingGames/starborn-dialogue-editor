using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RangeStepAttribute))]
public class RangeStepDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        RangeStepAttribute rangeStep = attribute as RangeStepAttribute;

        if (property.propertyType == SerializedPropertyType.Float)
        {
            float value = EditorGUI.Slider(position, label, property.floatValue, rangeStep.min, rangeStep.max);
            property.floatValue = Mathf.Round(value / rangeStep.step) * rangeStep.step;
        }
        else if (property.propertyType == SerializedPropertyType.Integer)
        {
            int value = EditorGUI.IntSlider(position, label, property.intValue, (int)rangeStep.min, (int)rangeStep.max);
            property.intValue = Mathf.RoundToInt(value / rangeStep.step) * (int)rangeStep.step;
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use RangeStep with float or int.");
        }
    }
}

public class RangeStepAttribute : PropertyAttribute
{
    public float min;
    public float max;
    public float step;

    public RangeStepAttribute(float min, float max, float step)
    {
        this.min = min;
        this.max = max;
        this.step = step;
    }
}
#endif