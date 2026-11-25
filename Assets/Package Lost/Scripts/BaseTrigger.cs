using System;
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

		BasePoints.Instance.AddPoint(baseIndex,1);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!IsServer)
			return;

		if (!other.CompareTag("Box"))
			return;

		BasePoints.Instance.AddPoint(baseIndex,-1);
	}
}
