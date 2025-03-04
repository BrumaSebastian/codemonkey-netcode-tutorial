using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTemplateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI lobbySlots;
    [SerializeField] private Button button;

    public string LobbyId { get; private set; }

    private void Awake()
    {
        button.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.JoinLobby(LobbyId);
        });
    }

    public void SetData(Lobby e)
    {
        Debug.Log($"Lobby set data: {e.AvailableSlots} {e.MaxPlayers}");
        lobbyName.text = e.Name;
        lobbySlots.text = $"{e.Players.Count}/{e.MaxPlayers}";
        LobbyId = e.Id;
    }
}
