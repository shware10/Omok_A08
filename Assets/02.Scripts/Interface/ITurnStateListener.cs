using UnityEngine;

public interface ITurnStateListener
{
    public void OnTurnChanged(StoneState curStone);
}
