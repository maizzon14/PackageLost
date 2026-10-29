using UnityEngine;
using Unity.Netcode;

public class BaseTrigger : NetworkBehaviour
{
	[SerializeField] private int baseIndex;

	private void OnTriggerEnter(Collider other)
	{
		if (!IsServer)
			return;

		if (!other.CompareTag("Box"))
			return;

		if (BasePoints.Instance == null)
		{
			Debug.LogError("No se encuentra BasePoints.Instance");
			return;
		}

		BasePoints.Instance.AddPoint(baseIndex);
	}
}
