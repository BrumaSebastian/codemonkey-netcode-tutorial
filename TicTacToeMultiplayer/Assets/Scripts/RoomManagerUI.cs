using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class RoomManagerUI : MonoBehaviour
{
    [SerializeField] private Transform playersContainer;
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private GameObject playerLobbyPrefab;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button startGameButton;

    private readonly List<GameObject> playersGameObject = new();

    private void Awake()
    {
        leaveButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.LeaveLobby();
            gameObject.SetActive(false);
        });

        startGameButton.onClick.AddListener( () =>
        {
            LobbyManager.Instance.StartGame();
            gameObject.SetActive(false);
        });
    }

    private void Start()
    {
        gameObject.SetActive(false);
        LobbyManager.Instance.OnLobbyCreated += LobbyManager_OnLobbyJoined;
        LobbyManager.Instance.OnJoinLobby += LobbyManager_OnLobbyJoined;
        LobbyManager.Instance.OnLobbyPlayersChange += LobbyManager_OnLobbyPlayersChangeAsync;
        LobbyManager.Instance.OnGameStarted += LobbyManager_OnGameStarted;
    }

    private void LobbyManager_OnGameStarted(object sender, string e)
    {
        gameObject.SetActive(false);
    }

    private void LobbyManager_OnLobbyPlayersChangeAsync(object sender, Lobby lobby)
    {
        ClearPlayersInLobby();
        RefreshPlayers(lobby);
        startGameButton.gameObject.SetActive(IsHost(lobby));
    }

    private void LobbyManager_OnLobbyJoined(object sender, Lobby lobby)
    {
        Debug.Log($"On joined host:{lobby.HostId}");
        lobbyName.text = lobby.Name;
        ClearPlayersInLobby();
        RefreshPlayers(lobby);
        startGameButton.gameObject.SetActive(IsHost(lobby));
        gameObject.SetActive(true);
    }

    private void SetupPlayer(string playerName, bool isHost = false)
    {
        var playerLobby = Instantiate(playerLobbyPrefab, playersContainer);
        var playerLobbyTemplate = playerLobby.GetComponent<PlayerLobbyTemplateUI>();
        playerLobbyTemplate.SetData(playerName, isHost);
        playersGameObject.Add(playerLobby);
    }

    private void RefreshPlayers(Lobby lobby)
    {
        foreach (var player in lobby.Players)
        {
            bool isHost = lobby.HostId == player.Id;
            SetupPlayer(player.Data[LobbyManager.PLAYER_NAME].Value, isHost);
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

    private static bool IsHost(Lobby lobby)
    {
        return AuthenticationService.Instance.PlayerId == lobby.HostId;
    }
}
