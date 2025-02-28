using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : MonoBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;

    private void Start()
    {
        GameManager.Instance.OnClickedGridPosition += GameManager_OnClickedOnGridPosition;
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        Transform spawnedCrossTransform = Instantiate(crossPrefab);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
        spawnedCrossTransform.position = GetGridWorldPosition(e.x, e.y);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE,-GRID_SIZE + y * GRID_SIZE);
    }
}
