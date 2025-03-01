using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObject;
    [SerializeField] private GameObject circleArrowGameObject;
    [SerializeField] private GameObject crossYouTextGameObject;
    [SerializeField] private GameObject circleYouTextGameObject;
    [SerializeField] private TextMeshProUGUI playerCrossScoreTextMesh;
    [SerializeField] private TextMeshProUGUI playerCircleScoreTextMesh;

    private void Awake()
    {
        crossArrowGameObject.SetActive(false);
        crossYouTextGameObject.SetActive(false);

        circleArrowGameObject.SetActive(false);
        circleYouTextGameObject.SetActive(false);

        playerCrossScoreTextMesh.text = "";
        playerCircleScoreTextMesh.text = "";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnPlayablePlayerTypeChanged += GameManager_OnPlayablePlayerTypeChanged;
        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
    }

    private void GameManager_OnScoreChanged(object sender, System.EventArgs e)
    {
        GameManager.Instance.GetScores(out int playerCrossScore, out int playerCicleScore);

        playerCrossScoreTextMesh.text = playerCrossScore.ToString();
        playerCircleScoreTextMesh.text = playerCicleScore.ToString();
    }

    private void GameManager_OnPlayablePlayerTypeChanged(object sender, System.EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            crossYouTextGameObject.SetActive(true);
        }
        else
        {
            circleYouTextGameObject.SetActive(true);
        }

        playerCrossScoreTextMesh.text = "0";
        playerCircleScoreTextMesh.text = "0";
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrowGameObject.SetActive(true);
            circleArrowGameObject.SetActive(false);
        }
        else
        {
            circleArrowGameObject.SetActive(true);
            crossArrowGameObject.SetActive(false);
        }
    }
}
