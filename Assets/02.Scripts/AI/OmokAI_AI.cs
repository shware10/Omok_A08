using System;
using System.Collections;
using UnityEngine;

public class OmokAI_AI : MonoBehaviour
{

    [Header("Search")]
    public int maxDepth = 3;
    public float timeLimit = 200f; //ms

    struct Move { public int x, y; }

    public IEnumerator Think(Board_AI board, StoneState me, Action<int, int> OnStone)
    {

        Move bestMove = new Move { x = -1, y = -1 };
        int bestScore = int.MinValue;

        int a = int.MinValue + 1;
        int b = int.MaxValue - 1;

        yield return null;
    }





    void MiniMaxWithAlphaBeta(Board_AI board, int depth, int a, int b, bool maxi, StoneState me, ref Move bestMove )
    {

    }
}
