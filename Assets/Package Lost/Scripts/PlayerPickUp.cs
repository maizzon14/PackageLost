using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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

    Rigidbody boxRigidbody;
    BoxCollider boxCollider;

    public float minForce = 3f;
    public float maxForce = 15;
    public float maxChargeTime = 1.5f;
    public float upwardcurve = 0.35f;

    private bool charging;
    private float chargeTime;
    private bool waitForRelease;

	[SerializeField] private LineRenderer trajectoryLine;
	[SerializeField] private int maxPoints = 40;
	[SerializeField] private float timeStep = 0.05f;
	[SerializeField] private LayerMask collisionMask = ~0;
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
        CancelCharge();
    }

    private void Update()
    {
        //ApplyHeldBoxState();

        if (!IsOwner || input == null) return;

        bool pressed = input.ConsumeInteract();
		bool held = Keyboard.current != null && Keyboard.current.eKey.isPressed;

		bool isHolding = heldBoxReference.Value.TryGet(out NetworkObject caja);
		if (isHolding) Debug.Log($"held={held} charging={charging} waitForRelease={waitForRelease}");
		if (!isHolding)
        {
           CancelCharge();
            if (pressed && cogible && boxNetworkObject != null)
            {
                PickBoxServerRpc(new NetworkObjectReference(boxNetworkObject));
                waitForRelease = true; //para que al pulsarle no se lance inmediato
            }
            return;
        }

        if (waitForRelease)
        {
            if(!held) waitForRelease = false;
            return;
        }

        if (held)
        {
            charging = true;
            chargeTime = Mathf.Min(chargeTime + Time.deltaTime, maxChargeTime);
			DrawTrajectory(GetThrowVelocity(chargeTime / maxChargeTime));
		}
        else if (charging)
        {
            float charge1 = chargeTime/maxChargeTime;
            ThrowBoxServerRpc(new NetworkObjectReference(caja), charge1);

            boxNetworkObject = null;
            cogible = false;
            CancelCharge();
        }
    }

    private void CancelCharge()
    {
        charging = false;
        chargeTime = 0f;
        if (trajectoryLine != null) trajectoryLine.enabled = false;
    }
	private Vector3 GetThrowVelocity(float charge1)
	{
        float force = Mathf.Lerp(minForce, maxForce, Mathf.Clamp01(charge1));
		Vector3 dir = (transform.parent.forward + Vector3.up * upwardcurve).normalized;
		return dir * force;
	}

	private void DrawTrajectory(Vector3 velocity)
	{
		if (trajectoryLine == null) return;

		trajectoryLine.enabled = true;

		Vector3 pos = posBox.position;
		int count = 0;
		trajectoryLine.positionCount = maxPoints;
		trajectoryLine.SetPosition(count++, pos);

		for (int i = 1; i < maxPoints; i++)
		{
			Vector3 next = pos + velocity * timeStep;
			velocity += Physics.gravity * timeStep;

			if (Physics.Linecast(pos, next, out RaycastHit hit, collisionMask,
								 QueryTriggerInteraction.Ignore))
			{
				trajectoryLine.SetPosition(count++, hit.point);
				break;
			}

			trajectoryLine.SetPosition(count++, next);
			pos = next;
		}

		trajectoryLine.positionCount = count;
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
            /*if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }*/
        }
    }
    public bool HasBox()
    {
        return IsServer && heldBoxReference.Value.TryGet(out _);
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
        
         boxRigidbody = box.GetComponent<Rigidbody>();
         boxCollider = box.GetComponent<BoxCollider>();
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
    private void ThrowBoxServerRpc(NetworkObjectReference boxReference, float charge1)
    {
		Debug.Log("ThrowBoxServerRpc llamado, charge: " + charge1);
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

        boxRigidbody.AddForce(GetThrowVelocity(charge1), ForceMode.VelocityChange);

        heldBoxReference.Value = default;
    }
    
    
    public bool DepositHeldBoxServer()
    {
        if (!IsServer) return false;

        if (!heldBoxReference.Value.TryGet(out NetworkObject box)) return false;
       
        if (!box.TryRemoveParent(true)) return false;

        heldBoxReference.Value = default;
        Rigidbody boxRigidbody = box.GetComponent<Rigidbody>();
        BoxCollider boxCollider = box.GetComponent<BoxCollider>();
        boxRigidbody.useGravity = true;
        boxRigidbody.isKinematic = false;
        boxCollider.enabled = true;
        return true;
    }
}
