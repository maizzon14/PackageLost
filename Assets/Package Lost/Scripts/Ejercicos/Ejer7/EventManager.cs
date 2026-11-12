using System;

using UnityEngine;
public static class EventManager
{
	public static Action<ulong> OnPlayerDied;
	public static Action<ulong, float, float> OnHealthChanged;
	public static Action<Vector3, Quaternion> OnBulletShoot;


}