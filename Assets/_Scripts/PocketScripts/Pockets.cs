using UnityEngine;

public class Pockets : MonoBehaviour
{
    //references
    private PocketManager _pocketManager;

    private void Awake()
    {
        _pocketManager = GetComponentInParent<PocketManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _pocketManager.OnChildCollision(GetComponent<Collider>(), other); //send out this collider and the other collider (other being a ball)
    }
}
