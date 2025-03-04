using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyTemplateUI : MonoBehaviour
{
    [SerializeField] private Image frontImage;
    [SerializeField] private TextMeshProUGUI nameInput;
    [SerializeField] private Button kickButton;

    private string playerId;

    private void Awake()
    {
        kickButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.KickPlayer(playerId);
        });
    }

    public void SetData(string playerName, string playerId, bool isHost)
    {
        nameInput.text = playerName;
        frontImage.gameObject.SetActive(isHost);
        this.playerId = playerId;
    }

    public void SetKickButtonVisible(bool isVisible)
    {
        kickButton.gameObject.SetActive(isVisible);
    }
}
