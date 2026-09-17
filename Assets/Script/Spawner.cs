using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player Settings")]
    public GameObject playerPrefab; // must be in Resources folder
    private int playerId;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    //this is for the owner of the server (player who created the game)
    public void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            StartCoroutine(DelaySpawn());
        }
        else
        {
            Debug.LogWarning("Not connected to Photon or not in a room yet.");
        }
    }

    IEnumerator DelaySpawn()
    {
        yield return new WaitForSeconds(0.2f); // wait a frame or two
        SpawnPlayer();
        Debug.Log("Spawned player: " + playerId);
    }

    //this is accessed by players who joined the server
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room, spawning player...");
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is missing in inspector!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        // Prevent double-spawning if the player already exists
        if (PhotonNetwork.LocalPlayer.TagObject != null)
        {
            Debug.Log("Player already spawned, skipping.");
            return;
        }

        playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("playerID: " + playerId);

        Transform spawnLocation; //this gets the position of location transform

        if (playerId == 1)
        {
            spawnLocation = spawnPoints[0];
        }
        else if (playerId == 2 && spawnPoints.Length > 1)
        {
            spawnLocation = spawnPoints[1];
        }
        else
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            spawnLocation = spawnPoints[randomIndex];
        }

        // The prefab MUST be in a folder called "Resources"
        // Instantiate networked player
        GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation.position, spawnLocation.rotation);

        // Store a reference so Photon knows this player exists
        PhotonNetwork.LocalPlayer.TagObject = newPlayer;

        Debug.Log("Spawned player " + playerId + " at " + spawnLocation.name);
    }
}