using UnityEngine;

public class Board_AI : MonoBehaviour
{
    public int N;
    int empty = 15 * 15;
    private StoneState[,] board;

    int[,] directions = { { 1, 0 }, { 0, 1 }, { 1, 1 }, { -1, 1 } };

    public Board_AI(int N)
    {
        this.N = N;
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

    public bool IsInRange(int x, int y)
    {
        return (x >= 0 && y >= 0 && x < N && y < N);
    }

    public bool TryPlace(int x, int y, StoneState curStone)
    {
        if (!IsEmpty(x, y)) return false;
        board[x, y] = curStone;
        empty -= 1;
        return true;
    }

    public bool IsFive(int x, int y, StoneState curStone)
    {
        for(int i = 0; i < directions.Length; ++i)
        {
            int count = 1;//놓은 자리 포함

            int dx = directions[i, 0];
            int dy = directions[i, 1];

            // 정 방향
            int nx = x + dx;
            int ny = y + dy;
            while(IsInRange(nx,ny) && board[nx,ny] == curStone)
            {
                count += 1;
                nx += dx;
                ny += dy;
            }

            //역 방향
            nx = x - dx;
            ny = y - dy;
            while(IsInRange(nx,ny) && board[nx,ny] == curStone)
            {
                count += 1;
                nx -= dx;
                ny -= dy;
            }

            if (count == 5) return true; //딱 5일때만 6목은 배제
        }
        return false;
    }


    public bool IsFull()
    {
        if (empty != 0) return false;
        return true;
    }
}
