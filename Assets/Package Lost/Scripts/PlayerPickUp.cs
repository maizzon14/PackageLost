using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerPickUp : NetworkBehaviour
{
    //public NetworkVariable<bool> boxEnabled = new NetworkVariable<bool>(true);
    InputSystem_Actions inputActions;
    bool cogible = false;
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            this.enabled = false;
        }
        inputActions.Player.PickUp.performed += PickBox;
    }
    public override void OnNetworkDespawn()
    {
       
        inputActions.Player.PickUp.performed -= PickBox;
    }
   

    public void PickBox(InputAction.CallbackContext callback)
    {
        print("me llama");
       
        if (cogible)
        {
            print($" El jugador con ID: {this.OwnerClientId} ha cogido la caja");
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

   
}
