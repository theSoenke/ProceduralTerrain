using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(MinMaxSlider))]
class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        const float ComponentHeight = 16.0f;
        const float VerticalPadding = 2.0f;
        const float HorizontalPadding = 5.0f;
        const float FieldWidth = 45;

        if (property.propertyType != SerializedPropertyType.Vector2)
        {
            EditorGUI.LabelField(position, label, "Type needs to be a Vector2");
            return;
        }

        Vector2 range = property.vector2Value;
        float min = range.x;
        float max = range.y;
        MinMaxSlider attr = attribute as MinMaxSlider;

        label = EditorGUI.BeginProperty(position, label, property);

        float verticalPos = position.y + ComponentHeight + VerticalPadding;
        float sliderWidth = position.width - 5 * HorizontalPadding - 2 * FieldWidth;
        Rect sliderRect = new Rect(position.x + FieldWidth + 2 * HorizontalPadding, verticalPos, sliderWidth, ComponentHeight);
        Rect leftRect = new Rect(position.x + HorizontalPadding, verticalPos, FieldWidth, ComponentHeight);
        Rect rightRect = new Rect(position.width - FieldWidth + HorizontalPadding, verticalPos, FieldWidth, ComponentHeight);

        EditorGUI.LabelField(position, label);

        EditorGUI.BeginChangeCheck();

        EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, attr.min, attr.max);

        min = EditorGUI.FloatField(leftRect, min);
        max = EditorGUI.FloatField(rightRect, max);

        if (EditorGUI.EndChangeCheck())
        {
            if (!(min > max || max < min))
            {
                if (min < attr.min)
                {
                    min = attr.min;
                }

                if (max > attr.max)
                {
                    max = attr.max;
                }

                range.x = min;
                range.y = max;
                property.vector2Value = range;
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + 18.0f + 18.0f;
    }
}