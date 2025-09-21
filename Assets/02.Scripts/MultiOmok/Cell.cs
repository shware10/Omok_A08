using UnityEngine;

public class Cell : MonoBehaviour,IGameStateListener
{
    public int X, Y;
    GameObject whiteStone;
    GameObject blackStone;
    GameObject lastMarker;

    public StoneState curState;

    void Awake()
    {
        whiteStone = transform.GetChild(0).gameObject;
        blackStone = transform.GetChild(1).gameObject;
        lastMarker = transform.GetChild(2).gameObject;
        InitCell();
    }

    public void InitCell()
    {
        whiteStone.SetActive(false);
        blackStone.SetActive(false);
        lastMarker.SetActive(false);
        curState = StoneState.Empty;
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

    public void SetLastMarker(bool active)
    {
        lastMarker.SetActive(active);
    }
}
