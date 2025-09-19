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
    /// 흑돌의 33, 44, 장목(6목 이상) 금수 위치 지정 (띈 3, 띈 4도 검사 가능케 수정되었음)
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public bool IsForbiddenMove(int row, int col)
    {
        // 빈칸 부분만 금수인지 판단해서 놓지 못하게 해야하므로 빈공간이 아니라면 함수 종료
        if (!IsWithinBounds(row, col) || board[row, col] != StoneState.Empty)
        {
            return false;
        }
        // 임의로 검은돌을 놓아봄 (시뮬)
        board[row, col] = StoneState.Black;

        int openThreecount = 0;
        int fourCount = 0;

        // 4가지 방향 검사 (가로, 세로, 대각선 2개)
        for (int i = 0; i < 4; i++)
        {
            int dRow = directions[i, 0];
            int dCol = directions[i, 1];

            int consecutiveStones = CountConsecutiveStones(row, col, dRow, dCol, StoneState.Black);


            // 오목인지 장목인지 먼저 검사 (5목이 되어버리면 승리, 흑돌이 장목이면 금수)
            if (consecutiveStones > 5)
            {
                board[row, col] = StoneState.Empty;
                return true;
            }
            if (consecutiveStones == 5)
            {
                board[row, col] = StoneState.Empty;
                return false;
            }

            // 현제의 방향에서 만들어질 수 있는 33, 44 패턴의 개수 검사 ( 44 먼저 검사후 33 검사 )
            char[] line = CreateDirectionLine(row, col, dRow, dCol);
            for (int start = 0; start <= line.Length - 6; start++)
            {
                if (IsWindowOpenFour(line, start))
                {
                    fourCount++;
                    break;
                }
            }

            if (IsOpenThree(row, col, dRow, dCol, StoneState.Black))
            {
                openThreecount++;
            }
        }

        // 임의로 놓아본 검은돌을 다시 빈 칸으로 되돌리기 (시뮬 회수)
        board[row, col] = StoneState.Empty;

        // 4, 3의 패턴중 만들어질 패턴이 2개 이상이면 금수 지정
        if (fourCount >= 2 || openThreecount >= 2)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="dRow"></param>
    /// <param name="dCol"></param>
    /// <returns></returns>
    private char[] CreateDirectionLine(int row, int col, int dRow, int dCol)
    {
        const int R = 4; // 양쪽 4칸씩 = 총 9칸
        char[] line = new char[2 * R + 1]; // 길이 9

        for (int i = -R; i <= R; i++)
        {
            int r = row + i * dRow;
            int c = col + i * dCol;

            if (!IsWithinBounds(r, c))
            {
                line[i + R] = 'X'; // 보드 밖 → 막힘
            }
            else if (r == row && c == col)
            {
                line[i + R] = 'B'; // 현재 위치에 놓은 흑돌
            }
            else
            {
                var s = board[r, c];
                line[i + R] = s switch
                {
                    StoneState.Black => 'B',
                    StoneState.White => 'W',
                    StoneState.Empty => 'E',
                    _ => 'X'
                };
            }
        }

        return line;
    }

    /// <summary>
    /// 연속된 같은 색돌을 검사 (기존 CountStonesInLine 함수와 대체 되었음)_
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="dRow"></param>
    /// <param name="dCol"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    private int CountConsecutiveStones(int row, int col, int dRow, int dCol, StoneState player)
    {
        int count = 1;
        for (int i = 1; i < 6; i++)
        {
            if (GetState(row + i * dRow, col + i * dCol, player) == player)
            {
                count++;
            }
            else
            {
                break;
            }
        }
        for (int i = 1; i < 6; i++)
        {
            if (GetState(row - i * dRow, col - i * dCol, player) == player)
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

    private bool IsOpenThree(int row, int col, int dRow, int dCol, StoneState player)
    {
        const int R = 4; // 중심 좌우로 4칸 = 총 9칸 검사
        var line = new char[2 * R + 1];

        for (int i = -R; i <= R; i++)
        {
            int r = row + i * dRow;
            int c = col + i * dCol;

            if (!IsWithinBounds(r, c))
            {
                line[i + R] = 'X'; // 보드 밖 = 막힘
                continue;
            }

            if (r == row && c == col)
            {
                line[i + R] = 'B'; // 지금 둔 내 돌
                continue;
            }

            var s = board[r, c];
            line[i + R] = s switch
            {
                StoneState.Empty => 'E',
                _ when s == player => 'B',
                _ => 'W'
            };
        }

        // 빈칸 하나에 "내가 한 수 더 둔다"고 가정해 오픈4(EBBBBE)가 되는지 확인
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] != 'E') continue;

            char keep = line[i];
            line[i] = 'B'; // 시뮬레이션 착수

            bool makesOpenFour = false;
            for (int start = 0; start <= line.Length - 6; start++)
            {
                if (IsWindowOpenFour(line, start))
                {
                    makesOpenFour = true;
                    break;
                }
            }

            line[i] = keep; // 시뮬 회수

            if (makesOpenFour)
                return true; // 이 방향에 오픈3 존재
        }

        return false;
    }

    // 6칸 창이 정확히 'E B B B B E' 인지 검사
    private bool IsWindowOpenFour(char[] line, int start)
    {
        if (line[start] != 'E' || line[start + 5] != 'E')
            return false;
        for (int k = 1; k <= 4; k++)
            if (line[start + k] != 'B')
                return false;

        // 양끝이 진짜 빈칸인지(보드 밖 'X'가 아닌지) 확인
        return true;
    }

    /// <summary>
    /// 특정 좌표의 상태를 검사하기 위함
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    private StoneState GetState(int row, int col, StoneState player)
    {
        if (!IsWithinBounds(row, col))
        {
            return player == StoneState.Black ? StoneState.White : StoneState.Black;
        }
        return board[row, col];
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
