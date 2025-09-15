using UnityEngine;

public class Board_AI : MonoBehaviour
{
    public int N = 15;
    private StoneState[,] board;

    public Board_AI(int N)
    {
        board = new StoneState[N, N];
        for (int i = 0; i < N; ++i)
            for (int j = 0; j < N; ++j)
                board[i, j] = StoneState.Empty;
    }

    public bool IsEmpty(int x, int y)
    {
        if (board[x, y] == StoneState.Empty) return true;
        else return false;
    }

    public bool TryPlace(int x, int y, StoneState curStone)
    {
        if (!IsEmpty(x, y)) return false;
        board[x, y] = curStone;
        return true;
    }
}
