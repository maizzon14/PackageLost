using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterComponent : NetworkBehaviour
{
	[SerializeField]
	private Transform spawnTransform;
	public override void OnNetworkSpawn()
	{
		if (!IsOwner)
		{
			enabled = false;
		}
	}
	void Update()
	{
		if (Keyboard.current.tKey.wasPressedThisFrame)
		{
			ShootServerRpc(spawnTransform.position, spawnTransform.rotation);
		}
	}

	[Rpc(SendTo.Server)]
	private void ShootServerRpc(Vector3 pos, Quaternion rot)
	{
		EventManager.OnBulletShoot?.Invoke(pos, rot);
	}
}