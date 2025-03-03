using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class StartGameManager : MonoBehaviour
{

    private void Start()
    {
        LobbyManager.Instance.OnGameStart += LobbyManager_OnGameStart;
    }

    private async void LobbyManager_OnGameStart(object sender, Lobby lobby)
    {
        if (AuthenticationService.Instance.PlayerId == lobby.HostId)
        {
            Debug.Log("host");

            string joinCode = await StartHostWithRelay();

            var updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                    { LobbyManager.KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
            });
        }
        else
        {
            Debug.Log("client");

            await StartClientWithRelay(lobby.Data[LobbyManager.KEY_RELAY_JOIN_CODE].Value);
        }
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
