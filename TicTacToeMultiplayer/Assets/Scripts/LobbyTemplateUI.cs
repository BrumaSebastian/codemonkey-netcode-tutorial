using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyTemplateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI lobbySlots;

    public string LobbyId { get; private set; }
    public bool IsSelected { get; private set; }

    public void SetData(Lobby e)
    {
        Debug.Log($"Lobby set data: {e.AvailableSlots} {e.MaxPlayers}");
        lobbyName.text = e.Name;
        lobbySlots.text = $"{e.Players.Count}/{e.MaxPlayers}";
        LobbyId = e.Id;
    }

    //public void OnSelect(BaseEventData eventData)
    //{
    //    Debug.Log($"selected {LobbyId}");
    //    IsSelected = true;
    //}

    //public void OnDeselect(BaseEventData eventData)
    //{
    //    Debug.Log($"deselected {LobbyId}");
    //    IsSelected = false;
    //}
}
