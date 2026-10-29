using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class BasePoints : NetworkBehaviour
{
	public static BasePoints Instance;

	[Header("Bases en orden: Base1, Base2, Base3, Base4")]
	[SerializeField] private NetworkObject[] bases = new NetworkObject[4];

	private Dictionary<ulong, int> playerBases = new Dictionary<ulong, int>();

	private NetworkVariable<int>[] scores = new NetworkVariable<int>[4];

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		Debug.Log("BasePoints iniciado. IsServer: " + IsServer);

		Instance = this;

		for (int i = 0; i < 4; i++)
		{
			scores[i] = new NetworkVariable<int>(
				0,
				NetworkVariableReadPermission.Everyone,
				NetworkVariableWritePermission.Server
			);
		}

		if (IsServer)
		{
			AssignConnectedPlayers();
			NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
		}
	}

	private void AssignConnectedPlayers()
	{
		foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
		{
			AssignBase(clientId);
		}
	}

	private void OnClientConnected(ulong clientId)
	{
		AssignBase(clientId);
	}

	private void OnClientDisconnected(ulong clientId)
	{
		playerBases.Remove(clientId);
	}

	private void AssignBase(ulong clientId)
	{
		if (playerBases.ContainsKey(clientId))
			return;

		for (int i = 0; i < bases.Length; i++)
		{
			if (bases[i] == null)
				continue;

			bool occupied = playerBases.ContainsValue(i);

			if (!occupied)
			{
				playerBases[clientId] = i;

				Debug.Log($"Cliente {clientId} asignado a Base {i + 1}");
				return;
			}
		}

		Debug.LogWarning($"No hay bases libres para el cliente {clientId}");
	}

	public int GetBaseIndex(ulong clientId)
	{
		if (playerBases.TryGetValue(clientId, out int index))
			return index;

		return -1;
	}

	public void AddPoint(int baseIndex)
	{
		if (!IsServer)
			return;

		if (baseIndex < 0 || baseIndex >= scores.Length)
			return;

		scores[baseIndex].Value++;

		Debug.Log($"Base {baseIndex + 1}: {scores[baseIndex].Value} puntos");
	}

	public int GetScore(int baseIndex)
	{
		if (baseIndex < 0 || baseIndex >= scores.Length)
			return 0;

		return scores[baseIndex].Value;
	}

	public override void OnNetworkDespawn()
	{
		if (IsServer && NetworkManager.Singleton != null)
		{
			NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
		}

		if (Instance == this)
			Instance = null;

		base.OnNetworkDespawn();
	}
}
