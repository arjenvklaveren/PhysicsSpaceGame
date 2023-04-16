using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlanetRing", order = 1)]
public class PlanetRing : ScriptableObject
{
    public Color ringColor;
    public int density;
    public int width;
}