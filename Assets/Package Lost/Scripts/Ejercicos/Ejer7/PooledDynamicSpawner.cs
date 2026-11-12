using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PooledDynamicSpawner : NetworkBehaviour, INetworkPrefabInstanceHandler
{
	[Header("Pool Settings")]
	public GameObject prefabToSpawn;
	public int initialPoolSize = 20;

	private Queue<NetworkObject> m_PooledObjects = new Queue<NetworkObject>();



	private void Start()
	{
		for (int i = 0; i < initialPoolSize; i++)
		{
			CreateNewInstance();
		}
	}

	private NetworkObject CreateNewInstance()
	{
		GameObject instance = Instantiate(prefabToSpawn);
		instance.SetActive(false);

		NetworkObject netObj = instance.GetComponent<NetworkObject>();
		m_PooledObjects.Enqueue(netObj);

		return netObj;
	}

	private NetworkObject GetFromPool()
	{
		if (m_PooledObjects.Count == 0)
		{
			Debug.LogWarning("Empty pool. Creating instance.");
			CreateNewInstance();
		}
		return m_PooledObjects.Dequeue();
	}

	public override void OnNetworkSpawn()
	{
		NetworkManager.Singleton.PrefabHandler.AddHandler(prefabToSpawn, this);
		EventManager.OnBulletShoot += SpawnBullet;
	}

	public override void OnNetworkDespawn()
	{
		EventManager.OnBulletShoot -= SpawnBullet;
	}

	/// <summary>
	/// Invoked only on non-authority clients
	/// INetworkprefabInstanceHandler.Instantiate implementation
	/// Called when Netcode for GameObjects need an instance to be spawned
	/// </summary>
	public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
	{
		NetworkObject netObj = GetFromPool();
		netObj.gameObject.SetActive(true);
		netObj.transform.SetPositionAndRotation(position, rotation);
		return netObj;
	}

	/// <summary>
	/// Called on all game clients
	/// INetworkprefabInstanceHandler.Destroy implementation
	/// </summary>
	public void Destroy(NetworkObject networkObject)
	{
		networkObject.gameObject.SetActive(false);
		m_PooledObjects.Enqueue(networkObject);
	}

	/// <summary>
	/// Spawns the bullet.
	/// </summary>
	public void SpawnBullet(Vector3 position, Quaternion rotation)
	{
		if (!IsServer) return;
		print("disparo");
		NetworkObject bullet = GetFromPool();

		bullet.transform.SetPositionAndRotation(position, rotation);
		bullet.gameObject.SetActive(true);

		bullet.Spawn(true);
	}

	public override void OnDestroy()
	{
		if (NetworkManager.Singleton != null && NetworkManager.Singleton.PrefabHandler != null)
		{
			NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefabToSpawn);
		}
		base.OnDestroy();
	}
}