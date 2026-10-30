using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Powerups", menuName = "ScritableObjects/PowerUps")]
public class SO_PowerUp : ScriptableObject
{
    public string id;
    public string description;
    public string sabDescription;
    public bool sabotage; //Helps know when to use sabotage vfx/UI
    public bool affectNext; //For sabotages that affect the next player
    public bool affectShot; //If a PU affects the player shot
    public Sprite icon;
    public GameObject prefab;
    public static event Action<SO_PowerUp> UsePU; //Event for applying the PU
    public void Activate()
    {
        UsePU?.Invoke(this);
    }

}
