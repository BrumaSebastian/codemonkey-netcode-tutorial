using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class RoomManagerUI : MonoBehaviour
{
    const string PLAYER_NAME = "PlayerName";

    [SerializeField] private Transform playersContainer;
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private GameObject playerLobbyPrefab;

    private List<Player> players = new(); 
    private List<GameObject> playersGameObject = new();

    private void Start()
    {
        gameObject.SetActive(false);
        LobbyManager.Instance.OnLobbyCreated += LobbyManager_OnLobbyCreated;
        LobbyManager.Instance.OnQuickJoin += LobbyManager_OnQuickJoin;
        LobbyManager.Instance.OnPlayerJoined += LobbyManager_OnPlayerJoined;
    }

    private void LobbyManager_OnPlayerJoined(object sender, Player player)
    {
        Debug.Log("New player joined " + player.Id);
        players.Add(player);
        RefreshPlayersInLobby();
    }

    private void LobbyManager_OnQuickJoin(object sender, Lobby lobby)
    {
        lobbyName.text = lobby.Name;

        Debug.Log("Players" + lobby.Players.Count);

        foreach (Player player in lobby.Players)
        {
            SetupPlayer(player.Data[PLAYER_NAME].Value, isHost: lobby.HostId == player.Id);
        }

        gameObject.SetActive(true);
    }

    private void LobbyManager_OnLobbyCreated(object sender, Lobby lobby)
    {
        lobbyName.text = lobby.Name;
        SetupPlayer(lobby.Players[0].Data[PLAYER_NAME].Value, true);

        gameObject.SetActive(true);
    }

    private void SetupPlayer(string playerName, bool isHost = false)
    {
        var playerLobby = Instantiate(playerLobbyPrefab, playersContainer);
        var playerLobbyTemplate = playerLobby.GetComponent<PlayerLobbyTemplateUI>();
        playerLobbyTemplate.SetData(playerName, isHost);
        playersGameObject.Add(playerLobby);
    }

    private void RefreshPlayersInLobby()
    {
        foreach (var player in players)
        {
            SetupPlayer(player.Data[PLAYER_NAME].Value);
        }

        playersGameObject.Clear();
    }

    private void ClearPlayersInLobby()
    {
        foreach (var item in playersGameObject)
        {
            Destroy(item);
        }

        playersGameObject.Clear();
    }
}
