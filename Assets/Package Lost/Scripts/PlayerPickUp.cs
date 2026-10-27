using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public class PlayerPickUp : NetworkBehaviour
{
    private readonly NetworkVariable<NetworkObjectReference> heldBoxReference =
        new NetworkVariable<NetworkObjectReference>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private StarterAssetsInputs input;
    private bool cogible;
    private NetworkObject boxNetworkObject;
    public Transform posBox;

    private void Awake()
    {
        input = GetComponentInParent<StarterAssetsInputs>();
    }

    public override void OnNetworkSpawn()
    {
        heldBoxReference.OnValueChanged += OnHeldBoxChanged;
        ApplyHeldBoxState();
    }

    public override void OnNetworkDespawn()
    {
        heldBoxReference.OnValueChanged -= OnHeldBoxChanged;
    }

    private void Update()
    {
        ApplyHeldBoxState();

        if (!IsOwner || input == null || !input.ConsumeInteract() || !cogible || boxNetworkObject == null)
        {
            return;
        }

        PickBoxServerRpc(new NetworkObjectReference(boxNetworkObject));
        cogible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner)
        {
            return;
        }

        NetworkObject box = other.GetComponentInParent<NetworkObject>();
        if (box == null || !box.IsSpawned)
        {
            return;
        }

        boxNetworkObject = box;
        cogible = true;
    }

    private void OnTriggerExit(Collider other)
    {
        NetworkObject box = other.GetComponentInParent<NetworkObject>();
        if (box == boxNetworkObject)
        {
            boxNetworkObject = null;
            cogible = false;
        }
    }

    private void OnHeldBoxChanged(NetworkObjectReference previousValue, NetworkObjectReference newValue)
    {
        ApplyHeldBoxState();
    }

    private void ApplyHeldBoxState()
    {
        if (heldBoxReference.Value.TryGet(out NetworkObject box))
        {
            BoxCollider boxCollider = box.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }
        }
    }

    [ServerRpc]
    private void PickBoxServerRpc(NetworkObjectReference boxReference)
    {
        if (!boxReference.TryGet(out NetworkObject box))
        {
            Debug.LogWarning("No se pudo encontrar la caja de red.");
            return;
        }

        if (heldBoxReference.Value.TryGet(out _) || box.transform.parent != null)
        {
            return;
        }
        
        Rigidbody boxRigidbody = box.GetComponent<Rigidbody>();
        BoxCollider boxCollider = box.GetComponent<BoxCollider>();
        if (boxRigidbody == null || boxCollider == null)
        {
            Debug.LogError("La caja necesita un Rigidbody y un BoxCollider.");
            return;
        }

        if (!box.TrySetParent(NetworkObject, false))
        {
            Debug.LogError("No se pudo asociar la caja al jugador en la red.");
            return;
        }

        box.transform.SetLocalPositionAndRotation(
            posBox.localPosition,
            posBox.localRotation
        );
        boxRigidbody.useGravity = false;
        boxRigidbody.isKinematic = true;
        boxCollider.enabled = false;
        heldBoxReference.Value = new NetworkObjectReference(box);
    }
}
