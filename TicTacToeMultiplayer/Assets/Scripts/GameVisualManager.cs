using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineCompletePrefab;

    private void Start()
    {
        GameManager.Instance.OnClickedGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        float eulerZ = e.line.orientation switch
        {
            GameManager.Orientation.Horizontal => 0f,
            GameManager.Orientation.Vertical => 90f,
            GameManager.Orientation.DiagonalA => 45,
            GameManager.Orientation.DiagonalB => -45f,
            _ => 0f
        };

        Transform lineCompleteTransform = 
            Instantiate(lineCompletePrefab, GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y), Quaternion.Euler(0, 0, eulerZ));
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Transform prefab;

        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
                break;
            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
                break;
        }

        Transform spawnedCrossTransform = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE,-GRID_SIZE + y * GRID_SIZE);
    }
}
