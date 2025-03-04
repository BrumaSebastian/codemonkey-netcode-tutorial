using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyManagerUI : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Transform searchLobbyContainer;
    [SerializeField] private GameObject lobbyPrefab;

    private readonly List<GameObject> lobbies = new();

    private void Awake()
    {
        createButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.CreatePublicLobby($"Lobby of: {AuthenticationService.Instance.PlayerName}");
            gameObject.SetActive(false);
        });

        quickJoinButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.QuickJoinLobby();
        });

        refreshButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.RefreshLobbies();
        });

        joinButton.onClick.AddListener(() =>
        {
            //var go = EventSystem.current.currentSelectedGameObject;

            //if (go != null && go.TryGetComponent<Selectable>(out var selectable) && selectable != null)
            //{
            //    if (go.TryGetComponent<LobbyTemplateUI>(out var lobbyTemplateUI))
            //    {
            //        Debug.Log("Joining Lobby: " + lobbyTemplateUI.LobbyId);
            //        await LobbyManager.Instance.JoinLobby(lobbyTemplateUI.LobbyId);
            //    }
            //}
        });
    }

    private void Start()
    {
        gameObject.SetActive(false);
        LobbyManager.Instance.OnAuthenticated += LobbyManager_OnAuthenticated;
        LobbyManager.Instance.OnLobbyRefresh += LobbyManager_OnLobbyRefresh;
        LobbyManager.Instance.OnPlayerLeftLobby += LobbyManager_OnLeaveLobby;
        LobbyManager.Instance.OnJoinLobby += LobbyManager_OnJoinLobby;
    }

    private void LobbyManager_OnJoinLobby(object sender, Lobby e)
    {
        gameObject.SetActive(false);
    }

    private void LobbyManager_OnAuthenticated(object sender, EventArgs e)
    {
        gameObject.SetActive(true);
    }

    private void LobbyManager_OnLeaveLobby(object sender, EventArgs e)
    {
        gameObject.SetActive(true);
    }

    private void LobbyManager_OnLobbyRefresh(object sender, List<Lobby> lobbies)
    {
        ClearLobbies();
        RefreshLobbies(lobbies);
    }

    private void SetupLobby(Lobby lobby)
    {
        var lobbyGameObject = Instantiate(lobbyPrefab, searchLobbyContainer);
        var lobbyTemplate = lobbyGameObject.GetComponent<LobbyTemplateUI>();
        lobbyTemplate.SetData(lobby);
        lobbies.Add(lobbyGameObject);
    }

    private void RefreshLobbies(List<Lobby> lobbies)
    {
        foreach (Lobby lobby in lobbies)
        {
            SetupLobby(lobby);
        }
    }

    private void ClearLobbies()
    {
        foreach (GameObject lobby in lobbies)
        {
            Destroy(lobby);
        }

        lobbies.Clear();
    }
}
