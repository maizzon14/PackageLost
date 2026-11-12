using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
	public NetworkList<NetworkItem> Inventory;

	[SerializeField, Tooltip("Add all the items that the player can pick up.")] private ItemData[] itemDatabase;
	private void Awake()
	{
		Inventory = new NetworkList<NetworkItem>();
	}

	[Rpc(SendTo.Server)]
	public void PickupItemRpc(uint pickedItemID, int amount, ulong networkObjectId)
	{
		NetworkItem newItem = new NetworkItem
		{
			ItemID = pickedItemID,
			Amount = amount
		};

		Inventory.Add(newItem);

		if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject itemToDespawn))
		{
			itemToDespawn.Despawn(false); //We don't destroy the game object
		}
	}

	public void ShowInventory()
	{
		foreach (var networkItem in Inventory)
		{
			// Translate the ID to an item
			ItemData itemData = GetItemDataByID(networkItem.ItemID);

			Debug.Log($"I have {networkItem.Amount} of {itemData.ItemName}");
		}
	}


	private ItemData GetItemDataByID(uint id)
	{
		foreach (var item in itemDatabase)
		{
			if (item.ItemID == id) return item;
		}
		return null;
	}
}