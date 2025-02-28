using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private PlayerType localPlayerType;
    private PlayerType currentPlayablePlayerType;

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedGridPosition;

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager instance!");
        }

        Instance = this;

    }

    public override void OnNetworkSpawn()
    {
        localPlayerType = NetworkManager.Singleton.LocalClientId == 0 ? PlayerType.Cross : PlayerType.Circle;

        if (IsServer)
        {
            currentPlayablePlayerType = PlayerType.Cross;
        }
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType)
        {
            return;
        }

        OnClickedGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType
        });

        switch (currentPlayablePlayerType)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType = PlayerType.Cross;
                break;
        }
    }

    public PlayerType GetLocalPlayerType() => localPlayerType;
}
