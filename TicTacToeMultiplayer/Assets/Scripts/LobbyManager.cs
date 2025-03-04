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
    public const string IS_GAME_STARTED = "IsGameStarted";

    const int MAX_PLAYERS_IN_LOBBY = 2;

    public static LobbyManager Instance { get; private set; }

    public event EventHandler OnAuthenticated;
    public event EventHandler<Lobby> OnLobbyCreated;
    public event EventHandler<Lobby> OnJoinLobby;
    public event EventHandler<List<Lobby>> OnLobbyRefresh;
    public event EventHandler<Lobby> OnLobbyPlayersChange;
    public event EventHandler OnPlayerLeftLobby;
    public event EventHandler<Lobby> OnHostStartGame;
    public event EventHandler<string> OnGameStarted;

    private Lobby joinedLobby;

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
            Data = new Dictionary<string, DataObject>
            {
                {  IS_GAME_STARTED, new DataObject(DataObject.VisibilityOptions.Public, "0") },
                {  KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, string.Empty) }
            }
        };

        joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYERS_IN_LOBBY, options);
        await SubscribeToLobbyCallbacks();
        StartCoroutine(HeartbeatLobby(joinedLobby.Id, 15));
        OnLobbyCreated?.Invoke(this, joinedLobby);
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

            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            await SubscribeToLobbyCallbacks();
            OnJoinLobby?.Invoke(this, joinedLobby);
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
            var playerData = new Dictionary<string, PlayerDataObject>
            {
                { PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerName) }
            };

            JoinLobbyByIdOptions options = new()
            {
                Player = new Player(
                    id: AuthenticationService.Instance.PlayerId,
                    data: playerData)
            };

            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            await SubscribeToLobbyCallbacks();
            OnJoinLobby?.Invoke(this, joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task RefreshLobbies()
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

    public async Task LeaveLobby()
    {
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            Debug.Log($"Player {AuthenticationService.Instance.PlayerName} left lobby {joinedLobby.Id}");
            OnPlayerLeftLobby?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async Task KickPlayer(string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            OnPlayerLeftLobby?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
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

    public void StartGame()
    {
        OnHostStartGame?.Invoke(this, joinedLobby);
    }

    private bool IsHost()
    {
        return AuthenticationService.Instance.PlayerId == joinedLobby.HostId;
    }

    #region Lobby Callbacks
    private void LobbyCallbacks_LobbyChanged(ILobbyChanges changes)
    {
        changes.ApplyToLobby(joinedLobby);

        if (changes.PlayerLeft.Changed || changes.PlayerJoined.Changed)
        {
            Debug.Log("Players in lobby are updated");
            OnLobbyPlayersChange?.Invoke(this, joinedLobby);
        }

        if (!IsHost() && changes.Data.Changed && IsLobbyDataValueChanged(IS_GAME_STARTED, "1", changes.Data.Value))
        {
            Debug.Log("Client game should start");
            OnGameStarted?.Invoke(this, joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value);
        }
    }

    private void LobbyCallbacks_KickedFromLobby()
    {
        OnLobbyPlayersChange?.Invoke(this, joinedLobby);

        gameObject.SetActive(true);
    }
    #endregion

    private bool IsLobbyDataValueChanged(string key, string expectedValue, Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> lobbyData)
    {
        return lobbyData.ContainsKey(key)
            && lobbyData[key].ChangeType == LobbyValueChangeType.Changed
            && lobbyData[key].Value.Value == expectedValue;
    }

    private async Task SubscribeToLobbyCallbacks()
    {
        LobbyEventCallbacks callbacks = new();
        callbacks.LobbyChanged += LobbyCallbacks_LobbyChanged;
        callbacks.KickedFromLobby += LobbyCallbacks_KickedFromLobby;
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, callbacks);
    }

}
