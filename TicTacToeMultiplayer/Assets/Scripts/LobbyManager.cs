using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public const string PLAYER_NAME = "PlayerName";
    public const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

    const int MAX_PLAYERS_IN_LOBBY = 2;

    public static LobbyManager Instance { get; private set; }

    public event EventHandler OnAuthenticated;
    public event EventHandler<Lobby> OnLobbyCreated;
    public event EventHandler<Lobby> OnQuickJoin;
    public event EventHandler<List<Lobby>> OnLobbyRefresh;
    public event EventHandler OnLobbyPlayersChange;
    public event EventHandler OnPlayerLeftLobby;
    public event EventHandler<Lobby> OnGameStart;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("N-are cum");
        }

        Instance = this;
    }

    public async Task AuthenticateAnonymously(string playerName)
    {
        try
        {
            InitializationOptions initializationOptions = new();
            initializationOptions.SetProfile(playerName);
            await UnityServices.InitializeAsync(initializationOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
            OnAuthenticated?.Invoke(this, EventArgs.Empty);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task CreatePublicLobby(string lobbyName)
    {
        var playerData = new Dictionary<string, PlayerDataObject>
        {
            { PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerName) }
        };

        CreateLobbyOptions options = new()
        {
            IsPrivate = false,
            Player = new Player(
                id: AuthenticationService.Instance.PlayerId, 
                data: playerData),
            Data = new Dictionary<string, DataObject>()
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYERS_IN_LOBBY, options);

        LobbyEventCallbacks callbacks = new();
        callbacks.PlayerJoined += LobbyCallbacks_PlayerJoined;
        callbacks.PlayerLeft += LobbyCallbacks_PlayerLeft;
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);

        StartCoroutine(HeartbeatLobby(lobby.Id, 15));

        Debug.Log($"Lobby created {lobby.Created} - host - {lobby.Players[0].Data[PLAYER_NAME].Value}");

        OnLobbyCreated?.Invoke(this, lobby);
    }

    private void LobbyCallbacks_PlayerLeft(List<int> obj)
    {
        Debug.Log($"Player left: {obj[0]}");
        OnLobbyPlayersChange?.Invoke(this, EventArgs.Empty);
    }

    private void LobbyCallbacks_PlayerJoined(List<LobbyPlayerJoined> obj)
    {
        OnLobbyPlayersChange?.Invoke(this, EventArgs.Empty);
    }

    public async Task QuickJoinLobby()
    {
        try
        {
            var playerData = new Dictionary<string, PlayerDataObject>
            {
                { PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerName) }
            };

            QuickJoinLobbyOptions options = new()
            {
                Player = new Player(
                    id: AuthenticationService.Instance.PlayerId,
                    data: playerData)
            };

            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            OnQuickJoin?.Invoke(this, lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task JoinLobby(string lobbyId)
    {
        try
        {
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions()
            {
                Count = 5,
                Filters = new List<QueryFilter>()
                {
                    //new QueryFilter(
                    //    field: QueryFilter.FieldOptions.AvailableSlots,
                    //    op: QueryFilter.OpOptions.GT,
                    //    value: "0")
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(
                        asc: false,
                        field: QueryOrder.FieldOptions.Created)
                }
            };

            var lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            OnLobbyRefresh?.Invoke(this, lobbies.Results);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task LeaveLobby(string lobbyId)
    {
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
            Debug.Log($"Player {AuthenticationService.Instance.PlayerName} left lobby {lobbyId}");
            OnPlayerLeftLobby?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async Task<Lobby> GetLobbyData(string lobbyId)
    {
        return await LobbyService.Instance.GetLobbyAsync(lobbyId);
    }

    private System.Collections.IEnumerator HeartbeatLobby(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSeconds(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public void StartGame(Lobby lobby)
    {
        OnGameStart?.Invoke(this, lobby);
    }
}
