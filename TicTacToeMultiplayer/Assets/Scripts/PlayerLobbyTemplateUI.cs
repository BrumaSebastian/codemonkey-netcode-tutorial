using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyTemplateUI : MonoBehaviour
{
    [SerializeField] private Image frontImage;
    [SerializeField] private TextMeshProUGUI nameInput;

    public void SetData(string playerName, bool isHost)
    {
        nameInput.text = playerName;
        frontImage.gameObject.SetActive(isHost);
    }
}
