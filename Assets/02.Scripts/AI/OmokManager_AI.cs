using UnityEngine;
using System.Collections;



public class OmokManager_AI : MonoBehaviour, IGameStateListener
{
    public static OmokManager_AI Instance;
    public int N = 15;

    public GameState curState;

    public Board_AI board;

    StoneState selectedStone;
    Player_AI player;
    OmokAI_AI ai;

    int turn = 0;

    public delegate void OnGameStateChanged(GameState state);
    public event OnGameStateChanged OnStateChanged;

    public delegate void OnBoardStateChanged(int x, int y, StoneState curStone);
    public event OnBoardStateChanged OnBoardChanged;

    public void SetState(GameState newState)
    {
        curState = newState;
        OnStateChanged?.Invoke(curState);
    }

    void Start()
    {
        board = new Board_AI(15);
    }

    void LoopStart()
    {
        SetState(GameState.None);
        StartCoroutine(nameof(GameLoop));
    }

    IEnumerator GameLoop()
    {
        while(curState == GameState.None)
        {
            if((StoneState)(turn%2) == selectedStone)
            {
                yield return player.Think(board, selectedStone, OnStone);
            }
            else
            {
                yield return ai.Think(board, (StoneState)(turn % 2), OnStone);
            }
            yield return null;
        }
    }

    void OnStone(int x, int y)
    {
        StoneState curStone = (StoneState)(turn % 2);
        if (!board.TryPlace(x, y, curStone)) return;

        OnBoardChanged?.Invoke(x, y, curStone);

        if (board.IsFive(x,y,curStone))
        {
            if(curStone == StoneState.White) SetState(GameState.WhiteWin);
            else SetState(GameState.BlackWin);
            return;
        }

        if(board.IsFull())
        {
            SetState(GameState.Draw);
            return;
        }

        turn++;
    }

}