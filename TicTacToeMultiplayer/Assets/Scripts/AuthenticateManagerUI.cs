using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateManagerUI : MonoBehaviour
{
    [SerializeField] private Button authenticateButton;
    [SerializeField] private TMP_InputField nameInput;

    private void Awake()
    {
        authenticateButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.AuthenticateAnonymously(nameInput.text);
            gameObject.SetActive(false);
        });
    }
}
