using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSfxPrefab;
    [SerializeField] private Transform winSfxPrefab;
    [SerializeField] private Transform loseSfxPrefab;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject += GameManager_OnPlacedObject;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        Transform sfxTransform = GameManager.Instance.GetLocalPlayerType() == e.winPlayerType 
            ? Instantiate(winSfxPrefab)
            : Instantiate(loseSfxPrefab);

        Destroy(sfxTransform.gameObject, 5f);
    }

    private void GameManager_OnPlacedObject(object sender, System.EventArgs e)
    {
        Transform sfxTransform = Instantiate(placeSfxPrefab);

        Destroy(sfxTransform.gameObject, 5f);
    }
}
