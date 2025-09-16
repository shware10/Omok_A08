using UnityEngine;

public class BoardView_AI : MonoBehaviour, IBoardStateListener
{
    public Cell_AI[,] cells;

    int N;

    void Start()
    {
        N = OmokManager_AI.Instance.N;

        Cell_AI[] cellArray = transform.GetComponentsInChildren<Cell_AI>();

        for (int i = 0; i < cellArray.Length; ++i)
        {
            cells[(i / N), (i % N)] = cellArray[i];
        }
    }

    public void OnBoardChanged(int x, int y, StoneState curStone)
    {
        cells[x, y].ActivateStone(curStone);
    }
}
