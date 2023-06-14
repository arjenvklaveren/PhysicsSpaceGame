using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class RangeExAttribute : PropertyAttribute
{
    public readonly float min = .0f;
    public readonly float max = 100.0f;
    public readonly float step = 1.0f;
    public readonly string label = "";

    public RangeExAttribute(float min, float max, float step = 1.0f, string label = "")
    {
        this.min = min;
        this.max = max;
        this.step = step;
        this.label = label;
    }
}