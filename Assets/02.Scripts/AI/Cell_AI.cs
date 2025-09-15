using UnityEngine;

public class Cell_AI : MonoBehaviour,IGameStateListener
{
    public int X, Y;
    GameObject whiteStone;
    GameObject blackStone;
    public StoneState curState;

    void Awake()
    {
        whiteStone = transform.GetChild(0).gameObject;
        blackStone = transform.GetChild(1).gameObject;
        InitCell();
    }

    public void InitCell()
    {
        whiteStone.SetActive(false);
        blackStone.SetActive(false);
    }

    public void ActivateStone(StoneState curStone)
    {
        switch (curStone)
        {
            case StoneState.Black:
                blackStone.SetActive(true);
                break;
            case StoneState.White:
                whiteStone.SetActive(true);
                break;
        }
    }
}
