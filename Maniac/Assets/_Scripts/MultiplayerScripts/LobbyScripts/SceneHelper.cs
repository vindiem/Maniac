using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using _Scripts.MultiplayerScripts.LobbyScripts;

public class SceneHelper : MonoBehaviourPunCallbacks
{
    public static SceneHelper Instance;

    void Awake()
    {
        if(Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.name == "Level1" || scene.name == "Level2") // We're in the game scene
        {
            RoomManager roomManager = FindObjectOfType<RoomManager>();
            roomManager.SpawnPlayer();
        }
    }
}