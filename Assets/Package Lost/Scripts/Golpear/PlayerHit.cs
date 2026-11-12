using StarterAssets;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerHit : NetworkBehaviour
{

    BoxCollider hitTrigger;
    private StarterAssetsInputs input;
    private NetworkObject myPlayer;

    private void Awake()
    {
        hitTrigger = GetComponent<BoxCollider>();
        hitTrigger.enabled = false;

        input = GetComponentInParent<StarterAssetsInputs>();
        myPlayer = GetComponentInParent<NetworkObject>();
    }

    private void Update()
    {
        if(input.ConsumePlayerHit())
        {
            Debug.Log("hola soy click");
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        hitTrigger.enabled = true;

        yield return new WaitForSeconds(0.2f);

        hitTrigger.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject player = other.GetComponentInParent<NetworkObject>();

        Debug.Log("He detectado algo: " + player.OwnerClientId);
        Debug.Log("He detectado algo: " + other.name);


        if (!IsOwner || other.transform.root == transform.root || !other.transform.root.CompareTag("Player"))
        {
            return;
        }
        

        if (player == null)
        {
            return;
        }

        print($"he hiteado al jugador: {player.OwnerClientId}");
    }
}
