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

    [SerializeField] private float force = 5f;
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

        if (!IsOwner || input == null || !input.ConsumeInteract()) return;

        if (heldBoxReference.Value.TryGet(out NetworkObject caja))
        {
            print("estoy arriba");
            ThrowBoxServerRpc(new NetworkObjectReference(caja));
            return;
        }

        if (!cogible || boxNetworkObject == null) return;

        PickBoxServerRpc(new NetworkObjectReference(boxNetworkObject));
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner || !other.CompareTag("Box"))
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

        box.transform.SetLocalPositionAndRotation(posBox.localPosition,posBox.localRotation);

        boxRigidbody.useGravity = false;
        boxRigidbody.isKinematic = true;
        //boxCollider.enabled = false;

        heldBoxReference.Value = new NetworkObjectReference(box);
        cogible = false;
        boxNetworkObject = null;
    }

    [ServerRpc]
    private void ThrowBoxServerRpc(NetworkObjectReference boxReference)
    {
        if (!boxReference.TryGet(out NetworkObject box))
        {
            Debug.LogWarning("No se pudo encontrar la caja de red.");
            return;
        }

        if (!box.TryRemoveParent(true)) return;

        Rigidbody boxRigidbody = box.GetComponent<Rigidbody>();
        BoxCollider boxCollider = box.GetComponent<BoxCollider>();

        if (boxRigidbody == null || boxCollider == null)
        {
            Debug.LogError("La caja necesita un Rigidbody y un BoxCollider.");
            return;
        }

        boxRigidbody.useGravity = true;
        boxRigidbody.isKinematic = false;
        boxCollider.enabled = true;

        Vector3 throwDirection = transform.parent.forward;
        boxRigidbody.AddForce(throwDirection * force, ForceMode.VelocityChange);

        heldBoxReference.Value = default;
    }
}
