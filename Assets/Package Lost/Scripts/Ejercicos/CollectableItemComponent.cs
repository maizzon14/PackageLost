using UnityEngine;
using Unity.Netcode;
[RequireComponent(typeof(Collider))]
public class CollectableItemComponent : NetworkBehaviour
{
	[SerializeField] private ItemData itemData;
	[SerializeField] private int amount;
	private Collider colliderComponent;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		AssignData();
		colliderComponent.isTrigger = true;
	}
	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		gameObject.SetActive(false); //Turn off the game object
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!IsClient) return;

		if (other.TryGetComponent(out PlayerInventory inventory))
		{

			if (!inventory.IsOwner) return; //Make sure only the owners inventory gets the object

			inventory.PickupItemRpc(itemData.ItemID, this.amount, NetworkObjectId);
		}
	}

	private void AssignData()
	{
		if (itemData != null)
		{
			if (TryGetComponent(out MeshFilter meshFilter))
				meshFilter.mesh = itemData.mesh;
			if (TryGetComponent(out MeshRenderer meshRenderer))
				meshRenderer.material = itemData.material;

			//Probably there should be a collider data also in order the manage the mesh proportions and the corresponding collider

			gameObject.name = itemData.name;
		}
	}
}