using UnityEngine;

[CreateAssetMenu(fileName = "SO_Powerups", menuName = "ScritableObjects/PowerUps")]
public class SO_PowerUp : ScriptableObject
{
    public string id;
    public string description;
    public Sprite icon;
    public GameObject prefab;
}
