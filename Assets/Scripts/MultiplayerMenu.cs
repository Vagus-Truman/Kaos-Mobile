using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.SceneManagement;

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

    public void StartGame()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null || !networkManager.IsServer)
        {
            Debug.LogWarning("Only the host can start the game.");
            return;
        }

        SceneEventProgressStatus status =
            networkManager.SceneManager.LoadScene(
                "UI Sketch",
                LoadSceneMode.Single
            );

        if (status == SceneEventProgressStatus.Started)
        {
            Debug.Log("Gameplay scene transition started.");
        }
        else
        {
            Debug.LogWarning("Scene transition failed: " + status);
        }
    }

}
