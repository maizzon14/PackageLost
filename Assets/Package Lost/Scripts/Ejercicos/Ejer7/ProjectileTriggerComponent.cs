using System.Collections;
using Unity.Netcode;
using UnityEngine;


[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ProjectileTriggerComponent : NetworkBehaviour
{
	[SerializeField]
	private float timeToDespawn = 4.0f;
	[SerializeField]
	private float speed = 1.0f;
	Collider colliderComponent;
	Rigidbody rigidbodyComponent;

	void FixedUpdate()
	{
		if (!IsServer) return;
		rigidbodyComponent.linearVelocity = transform.forward * speed;
	}

	public override void OnNetworkSpawn()
	{
		colliderComponent = GetComponent<Collider>();
		rigidbodyComponent = GetComponent<Rigidbody>();
		colliderComponent.isTrigger = true;
		if (IsServer)
			StartCoroutine(DespawnOnTime());

	}
	public void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out NetworkObject networkObject) && IsClient)
		{
			DespawnParentServerRpc();
		}
	}

	private IEnumerator DespawnOnTime()
	{
		yield return new WaitForSeconds(timeToDespawn);
		DespawnParentServerRpc();
	}

	[Rpc(SendTo.Server)]
	private void DespawnParentServerRpc()
	{
		if (transform.parent.TryGetComponent(out NetworkObject parentNetworkObject))
			parentNetworkObject.Despawn();
	}
}