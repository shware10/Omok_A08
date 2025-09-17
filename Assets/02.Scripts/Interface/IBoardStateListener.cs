using UnityEngine;

public interface IBoardStateListener
{
    void OnBoardChanged(int x, int y, StoneState curStone);
}
