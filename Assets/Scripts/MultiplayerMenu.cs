using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class MultiplayerMenu : MonoBehaviour
{
    private void Update()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsServer &&
            Keyboard.current != null &&
            Keyboard.current.cKey.wasPressedThisFrame)
        {
            Debug.Log(
                "Connected clients: " +
                NetworkManager.Singleton.ConnectedClientsIds.Count
            );
        }
    }
    public void HostGame()
    {
        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            Debug.Log("Host started successfully!");
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }
    }

    public void JoinGame()
    {
        bool started = NetworkManager.Singleton.StartClient();

        if (started)
        {
            Debug.Log("Client connection attempt started.");
        }
        else
        {
            Debug.LogError("Failed to start client.");
        }
    }
}
