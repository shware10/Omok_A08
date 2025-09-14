using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public enum Phase { PosSelectTurn, BlackTurn, WhiteTurn, EndTurn }
    public Phase phase;


    private void Awake()
    {
        phase = Phase.PosSelectTurn;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (phase)
        {
            case Phase.PosSelectTurn:

                break;
            case Phase.BlackTurn:

                break;
            case Phase.WhiteTurn:

                break;
            case Phase.EndTurn:

                break;
        }
    }


}
