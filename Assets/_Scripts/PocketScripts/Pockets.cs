using UnityEngine;

public class Pockets : MonoBehaviour
{
    //references
    private PocketManager _pocketManagerRef;

    private void Awake()
    {
        _pocketManagerRef = GetComponentInParent<PocketManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _pocketManagerRef.OnChildCollision(GetComponent<Collider>(), other); //send out this collider and the other collider
    }
}
