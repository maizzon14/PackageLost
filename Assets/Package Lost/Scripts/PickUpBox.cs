using StarterAssets;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpBox : NetworkBehaviour
{
    private ThirdPersonController closestPlayer = null; //necesitaremos referenciar otra cosa no el thirdpersoncontroller

    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if(IsClient && closestPlayer != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                GrabbedBox();
            }
        }
    }


    void GrabbedBox()
    {

    }


    void OnTriggerEnter (Collider other)
    {
        if (IsClient && IsOwner)
        {
            //closestPlayer = ;
            NetworkObject player = other.GetComponent<NetworkObject>();

            if (player != null && player.IsOwner)
            {
                closestPlayer = player.GetComponent<ThirdPersonController>();

            }
            
     

        }
        else
        {
            return;
        }
    }
}
