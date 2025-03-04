using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyTemplateUI : MonoBehaviour
{
    [SerializeField] private Image frontImage;
    [SerializeField] private TextMeshProUGUI nameInput;
    [SerializeField] private Button kickButton;

    public string PlayerId { get; private set; }

    private void Awake()
    {
        kickButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.KickPlayer(PlayerId);
        });

        kickButton.gameObject.SetActive(false);
    }

    public void SetData(string playerName, string playerId, bool isHost)
    {
        nameInput.text = playerName;
        frontImage.gameObject.SetActive(isHost);
        PlayerId = playerId;
    }

    public void SetKickButtonVisibility(bool isVisible)
    {
        kickButton.gameObject.SetActive(isVisible);
    }
}
