
using UnityEngine;
using Unity.Netcode;

public class NetworkSquareSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject networkSquarePrefab;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnSquare;
    }

    private void SpawnSquare()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        GameObject square = Instantiate(
            networkSquarePrefab,
            Vector3.zero,
            Quaternion.identity
        );

        NetworkObject networkObject =
            square.GetComponent<NetworkObject>();

        networkObject.Spawn();

        Debug.Log("NetworkSquare spawned successfully!");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= SpawnSquare;
        }
    }
}
