using System;
using Unity.Netcode;
using UnityEngine;

public class Puntuation : NetworkBehaviour
{
    private ulong ownerClientId; // Esto lo vamos a usar para que los jugadores que intenten robar paquetes no lo vuelvana a dejar en una base que no sea suya.
    [SerializeField] private int packagesToWin = 3;

    private readonly NetworkVariable<int> packagesDeposited = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool winnerDeclared;

    private void OnTriggerEnter(Collider other)
    {
        //if (!IsServer || winnerDeclared) return;

        PlayerPickUp player = other.transform.root.GetComponentInChildren<PlayerPickUp>();

        //if (player == null /*|| player.OwnerClientId != ownerClientId*/) return;
           
        //if (!player.HasBox()) return;

        //if (!player.DepositHeldBoxServer()) return;
            if(other.CompareTag("Box") && IsOwner)
        {
            packagesDeposited.Value++;
        }
            

        Debug.Log($"Base del jugador: {ownerClientId} Tiene {packagesDeposited.Value}/{packagesToWin} paquetes");

        if (packagesDeposited.Value >= packagesToWin)
        {
            winnerDeclared = true;
            Debug.Log($"�Ha ganado el jugador {ownerClientId}!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Box")) packagesDeposited.Value--;
        Debug.Log($"Base del jugador: " + $"{packagesDeposited.Value}/{packagesToWin} paquetes");
    }
}
