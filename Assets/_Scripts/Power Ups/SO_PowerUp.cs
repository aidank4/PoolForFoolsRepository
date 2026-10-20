using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Powerups", menuName = "ScritableObjects/PowerUps")]
public class SO_PowerUp : ScriptableObject
{
    public string id;
    public string description;
    public bool sabotage;
    public bool affectNext;
    public Sprite icon;
    public GameObject prefab;
    public static event Action<SO_PowerUp> UsePU;
    public void Activate()
    {
        UsePU?.Invoke(this);
    }

}
