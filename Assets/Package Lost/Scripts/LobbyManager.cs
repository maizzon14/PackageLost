using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviour
{
    private const string GameSceneName = "MainScene";
    public static LobbyManager Instance;
    public bool started = false;

    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            StartGame();
        }
    }

    // Starts the main game but only works for the host
    public void StartGame()
    {

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only the host/server can initiate the game.");
            started = false;
            return;
        }

        // Syncronize all clients.
        SceneEventProgressStatus status = NetworkManager.Singleton.SceneManager.LoadScene(
            GameSceneName,
            LoadSceneMode.Single
        );
        started = true;

        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogError($"Error trying to load the scene: {status}");
        }
    }
}
