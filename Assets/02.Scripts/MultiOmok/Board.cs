using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


// 렌주룰 (Renju-Rule)
// 흑돌이 33, 44, 장목의 위치에 돌을 놓지 못하게하는 룰

// 흑돌 금수 가능성 및 예외 (B = 흑돌, W = 백돌)

// 1)
//      X B B       => X 좌표는 33 금수
//    B B B W
//  B W B W B

// 2)
//        O B B ? => ? 좌표에 백돌이 1개 이상 있다면, O 좌표에 흑돌 착수 가능
//      B B B W
//    B W B W B
//  ? W W ? W W ?

// 3)
//  B W B W B
//  W B B B W
//  B B O B B   => O 좌표는 5목이 완성되어 흑돌 승리
//  W B B B W
//  B W B W B

// 4)
//  B W W B W W B
//  W B W B W B W
//  W W B B B W W
//  W B B X B B B  => X 좌표는 장목 금수
//  W W B B B W W
//  W B W B W B W

// 5)
//           X B B B  => X 좌표는 44 금수
//         B B B W W
//       B W B W B W
//     B W W B W W B

// 6)
//          X B B B ?  => ? 좌표에 백돌이 1개 있다면, X 좌표는 44 금수
//        B B B W W
//      B W B W B W
//    B W W B W W B
//  ? W W W ? W W W ?

// 7)
//          O B B B ?  => ? 좌표에 백돌이 2개 이상 있다면, O 좌표에 흑돌 착수 가능
//        B B B W W
//      B W B W B W
//    B W W B W W B
//  ? W W W ? W W W ?

public class Board
{
    // 팀원이 쉽게 렌주룰에 접근 가능하도록 싱글톤으로 만들어두었음!!
    public int N;
    private StoneState[,] board;

    // 현재 남아있는 빈칸 갯수
    int empty = 15 * 15;

    /* 8방향 탐색을 위한 백터 (상, 하, 좌, 우, 좌상, 우상, 우하, 좌하) */
    /* 상, 우상, 우, 우하 4개로 각각 -1을 곱해서 음수로 만들면 반대방향 백터 4개를 또 만들수 있으므로 4개만 선언하였음 */
    private readonly int[,] directions = 
    { 
        { 0, 1 }, 
        { 1, 1 }, 
        { 1, 0 }, 
        { 1, -1} 
    };

    /// <summary>
    /// BoardManager의 현제 행과 열의 상태를 가지고 클래스를 서로 동기화
    /// </summary>
    public Board(int N)
    {
        this.N = N;
        empty = N * N;
        board = new StoneState[N, N];
        for (int i = 0; i < N; ++i)
            for (int j = 0; j < N; ++j)
                board[i, j] = StoneState.Empty;
    }

    /// <summary>
    /// 정확히 5목이 되었는지 확인
    /// 승리 판정 함수 (가로세로, 대각선 체크 ==> 합산이 5면 승리)
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="Player"></param>
    /// <returns></returns>
    public bool CheckWin(int x, int y, StoneState player)
    {
        Vector2Int[] directions =
        {
            new (1, 0),
            new (0, 1),
            new (1, 1),
            new (-1, 1)
        };
        foreach (var dir in directions)
        {
            int count = 1;
            count += CountStones(x, y, dir.x, dir.y, player);
            count += CountStones(x, y, -dir.x, -dir.y, player);

            Debug.Log($"{count}");
            if (count == 5) return true;
        }
        return false;
    }

    /// <summary>
    /// 한 쪽 방향으로만 카운트를 세서 CountStones를 호출한 함수 CheckWin의 count를 올려줌 (총 2번)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public int CountStones(int x, int y, int dx, int dy, StoneState player)
    {
        int cnt = 0;
        int nx = x + dx;
        int ny = y + dy;

        //player돌이 이어진 동안, 이어졌는지 체크하면서 cnt++
        while (nx >= 0 && nx < 15 && ny >= 0 && ny < 15 && board[nx, ny] == player)
        {
            cnt++;
            nx += dx;
            ny += dy;
        }
        
        return cnt;
    }

