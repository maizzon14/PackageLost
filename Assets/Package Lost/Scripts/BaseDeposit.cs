
using Unity.Netcode;
using UnityEngine;

public class BaseDeposit : NetworkBehaviour
{
    [SerializeField] private ulong ownerClientId;
    [SerializeField] private int packagesToWin = 3;

    private readonly NetworkVariable<int> packagesDeposited = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool winnerDeclared;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || winnerDeclared) return;

        PlayerPickUp player = other.transform.root.GetComponentInChildren<PlayerPickUp>();

        if (player == null || player.OwnerClientId != ownerClientId) return;
           
        if (!player.HasBox()) return;

        if (!player.DepositHeldBoxServer()) return;

        packagesDeposited.Value++;

        Debug.Log($"Base del jugador {ownerClientId}: " + $"{packagesDeposited.Value}/{packagesToWin} paquetes");

        if (packagesDeposited.Value >= packagesToWin)
        {
            winnerDeclared = true;
            Debug.Log($"¡Ha ganado el jugador {ownerClientId}!");
        }
    }
}
