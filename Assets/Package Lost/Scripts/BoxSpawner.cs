using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BoxSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject prefab;
    [SerializeField] Transform[] cajaSpawn;
    [SerializeField] float intervalo;
    [SerializeField] int maxCajas;

    readonly List<NetworkObject> cajas = new List<NetworkObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        StartCoroutine(SpawnearCadaCiertoTiempo());
    }
    public override void OnNetworkDespawn()
    {
        StopAllCoroutines();
    }

    IEnumerator SpawnearCadaCiertoTiempo()
    {
        var espera = new WaitForSeconds(intervalo);
        while (true) 
        {
            yield return espera;
            
            cajas.RemoveAll(c => c != null || !c.IsSpawned);

            if (cajas.Count < maxCajas)
                SpawnerBox();
        }
    }



    public void SpawnerBox()
    {
        Transform punto = cajaSpawn[Random.Range(0, cajaSpawn.Length)];

        NetworkObject caja = Instantiate(prefab, punto.position, punto.rotation);

        caja.Spawn();

        cajas.Add(caja);
    }
}
