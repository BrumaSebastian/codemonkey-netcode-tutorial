using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyTemplateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI lobbySlots;

    public void SetData(Lobby e)
    {
        Debug.Log($"Lobby set data: {e.AvailableSlots} {e.MaxPlayers}");
        lobbyName.text = e.Name;
        lobbySlots.text = $"{e.AvailableSlots}/{e.MaxPlayers}";
    }
}
