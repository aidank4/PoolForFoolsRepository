using UnityEngine;


[CreateAssetMenu(fileName = "NewBall", menuName = "BallScriptableObjects/NewBall")]
public class BallSO : ScriptableObject
{
    public GameObject ballObject;
    public bool stripes;
    public bool spawned;
    public bool eightBall;
    public int arrayNum; // temp for spawning can't figure out better way
}
