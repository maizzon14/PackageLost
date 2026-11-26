using StarterAssets;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerHit : NetworkBehaviour
{

    BoxCollider hitTrigger;
    private StarterAssetsInputs input;
    private bool hitDone;
    

    private void Awake()
    {
        hitTrigger = GetComponent<BoxCollider>();
        hitTrigger.enabled = false;

        input = GetComponentInParent<StarterAssetsInputs>();
    }

    private void Update()
    {
        if (!IsOwner || input == null) return;

        if (input.ConsumePlayerHit())
        {
            //Debug.Log("hola soy click");
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        hitDone = false;

        hitTrigger.enabled = true;

        yield return new WaitForSeconds(0.2f);

        hitTrigger.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (!IsOwner || other.transform.root == transform.root || !other.transform.root.CompareTag("Player") || hitDone)
        {
            return;
        }

        NetworkObject player = other.transform.root.GetComponent<NetworkObject>();

        if (player == null)
        {
            return;
        }

        PlayerPickUp playerPickUp = player.GetComponentInChildren<PlayerPickUp>();

        if(playerPickUp == null)
        {
            return;
        }

        if(playerPickUp.HasBoxes()) 
        {
            DropBoxesServerRpc(player);
            hitDone = true;
        }

        print($"he hiteado al jugador: {player.OwnerClientId}");
    }

    [ServerRpc]

    void DropBoxesServerRpc(NetworkObjectReference playerHit)
    {
        if (!playerHit.TryGet(out NetworkObject playerRpc))
        {
            Debug.LogWarning("No se pudo encontrar el jugador de red");
            return;
        }

        Debug.LogWarning($"Jugador Nº{playerRpc.OwnerClientId} ha sido hitteado");

        PlayerPickUp playerPickUp = playerRpc.GetComponentInChildren<PlayerPickUp>();

        if (playerPickUp == null) return;

        playerPickUp.DropAllBoxesServer();

    }
}
