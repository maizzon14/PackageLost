using Unity.Netcode;
using System;

public struct NetworkItem : INetworkSerializable, IEquatable<NetworkItem>
{
	public uint ItemID;
	public int Amount;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref ItemID);
		serializer.SerializeValue(ref Amount);
	}

	public bool Equals(NetworkItem other)
	{
		return ItemID == other.ItemID && Amount == other.Amount;
	}
}
