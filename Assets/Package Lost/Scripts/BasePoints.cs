using UnityEngine;
using Unity.Netcode;
using TMPro;


public class BasePoints : NetworkBehaviour
{
	public static BasePoints Instance;

	private NetworkList<int> scores;

	[SerializeField] private TMP_Text[] textos = new TMP_Text[4];

	private void Awake()
	{
			scores = new NetworkList<int>();
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		
		Instance = this;
		if (IsServer)
		{
			for (int i = 0; i < 4; i++)
			{
				scores.Add(0);
			}
		}

		for (int i = 0; i < scores.Count; i++)
		{
			textos[i].text = $"Base {i + 1}: {scores[i]} puntos";
		}
		
		scores.OnListChanged+= OnScoresChanged;
	}


	public void AddPoint(int baseIndex, int puntos)
	{
		if (!IsServer)
			return;
		
		if (baseIndex < 0 || baseIndex >= scores.Count)
			return;
		
		scores[baseIndex] = Mathf.Max(0, scores[baseIndex] + puntos);

		
	}

	private void OnScoresChanged(NetworkListEvent<int> cambio)
	{
		 Debug.Log($"Base {cambio.Index + 1}: {cambio.Value} puntos");
		 textos[cambio.Index].text = $"Base {cambio.Index + 1}: {cambio.Value} puntos";
	}

	public int GetScore(int baseIndex)
	{
		if (baseIndex < 0 || baseIndex >= scores.Count)
			return 0;

		return scores[baseIndex];
	}

	public override void OnNetworkDespawn()
	{
		if (Instance == this)
			Instance = null;

		base.OnNetworkDespawn();
		scores.OnListChanged-= OnScoresChanged;
	}
}
