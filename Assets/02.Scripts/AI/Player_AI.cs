using System;
using System.Collections;
using UnityEngine;

public class Player_AI : MonoBehaviour
{
    private LayerMask cellMask;

    void Awake()
    {
        cellMask = LayerMask.NameToLayer("Cell");
    }

    public IEnumerator Think(Board_AI board, StoneState selectedStone, Action<int,int> OnStone)
    {
        while(true)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                Cell_AI cell = null;
                if (Physics.Raycast(r, out var hit, 100f, cellMask))
                {
                    cell = hit.collider.GetComponent<Cell_AI>();
                    if(cell != null && board.IsEmpty(cell.X, cell.Y))
                    {
                        OnStone(cell.X, cell.Y);
                        yield break;
                    }
                }
            }
            yield return null;
        }
    }
}
