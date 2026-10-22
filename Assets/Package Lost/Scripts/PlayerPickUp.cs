using StarterAssets;
using Unity.Netcode;
using UnityEngine;


public class PlayerPickUp : NetworkBehaviour
{
    //public NetworkVariable<bool> boxEnabled = new NetworkVariable<bool>(true);
    StarterAssetsInputs  input;
    bool cogible = false;
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
            Debug.Log("Recoger objeto");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        NetworkObject box = other.GetComponent<NetworkObject>();
        print(box);
        if(IsClient && box != null)
        {
            print(box.NetworkObjectId);
            cogible = true; 
        }

    }

    private void OnTriggerExit(Collider other)
    {
        cogible = false;
    }

}
