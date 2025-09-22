using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

public class Board_AI
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

    public StoneState Get(int x, int y) => board[x, y];

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
        if (!IsInRange(x, y)) return false;
        if (!IsEmpty(x, y)) return false;
        if(curStone == StoneState.Black)
        {
            if (IsForbiddenMove(x, y)) return false;
        }

        board[x, y] = curStone;
        empty -= 1;
        return true;
    }

    public void Undo(int x, int y)
    {
        if (!IsInRange(x, y)) return;
        if (board[x, y] == StoneState.Empty) return;

        board[x, y] = StoneState.Empty;
        empty += 1;
    }

    public bool IsFive(int x, int y, StoneState curStone)
    {
        for (int i = 0; i < directions.GetLength(0); ++i)
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

            if (count >= 5) return true; //딱 5일때만 6목은 배제
        }
        return false;
    }

    public List<(int x, int y)> GenerateMoves(int radius)
    {
        List<(int, int)> moves = new List<(int, int)>();
        HashSet<(int, int)> seen = new HashSet<(int, int)>();

        for (int x = 0; x < N; ++x)
        {
            for (int y = 0; y < N; ++y)
            {
                if (board[x, y] != StoneState.Empty) //돌이 있으면
                {
                    for (int d = 1; d <= radius; d++)
                    {
                        for (int dy = -d; dy <= d; dy++)
                        {
                            for (int dx = -d; dx <= d; dx++)
                            {
                                // 정확히 링 둘레만(체비쇼프 거리 == d) 수집
                                if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != d) continue;
                                int nx = x + dx, ny = y + dy;
                                if (nx < 0 || nx >= N || ny < 0 || ny >= N) continue;
                                if (board[nx, ny] != StoneState.Empty) continue;

                                if (seen.Add((nx, ny))) moves.Add((nx, ny)); // 처음 보는 좌표만 순서 유지 추가
                            }
                        }
                    }
                }
            }
        }
        if (empty == N * N) // 첫 수면
        {
            int center = N / 2;
            moves.Add((center, center));
        }
        return moves;
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

    public bool IsFull()
    {
        return empty == 0;
    }
}
