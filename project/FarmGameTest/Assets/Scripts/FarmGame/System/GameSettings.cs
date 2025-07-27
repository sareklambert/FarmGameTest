using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("World")]
    public float cellSize = 1f;
    public int gridSizeX = 8;
    public int gridSizeZ = 6;

    [Header("Game")] public int initialMoney = 100;
}
