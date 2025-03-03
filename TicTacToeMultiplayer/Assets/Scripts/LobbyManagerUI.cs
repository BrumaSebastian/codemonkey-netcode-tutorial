using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManagerUI : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Transform searchLobbyContainer;
    [SerializeField] private GameObject lobbyPrefab;

    private List<GameObject> lobbies = new();

    private void Awake()
    {
        createButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.CreatePublicLobby($"Lobby of: {AuthenticationService.Instance.PlayerName}");
            NetworkManager.Singleton.StartHost();
            gameObject.SetActive(false);
        });

        quickJoinButton.onClick.AddListener(async () =>
        {
            Debug.Log("Quick join button pressed");
            await LobbyManager.Instance.QuickJoinLobby();
            NetworkManager.Singleton.StartClient();
        });

        refreshButton.onClick.AddListener(async () =>
        {
            Debug.Log("Refresh lobby button pressed");
            await LobbyManager.Instance.RefreshLobbyList();
        });
    }

    private void Start()
    {
        gameObject.SetActive(false);
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        LobbyManager.Instance.OnLobbyCreated += LobbyManager_OnLobbyCreated;
        LobbyManager.Instance.OnLobbyRefresh += LobbyManager_OnLobbyRefresh;
        LobbyManager.Instance.OnAuthentication += LobbyManager_OnAuthentication;
    }

    private void LobbyManager_OnAuthentication(object sender, string playerId)
    {
        gameObject.SetActive(true);
    }

    private void LobbyManager_OnLobbyRefresh(object sender, List<Lobby> lobbies)
    {
        Debug.Log("Refresh lobbies" + lobbies.Count);

        ClearLobbyList();

        foreach (Lobby lobby in lobbies) 
        {
            SetupLobby(lobby);
        }
    }

    private void LobbyManager_OnLobbyCreated(object sender, Lobby lobby)
    {
        SetupLobby(lobby);
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        gameObject.SetActive(false);
    }

    private void SetupLobby(Lobby lobby)
    {
        var lobbyGameObject = Instantiate(lobbyPrefab, searchLobbyContainer);
        var lobbyTemplate = lobbyGameObject.GetComponent<LobbyTemplateUI>();
        lobbyTemplate.SetData(lobby);
        lobbies.Add(lobbyGameObject);
    }

    private void ClearLobbyList()
    {
        foreach (GameObject lobby in lobbies)
        {
            Object.Destroy(lobby);
        }

        lobbies.Clear();
    }
}
