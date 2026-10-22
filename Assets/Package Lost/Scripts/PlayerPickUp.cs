using StarterAssets;
using Unity.Netcode;
using UnityEngine;


public class PlayerPickUp : NetworkBehaviour
{
    //public NetworkVariable<bool> boxEnabled = new NetworkVariable<bool>(true);
    StarterAssetsInputs  input;
    bool cogible = false;
    Transform boxTransform;
    public Transform posBox;
    Rigidbody rb;


    private void Awake()
    {
        input = GetComponentInParent<StarterAssetsInputs>();
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            this.enabled = false;
        }
    }
    public override void OnNetworkDespawn()
    {
       
    }

    private void Update()
    {
        PickBox();
    }

    public void PickBox()
    {
        if (input != null && input.ConsumeInteract() && cogible)
        {
            rb.GetComponent<BoxCollider>().enabled = false;
            rb.useGravity = false;
            Debug.Log("Recoger objeto");
            print($" El jugador con ID: {this.OwnerClientId} ha cogido la caja");
            boxTransform.position = posBox.position;
            boxTransform.SetParent(this.transform.parent);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        NetworkObject box = other.GetComponent<NetworkObject>();
         rb = other.GetComponent<Rigidbody>();

        if(IsClient && box != null)
        {
            boxTransform = other.transform;

            
            cogible = true; 
        }

    }

    private void OnTriggerExit(Collider other)
    {
        cogible = false;
    }

}
