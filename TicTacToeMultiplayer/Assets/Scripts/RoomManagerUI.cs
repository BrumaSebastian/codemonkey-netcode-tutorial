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
        });

        startGameButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.StartGame();
        });
    }

    private void Start()
    {
        gameObject.SetActive(false);
        LobbyManager.Instance.OnLobbyCreated += LobbyManager_OnLobbyJoined;
        LobbyManager.Instance.OnJoinLobby += LobbyManager_OnLobbyJoined;
        LobbyManager.Instance.OnLobbyPlayersChange += LobbyManager_OnLobbyPlayersChangeAsync;
        LobbyManager.Instance.OnGameStarted += LobbyManager_OnGameStarted;
        LobbyManager.Instance.OnPlayerLeftLobby += LobbyManager_OnPlayerLeftLobby;
        LobbyManager.Instance.OnHostStartGame += LobbyManager_OnHostStartGame;
    }

    private void LobbyManager_OnHostStartGame(object sender, Lobby e)
    {
        HideUI();
    }

    private void LobbyManager_OnPlayerLeftLobby(object sender, System.EventArgs e)
    {
        HideUI();
    }

    private void LobbyManager_OnGameStarted(object sender, string e)
    {
        HideUI();
    }

    private void LobbyManager_OnLobbyPlayersChangeAsync(object sender, Lobby lobby)
    {
        ClearPlayerObjects();
        RefreshPlayers(lobby);
        RefreshUI(lobby);
    }

    private void LobbyManager_OnLobbyJoined(object sender, Lobby lobby)
    {
        Debug.Log($"On joined host:{lobby.HostId}");
        lobbyName.text = lobby.Name;
        ClearPlayerObjects();
        RefreshPlayers(lobby);
        RefreshUI(lobby);
        ShowUI();
    }

    private void RefreshPlayers(Lobby lobby)
    {
        foreach (var player in lobby.Players)
        {
            SetupPlayer(player.Data[LobbyManager.PLAYER_NAME].Value, player.Id, lobby.HostId == player.Id);
        }
    }

    private void RefreshUI(Lobby lobby)
    {
        startGameButton.gameObject.SetActive(IsHost(lobby));

        if (IsHost(lobby))
        {
            foreach (var playerGameObject in playersGameObject)
            {
                var playerLobbyTemplate = playerGameObject.GetComponent<PlayerLobbyTemplateUI>();
                playerLobbyTemplate.SetKickButtonVisibility(playerLobbyTemplate.PlayerId != lobby.HostId);
            }
        }
    }

    private void SetupPlayer(string playerName, string playerId, bool isHost = false)
    {
        var playerLobby = Instantiate(playerLobbyPrefab, playersContainer);
        var playerLobbyTemplate = playerLobby.GetComponent<PlayerLobbyTemplateUI>();
        playerLobbyTemplate.SetData(playerName, playerId, isHost);
        playersGameObject.Add(playerLobby);
    }

    private void ClearPlayerObjects()
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

    private void ShowUI()
    {
        gameObject.SetActive(true);
    }

    private void HideUI()
    {
        gameObject.SetActive(false);
    }
}
