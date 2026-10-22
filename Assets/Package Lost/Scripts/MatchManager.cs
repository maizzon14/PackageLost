
using UnityEngine;
using Unity.Netcode;

public class MatchManager : MonoBehaviour
{
    private const string GameSceneName = "MainScene";
    void Start()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvents;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvents;
        }
    }

    // Handles when the clients are ready
    private void HandleSceneEvents(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType)
        {
            // When each client and sever have completed the scene load
            case SceneEventType.LoadComplete:
                Debug.Log($"The client {sceneEvent.ClientId} has finished loading the scene {sceneEvent.SceneName}.");
                break;

            // Only fires on server side when ALL connected clients have finished loading the scene
            case SceneEventType.LoadEventCompleted:
                if (NetworkManager.Singleton.IsServer && sceneEvent.SceneName == GameSceneName)
                {
                    Debug.Log("All players have loaded the scene. The game can begin.");
                }
                break;
        }
    }
}
