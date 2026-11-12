using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private static GameManager Instance;
    public ulong?[] bases = new ulong?[4];   // El ? de ulong? hace que una posición que valga null, que aqui significa "base libre" no pueda usar 0 para eso, porque ese es el id del host

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected; 
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected; 
        }
    }

    private void OnClientConnected(ulong id)
    {
        Debug.Log($"id conectado: {id}");

        for (int i = 0; i < bases.Length; i++)
        {
            
            if (bases[i] == null)
            {
                
                bases[i] = id;
                Debug.Log($"Base {i} asiganda al jugador {id}");
                return;
            }
        }
    }

    private void OnClientDisconnected(ulong id)
    {
        for (int i = 0; i < bases.Length; i++)
        {
            if (bases[i] == id)
            {
                bases[i] = null;

                return;
            }
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
