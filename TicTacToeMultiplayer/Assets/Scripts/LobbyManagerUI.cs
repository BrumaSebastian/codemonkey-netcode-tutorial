using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManagerUI : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button quickJoinButton;

    private void Awake()
    {
        createButton.onClick.AddListener(async () =>
        {
            Debug.Log("Create lobby button pressed");
            await LobbyManager.Instance.AuthenticateAnonymously();
            await LobbyManager.Instance.CreatePublicLobby();
            NetworkManager.Singleton.StartHost();
        });

        quickJoinButton.onClick.AddListener(async () =>
        {
            Debug.Log("Quick join button pressed");
            await LobbyManager.Instance.AuthenticateAnonymously();
            await LobbyManager.Instance.QuickJoinLobby();
            NetworkManager.Singleton.StartClient();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        gameObject.SetActive(false);
    }
}
