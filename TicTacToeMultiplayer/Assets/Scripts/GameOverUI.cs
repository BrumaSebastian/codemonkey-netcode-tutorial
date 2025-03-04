using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTestMesh;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private Color tiedColor;
    [SerializeField] private Button rematchButton;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RematchRpc();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
        GameManager.Instance.OnGameTied += GameManager_OnGameTied;
        Hide();
    }

    private void GameManager_OnGameTied(object sender, System.EventArgs e)
    {
        resultTestMesh.text = "TIED";
        resultTestMesh.color = tiedColor;
        Show();
    }

    private void GameManager_OnRematch(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.Instance.GetLocalPlayerType())
        {
            resultTestMesh.text = "YOU WIN!";
            resultTestMesh.color = winColor;
        }
        else
        {
            resultTestMesh.text = "YOU LOSE!";
            resultTestMesh.color = loseColor;
        }

        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
