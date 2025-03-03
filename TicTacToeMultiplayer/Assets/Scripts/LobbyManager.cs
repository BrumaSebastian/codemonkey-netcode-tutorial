using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    const string PLAYER_NAME = "PlayerName";

    public static LobbyManager Instance { get; private set; }

    public event EventHandler<Lobby> OnLobbyCreated;
    public event EventHandler<List<Lobby>> OnLobbyRefresh;
    public event EventHandler<string> OnAuthentication;
    public event EventHandler<Lobby> OnQuickJoin;
    public event EventHandler<Player> OnPlayerJoined;


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
            OnAuthentication?.Invoke(this, playerName);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task CreatePublicLobby(string lobbyName)
    {
        int maxPlayers = 2;

        var playerData = new Dictionary<string, PlayerDataObject>
        {
            { PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerName) }
        };

        CreateLobbyOptions options = new()
        {
            IsPrivate = false,
            Player = new Player(
                id: AuthenticationService.Instance.PlayerId, 
                data: playerData)
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        var callbacks = new LobbyEventCallbacks();

        callbacks.PlayerJoined += LobbyCallbacks_PlayerJoined;

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);

        Debug.Log($"Lobby created {lobby.Created} - host - {lobby.Players[0].Data[PLAYER_NAME].Value}");

        OnLobbyCreated?.Invoke(this, lobby);
    }

    private void LobbyCallbacks_PlayerJoined(List<LobbyPlayerJoined> obj)
    {
        var joinedPlayer = obj.First().Player;
        OnPlayerJoined?.Invoke(this, joinedPlayer);
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
            gameObject.SetActive(false);
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
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0")
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
}
