using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("N-are cum");
        }

        Instance = this;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    public async Task AuthenticateAnonymously()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"PlayerId: {AuthenticationService.Instance.PlayerId}");
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task CreatePublicLobby()
    {
        string lobbyName = "new lobby";
        int maxPlayers = 4;

        CreateLobbyOptions options = new()
        {
            IsPrivate = false
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        Debug.Log($"Lobby created {lobby.Created} {lobby.Name}");
    }

    public async Task QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            options.Filter = new List<QueryFilter>()
            {
            };

            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            Debug.Log($"lobby {lobby.Players.Count}");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
