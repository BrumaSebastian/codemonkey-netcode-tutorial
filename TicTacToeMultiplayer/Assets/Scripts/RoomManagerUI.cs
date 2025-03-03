using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class RoomManagerUI : MonoBehaviour
{
    const string PLAYER_NAME = "PlayerName";

    [SerializeField] private Transform playersContainer;
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private GameObject playerLobbyPrefab;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button startGameButton;

    private readonly List<GameObject> playersGameObject = new();
    private Lobby lobby;

    private void Awake()
    {
        leaveButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.LeaveLobby(lobby.Id);
            gameObject.SetActive(false);
        });

        startGameButton.onClick.AddListener( () =>
        {
            LobbyManager.Instance.StartGame(lobby);
            gameObject.SetActive(false);
        });
    }

    private void Start()
    {
        gameObject.SetActive(false);
        LobbyManager.Instance.OnLobbyCreated += LobbyManager_OnLobbyJoined;
        LobbyManager.Instance.OnQuickJoin += LobbyManager_OnLobbyJoined;
        LobbyManager.Instance.OnLobbyPlayersChange += LobbyManager_OnLobbyPlayersChangeAsync;
    }

    private async void LobbyManager_OnLobbyPlayersChangeAsync(object sender, System.EventArgs e)
    {
        lobby = await LobbyManager.Instance.GetLobbyData(lobby.Id);
        Debug.Log($"on lobby changes: {lobby.HostId}");
        ClearPlayersInLobby();
        RefreshPlayers();
        VerifyHost();
    }

    private void LobbyManager_OnLobbyJoined(object sender, Lobby lobby)
    {
        Debug.Log($"On joined host:{lobby.HostId}");
        this.lobby = lobby;
        lobbyName.text = lobby.Name;
        ClearPlayersInLobby();
        RefreshPlayers();
        VerifyHost();
        gameObject.SetActive(true);
    }

    private void SetupPlayer(string playerName, bool isHost = false)
    {
        var playerLobby = Instantiate(playerLobbyPrefab, playersContainer);
        var playerLobbyTemplate = playerLobby.GetComponent<PlayerLobbyTemplateUI>();
        playerLobbyTemplate.SetData(playerName, isHost);
        playersGameObject.Add(playerLobby);
    }

    private void RefreshPlayers()
    {
        foreach (var player in lobby.Players)
        {
            bool isHost = lobby.HostId == player.Id;
            SetupPlayer(player.Data[PLAYER_NAME].Value, isHost);
        }
    }

    private void ClearPlayersInLobby()
    {
        foreach (var gameObject in playersGameObject)
        {
            Destroy(gameObject);
        }

        playersGameObject.Clear();
    }

    private void VerifyHost()
    {
        if (AuthenticationService.Instance.PlayerId == lobby.HostId)
        {
            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }
    }
}
