using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmokAI_AI : MonoBehaviour
{

    [Header("Search")]
    public int maxDepth;

    struct Move { public int x, y; }


    // 평가 밸류
    const int WIN = 1000000000;
    const int OFOUR = WIN / 100;
    const int CFOUR = OFOUR / 100;
    const int OTHREE = CFOUR / 10;
    const int CTHREE = OTHREE / 10;
    const int OTWO = CTHREE / 10;

    const int radius = 2;

    public IEnumerator Think(Board_AI board, StoneState me, Action<int, int> OnStone)
    {
        yield return new WaitForSeconds(1f);

        (int score, (int x, int y) bestMove) = Negamax(
            board, 
            maxDepth, 
            int.MinValue + 1, 
            int.MaxValue - 1, 
            me, 
            me);

        OnStone?.Invoke(bestMove.x, bestMove.y);

        yield break;
    }

    private (int score, (int x, int y) bestMove) Negamax(Board_AI board, int depth, int alpha, int beta, StoneState cur, StoneState me)
    {
        if (depth == 0)
        {
            int eval = Evaluate(board, me);
            // 현재 턴(cur)이 root의 me와 같으면 +, 다르면 -
            if (cur != me) eval = -eval;
            return (eval, (-1, -1));
        }

            int bestScore = int.MinValue + 1;
        (int x, int y) bestMove = (-1, -1);

        List<(int x, int y)> moves = board.GenerateMoves(radius);

        foreach((int x, int y) in moves)
        {
            if (!board.IsEmpty(x, y)) continue;
            if (!board.TryPlace(x, y, cur)) continue;

            if (board.IsFive(x, y, cur))
            {
                board.Undo(x, y);
                return (WIN - (maxDepth - depth), (x, y));
            }

            (int childScore, (int x, int y) curBest) = Negamax(board, depth - 1, -beta, -alpha, Opp(cur), me);
            int score = -childScore;

            board.Undo(x, y);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = (x, y);
            }
            if (score > alpha) alpha = score;
            if (alpha >= beta) break;
        }
        if (bestScore == int.MinValue + 1)
            return (Evaluate(board, me), (-1, -1));
        return (bestScore, bestMove);
    }

    int Evaluate(Board_AI board, StoneState me)
    {
        StoneState opp = Opp(me);
        int myScore = 0;
        int oppScore = 0;

        for(int x = 0; x < board.N; ++x)
        {
            for(int y = 0; y < board.N; ++y)
            {
                StoneState cur = board.Get(x, y);
                if (cur == StoneState.Empty) continue;

                if (cur == me) myScore += PatternScoreAt(board, x, y, me);
                else oppScore += PatternScoreAt(board, x, y, opp);
            }
        }
        return myScore - (int)(1.1f * oppScore);
    }

    int PatternScoreAt(Board_AI board, int x, int y, StoneState s)
    {
        int total = 0;
        total += LineScore(board, x, y, 1, 0, s);
        total += LineScore(board, x, y, 1, 1, s);
        total += LineScore(board, x, y, 0, 1, s);
        total += LineScore(board, x, y, -1, 1, s);

        return total;
    }

    int LineScore(Board_AI board, int x, int y, int dx, int dy, StoneState s)
    {
        int cnt = 1;
        int openEnds = 0;

        int nx = x + dx;
        int ny = y + dy;

        while(board.IsInRange(nx, ny) && board.Get(nx, ny) == s) { ++cnt; nx += dx; ny += dy; }
        if (board.IsInRange(nx, ny) && board.Get(nx, ny) == StoneState.Empty) openEnds++;

        nx = x - dx;
        ny = y - dy;
        while(board.IsInRange(nx, ny) && board.Get(nx, ny) == s) { ++cnt; nx -= dx; ny -= dy; }
        if (board.IsInRange(nx, ny) && board.Get(nx, ny) == StoneState.Empty) openEnds++;

        if (cnt == 5) return WIN;
        
        if(cnt == 4)
        {
            if (openEnds == 2) return OFOUR;
            if (openEnds == 1) return CFOUR;
        }
        else if(cnt == 3)
        {
            if (openEnds == 2) return OTHREE;
            if (openEnds == 1) return CTHREE;
        }
        else if(cnt == 2)
        {
            if (openEnds == 2) return OTWO;
        }

        return 0;
    }

    private StoneState Opp(StoneState s) => (s == StoneState.Black) ? StoneState.White : StoneState.Black;
}
