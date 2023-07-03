using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RangeExAttribute))]
internal sealed class RangeExDrawer : PropertyDrawer
{

    private int Precision(float value)
    {
        int _precision;
        if (value == .0f) return 0;
        _precision = value.ToString().Length - (((int)value).ToString().Length + 1);
        // Math.Round function get only precision between 0 to 15
        return Mathf.Clamp(_precision, 0, 15);
    }

    private float Step(float value, float min, float step)
    {
        if (step == 0) return value;
        float newValue = min + Mathf.Round((value - min) / step) * step;
        return (float)System.Math.Round(newValue, Precision(step));
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rangeAttribute = (RangeExAttribute)base.attribute;

        if (rangeAttribute.label != "")
            label.text = rangeAttribute.label;

        switch (property.propertyType)
        {
            case SerializedPropertyType.Float:
                float _floatValue = EditorGUI.Slider(position, label, property.floatValue, rangeAttribute.min, rangeAttribute.max);
                property.floatValue = Step(_floatValue, rangeAttribute.min, rangeAttribute.step);
                break;
            default:
                EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
                break;
        }
    }
}
#endif