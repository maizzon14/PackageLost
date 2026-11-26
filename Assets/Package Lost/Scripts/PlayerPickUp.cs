using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerPickUp : NetworkBehaviour
{
    private readonly List<NetworkObject> heldBoxes = new List<NetworkObject>();
    private readonly NetworkVariable<int> heldCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private StarterAssetsInputs input;
    private bool cogible;

    private NetworkObject boxNetworkObject;
    public Transform posBox;

    public float minForce = 3f;
    public float maxForce = 15;
    public float maxChargeTime = 1.5f;
    public float upwardcurve = 0.35f;

    private bool charging;
    private float chargeTime;
    private bool waitForRelease;

    public LineRenderer trajectoryLine;
    public int maxPoints = 40;
    public float timeStep = 0.05f;
    public LayerMask collisionMask = ~0;

    public int maxHeldBoxes = 3;
    public float boxSpacing = 0f;
    public float boxGap = 0.1f;
    private void Awake()
    {
        input = GetComponentInParent<StarterAssetsInputs>();
    }

    public override void OnNetworkDespawn()
    {
        CancelCharge();
    }

    private void Update()
    {

        if (!IsOwner || input == null) return;

        bool pressed = input.ConsumeInteract();
        bool held = Keyboard.current != null && Keyboard.current.eKey.isPressed;

        bool isHolding = heldCount.Value > 0;
        bool canPickMore = heldCount.Value < maxHeldBoxes;

        if (pressed && canPickMore && boxNetworkObject != null)
        {
            PickBoxServerRpc(new NetworkObjectReference(boxNetworkObject));
            waitForRelease = true; //para que al pulsarle no se lance inmediato
            CancelCharge();
            return;
        }

        if (!isHolding)
        {
            CancelCharge();
            return;
        }

        if (waitForRelease)
        {
            if (!held) waitForRelease = false;
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
            float charge1 = chargeTime / maxChargeTime;
            ThrowBoxServerRpc(charge1);
            CancelCharge();
        }

    }
    private void CancelCharge()
    {
        charging = false;
        chargeTime = 0f;
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
            trajectoryLine.positionCount = 0;
        }

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

            if (Physics.Linecast(pos, next, out RaycastHit hit, collisionMask, QueryTriggerInteraction.Ignore))
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
        if (box == null || !box.IsSpawned || box.transform.parent != null)
        {
            return;
        }

        boxNetworkObject = box;
        cogible = true;
    }

    private void OnTriggerExit(Collider other)
    {
        NetworkObject box = other.GetComponentInParent<NetworkObject>();
        if (box != null && box == boxNetworkObject)
        {
            boxNetworkObject = null;
            cogible = false;
        }
    }

    public bool HasBox()
    {
        return IsServer && heldBoxes.Count > 0;
    }

    public bool HasBoxes()
    {
        return heldCount.Value > 0;
    }

    public void DropAllBoxesServer()
    {
        Debug.Log($"soy dropallboxes he llegado");

        if (!IsServer) return;

        NetworkObject box;
        while ((box = PopTopBox()) != null)
        {
            Rigidbody rb = box.GetComponent<Rigidbody>();
            BoxCollider col = box.GetComponent<BoxCollider>();

            float randomDirX = Random.Range(-5f, 6f);
            float randomDirY = Random.Range(2f, 5f);
            float randomDirZ = Random.Range(-5f, 6f);

            Vector3 dropDir = new Vector3(randomDirX, randomDirY, randomDirZ);

            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.AddForce(dropDir, ForceMode.VelocityChange);
            }
            if (col != null) col.enabled = true;
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

        heldBoxes.RemoveAll(b => b == null);

        if (heldBoxes.Count >= maxHeldBoxes || heldBoxes.Contains(box) || box.transform.parent != null)
        {
            return;
        }

        Rigidbody rb = box.GetComponent<Rigidbody>();
        BoxCollider col = box.GetComponent<BoxCollider>();
        if (rb == null || col == null)
        {
            Debug.LogError("La caja necesita un Rigidbody y un BoxCollider.");
            return;
        }

        if (!box.TrySetParent(NetworkObject, false))
        {
            Debug.LogError("No se pudo asociar la caja al jugador en la red.");
            return;
        }
        float height = (boxSpacing > 0f ? boxSpacing : col.size.y * box.transform.lossyScale.y) + boxGap;
        Vector3 worldPos = posBox.position + Vector3.up * (height * heldBoxes.Count);
        box.transform.SetPositionAndRotation(worldPos, posBox.rotation);

        rb.useGravity = false;
        rb.isKinematic = true;

        heldBoxes.Add(box);
        heldCount.Value = heldBoxes.Count;
    }

    [ServerRpc]
    private void ThrowBoxServerRpc(float charge1)
    {
        NetworkObject box = PopTopBox();
        if (box == null) return;

        Rigidbody rb = box.GetComponent<Rigidbody>();
        BoxCollider col = box.GetComponent<BoxCollider>();
        if (rb == null || col == null)
        {
            Debug.LogError("La caja necesita un Rigidbody y un BoxCollider.");
            return;
        }

        rb.useGravity = true;
        rb.isKinematic = false;
        col.enabled = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(GetThrowVelocity(Mathf.Clamp01(charge1)), ForceMode.VelocityChange);
    }


    public bool DepositHeldBoxServer()
    {
        if (!IsServer) return false;

        NetworkObject box = PopTopBox();

        if (box == null) return false;


        Rigidbody rb = box.GetComponent<Rigidbody>();
        BoxCollider col = box.GetComponent<BoxCollider>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
        if(col != null) col.enabled = true;

        return true;
    }

    private NetworkObject PopTopBox() // Devuelve la caja superior, le quita el padre y actualiza el contador
    { 
        heldBoxes.RemoveAll(b => b == null);
        if (heldBoxes.Count == 0)
        {
            heldCount.Value = 0;
            return null;
        }

        NetworkObject box = heldBoxes[heldBoxes.Count - 1];
        if (!box.TryRemoveParent(true)) return null;

        heldBoxes.RemoveAt(heldBoxes.Count - 1);
        heldCount.Value = heldBoxes.Count;
        return box;
    }
}
