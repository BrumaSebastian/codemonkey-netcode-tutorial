using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class StartGameManager : MonoBehaviour
{
    private void Start()
    {
        LobbyManager.Instance.OnHostStartGame += LobbyManager_OnHostStartGame;
        LobbyManager.Instance.OnGameStarted += LobbyManager_OnGameStarted;
    }

    private async void LobbyManager_OnGameStarted(object sender, string relayJoinCode)
    {
        Debug.Log("client ");
        await StartClientWithRelay(relayJoinCode);
    }

    private async void LobbyManager_OnHostStartGame(object sender, Lobby lobby)
    {
        string joinCode = await StartHostWithRelay();

        Debug.Log("host relay code" + joinCode);

        await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject> {
                { LobbyManager.IS_GAME_STARTED, new DataObject(DataObject.VisibilityOptions.Public, "1") },
                { LobbyManager.KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
            }
        });
    }

    private async Task<string> StartHostWithRelay(int maxConnections = 3)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
