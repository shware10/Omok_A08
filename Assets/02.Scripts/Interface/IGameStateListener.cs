using UnityEngine;

public interface IGameStateListener
{
    public void OnStateChanged(GameState state);
}
