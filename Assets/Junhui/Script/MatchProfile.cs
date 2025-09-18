using UnityEngine;
using UnityEngine.UI;

public class MatchProfile : MonoBehaviour
{
    public Button GameStart;

    int room_id = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject lobbyManager = GameObject.FindWithTag("LobbyManager");
        LobbyManager manager = lobbyManager.GetComponent<LobbyManager>();
        GameStart.onClick.AddListener(delegate { manager.JoinRoom(room_id); });


        // Update is called once per frame
        void Update()
        {

        }
    }
}
