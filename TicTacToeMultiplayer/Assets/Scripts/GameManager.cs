using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private PlayerType localPlayerType;
    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;

    private NetworkVariable<PlayerType> currentPlayablePlayerType = new();

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedGridPosition;
    public event EventHandler OnGameStarted;
    public event EventHandler OnPlayablePlayerTypeChanged;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager instance!");
        }

        Instance = this;
        playerTypeArray = new PlayerType[3, 3];
        lineList = new List<Line>() {
            // Horizontal
            new Line
            {
                gridVector2IntList = new()
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(2, 0)
                },
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList = new()
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal,

            },
            new Line
            {
                gridVector2IntList = new()
                {
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 2),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal,
            },
            // Vertical
            new Line
            {
                gridVector2IntList = new()
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2)
                },
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical,
            },
            new Line
            {
                gridVector2IntList = new()
                {
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 2)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical,
            },
            new Line
            {
                gridVector2IntList = new()
                {
                    new Vector2Int(2, 0),
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical,
            },
            // Diagonals
            new Line
            {
                gridVector2IntList = new()
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA,
            },
            new Line
            {
                gridVector2IntList = new()
                {
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 0),
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB,
            },
        };
    }

    public override void OnNetworkSpawn()
    {
        localPlayerType = NetworkManager.Singleton.LocalClientId == 0 ? PlayerType.Cross : PlayerType.Circle;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.ConnectedClientsList.Count == 2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggetOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggetOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType.Value)
        {
            return;
        }

        if (playerTypeArray[x, y] != PlayerType.None)
        {
            return;
        }
        
        playerTypeArray[x, y] = playerType;

        OnClickedGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType
        });

        switch (currentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

        TestWinner();
    }

    private void TestWinner()
    {
        foreach (var line in lineList)
        {
            if (TestWinnerLine(line))
            {
                OnGameWin?.Invoke(this, new OnGameWinEventArgs
                {
                    line = line
                });
                currentPlayablePlayerType.Value = PlayerType.None;

                break;
            }
        }
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
        );
    }

    private bool TestWinnerLine(PlayerType a, PlayerType b, PlayerType c)
    {
        return a != PlayerType.None && a == b && b == c;
    }

    public PlayerType GetLocalPlayerType() => localPlayerType;
    public PlayerType GetCurrentPlayablePlayerType() => currentPlayablePlayerType.Value;
}
