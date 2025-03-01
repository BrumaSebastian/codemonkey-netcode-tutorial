using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTestMesh;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;

    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
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