    /// <summary>
    /// 흑돌의 33, 44, 장목(6목 이상) 금수 위치 지정
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public bool IsForbiddenBlackRock(int row, int col)
    {
        if (!IsWithinBounds(row, col) || board[row, col] != StoneState.Empty) // 보드 밖이거나, 빈자리가 아니라면 금수 체크 불필요
        {
            return false;
        }

        board[row, col] = StoneState.Black;  // 해당 좌표에 가상의 흑돌을 놓아 시뮬레이션

        if (IsCheckOverFive(row, col))      // 장목(6목 이상) 체크
        {
            return true;
        }

        /* 33, 44 체크 */
        int openThreeCount = 0; 
        int openFourCount = 0;

        for (int i = 0; i < 4; i++)
        {
            if (IsLine(row, col, directions[i, 0], directions[i, 1], StoneState.Black, 4, true)) // 열린 4인지 체크
            {
                openFourCount++;
            }

            if (IsLine(row, col, directions[i, 0], directions[i, 1], StoneState.Black, 3, true)) // 열린 3인지 체크
            {
                openThreeCount++;
            }
        }

        board[row, col] = StoneState.Empty;               // 가상으로 흑돌을 놓아본 시뮬을 다시 빈 공간으로 초기화

        if (openThreeCount >= 2 || openFourCount >= 2)  // 모두 열린 33 또는 한곳만 열린 44라면 금수
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 6목 이라면 true 반환 / 아니라면 false 반환
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    private bool IsCheckOverFive(int row, int col)
    {
        for (int i = 0; i < 4; i++)
        {
            int count = CountStonesInLine(row, col, directions[i, 0], directions[i, 1], StoneState.Black);
            if (count > 5)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 행열에 있는 흑돌의 연속된 길이를 length와 같은지 확인 | 
    /// 흑돌이 연속된 길이가 3,4라면 열린 3,4 인지 닫힌 3,4 확인
    /// </summary>
    /// <param name="row">행열의 특정 좌표</param>
    /// <param name="col">행열의 특정 좌표</param>
    /// <param name="dRow">방향 벡터를 위한 행</param>
    /// <param name="dCol">방향 벡터를 위한 열</param>
    /// <param name="player">흑돌인지 확인용</param>
    /// <param name="length">흑돌이 연속되어있는지를 위한 변수</param>
    /// <param name="isOpenCheck">연속된 흑돌의 길이가 열린 3,4인지 닫힌 3,4인지 확인용 매개변수</param>
    /// <returns></returns>
    private bool IsLine(int row, int col, int dRow, int dCol, StoneState player, int length, bool isOpenCheck)
    {
        // 특정 라인의 같은색 돌 개수
        int count = CountStonesInLine(row, col, dRow, dCol, player);
        if (count != length)
            return false;

        // 끝까지 확인할 필요없다면 여기서 반환
        if (!isOpenCheck)
            return true;

        // 나열된 같은색 돌이 열려있는지 판단
        int emptyEnds = 0;

        // 시작점 좌표
        int currentR = row;
        int currentC = col;
        // 끝의 다음칸이 보드 내부 일때
        // 상, 우상, 우, 우하 4개 방향의 같은색 돌 갯수 반복 검사 (조건이 맞다면 +해서 다음 좌표로 이동)
        while (IsWithinBounds(currentR + dRow, currentC + dCol) && board[currentR + dRow, currentC + dCol] == player)
        {
            currentR += dRow;
            currentC += dCol;
        }
        // 끝에 도달했다면 비어있는지 아닌지 검사 (비었다면 emptyEnds + 1 아니라면 그대로)
        if (IsWithinBounds(currentR + dRow, currentC + dCol) && board[currentR + dRow, currentC + dCol] == StoneState.Empty)
        {
            emptyEnds++;
        }

        // 시작점 좌표 리셋
        currentR = row;
        currentC = col;
        // 끝의 다음칸이 보드 내부 일때
        // 하, 좌하, 좌, 좌상 4개 방향의 같은색 돌 갯수 반복 검사 (조건이 맞다면 -해서 다음 좌표로 이동)
        while (IsWithinBounds(currentR - dRow, currentC - dCol) && board[currentR - dRow, currentC - dCol] == player)
        {
            currentR -= dRow;
            currentC -= dCol;
        }
        // 끝에 도달했다면 비어있는지 아닌지 검사 (비었다면 emptyEnds + 1 아니라면 그대로)
        if (IsWithinBounds(currentR - dRow, currentC - dCol) && board[currentR - dRow, currentC - dCol] == StoneState.Empty)
        {
            emptyEnds++;
        }

        // 만약 연속된 같은색 돌의 갯수가 3, 각 끝부분이 열려 있다면 true 반환(이 위치가 33 금수)
        if (length == 3 && emptyEnds == 2)
        {
            return true;
        }
        // 만약 연속된 같은색 돌의 갯수가 4, 끝부분이 한군대 이상 열려있다면 true 반환(이 위치가 44 금수)
        if (length == 4 && emptyEnds >= 1)
        {
            return true;
        }

        return false; // 위를 검사했을 때 모두 아니라면 금수가 아니므로 false반환
    }

    /// <summary>
    /// 8방향의 돌이 같은색인지 판별해서 count 변수에 저장해서 count 반환
    /// </summary>
    /// <param name="row">행열의 특정 좌표</param>
    /// <param name="col">행열의 특정 좌표</param>
    /// <param name="dRow">방향백터를 위한 행</param>
    /// <param name="dCol">방향백터를 위한 열</param>
    /// <param name="player">흑돌인지 확인용</param>
    /// <returns></returns>
    private int CountStonesInLine(int row, int col, int dRow, int dCol, StoneState player)
    {

        int count = 1;


        //상, 우상, 우, 우하 중 1개 방향에서 돌색이 같은 라인이 있다면 count++ 
        for (int i = 1; i < 6; i++)
        {
            int nRow = row + (dRow * i);
            int nCol = col + (dCol * i);

            if (IsWithinBounds(nRow, nCol) && board[nRow, nCol] == player)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        //하, 좌하, 좌, 좌상 중 1개 방향에서 돌색이 같은 라인이 있다면 count++ 
        for (int i = 1; i < 6; i++)
        {
            int nRow = row - (dRow * i);
            int nCol = col - (dCol * i);

            if (IsWithinBounds(nRow, nCol) && board[nRow, nCol] == player)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        return count;
    }

    /// <summary>
    /// 보드의 내부인지 판단
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    private bool IsWithinBounds(int row, int col)
    {
        return row >= 0 && row < N && col >= 0 && col < N;
    }


    public bool IsEmpty(int row, int col)
    {
        if (board[row, col] == StoneState.Empty) return true;
        return false;
    }

    public void Place(int row, int col, StoneState player)
    {
        if (IsWithinBounds(row, col) && IsEmpty(row, col))
        {
            board[row, col] = player;
            empty -= 1;
        }
    }

    public bool IsFull()
    {
        if (empty != 0) return false;
        return true;
    }

    public void ShowBoard()
    {
        string msg = "";
        for (int i = 0; i < N; ++i)
        {
            for (int j = 0; j < N; ++j)
            {
                if (board[i, j] == StoneState.Black) msg += "X ";
                if (board[i, j] == StoneState.White) msg += "O ";
                if (board[i, j] == StoneState.Empty) msg += "- ";
            }
            msg += '\n';
        }
        Debug.Log(msg);
    }
}
