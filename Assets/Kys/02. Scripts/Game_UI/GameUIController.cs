using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("Turn")]
    [SerializeField] private GameObject current_black;
    [SerializeField] private GameObject current_white;
    [SerializeField] private TextMeshProUGUI player_text_black;
    [SerializeField] private TextMeshProUGUI player_text_white;

    [Header("Result Panel")]
    [SerializeField] private GameObject win_or_lose_panel;
    [SerializeField] private GameObject win_black_image;
    [SerializeField] private GameObject win_white_image;
    [SerializeField] private TextMeshProUGUI win_state_text;

    [SerializeField] private GameObject draw_panel;

    [Header("Chat")]
    [SerializeField] private GameObject content;
    [SerializeField] private TMP_InputField input_field;
    [SerializeField] private Button chat_confirm_btn;
    [SerializeField] private GameObject text_box_prefab;

    [Header("Start / Exit BTN")]
    [SerializeField] private Button start_btn;
    [SerializeField] private Button exit_btn;

    public event Action start_btn_act;

    void Start()
    {
        // this.start_btn.onClick.AddListener(OnStartBtn);
        this.exit_btn.onClick.AddListener(OnExitBtn);
        this.chat_confirm_btn.onClick.AddListener(SendGameChat);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendGameChat();
        }
    }

    void OnEnable()
    {
        NetworkManager.Instance.game_chat_act += GameChatCallBack;
        NetworkManager.Instance.room_join_act += OppnentJoinAlert;
        NetworkManager.Instance.room_exit_act += OppnentExitAlert;
        NetworkManager.Instance.server_brc_act += ServerBrcCallback;
    }

    void OnDisable()
    {
        NetworkManager.Instance.game_chat_act -= GameChatCallBack;
        NetworkManager.Instance.room_join_act -= OppnentJoinAlert;
        NetworkManager.Instance.room_exit_act -= OppnentExitAlert;
        NetworkManager.Instance.server_brc_act -= ServerBrcCallback;
    }

    /// <summary> 게임 시작 시 초기화 ( 게임 시작 시 호출 필요 ) </summary>
    public void SetGameStart(StoneState my_state)
    {
        win_or_lose_panel.SetActive(false);
        draw_panel.SetActive(false);

        player_text_black.text = my_state == StoneState.Black ? "Player" : "Opponent";
        player_text_white.text = my_state == StoneState.Black ? "Opponent" : "Player";

        current_black.SetActive(true);
        current_white.SetActive(false);
    }

    /// <summary> 현재 턴 상태 UI 변경 ( 착수 확정 시 호출 필요 )</summary>
    public void SetCurrentTrunUI(StoneState last_state)
    {
        bool isBlack = last_state == StoneState.Black ? true : false;

        current_black.SetActive(!isBlack);
        current_white.SetActive(isBlack);
    }

    /// <summary> 결과 창 보여주기 </summary>
    public void ShowResultPanel(GameState result)
    {
        switch (result)
        {
            case GameState.BlackWin:
                win_or_lose_panel.SetActive(true);
                win_black_image.SetActive(true);
                win_white_image.SetActive(false);
                break;
            case GameState.WhiteWin:
                win_or_lose_panel.SetActive(true);
                win_black_image.SetActive(false);
                win_white_image.SetActive(true);
                break;
            case GameState.Draw:
                draw_panel.SetActive(true);
                break;
        }
    }

    /// <summary> Start 버튼 활성/비활성화 </summary>
    public void SetStartBtnVisible(bool show)
    {
        this.start_btn.gameObject.SetActive(show);
    }

    private void OnStartBtn()
    {
        start_btn_act?.Invoke();
    }

    private void OnExitBtn()
    {
        int scene_idx = SceneManager.GetActiveScene().buildIndex;
        if (scene_idx == 1)
        {
            NetworkManager.Instance.Send_Room_Exit();

        }
        else if (scene_idx == 2)
            SceneManager.LoadScene(0);
    }


    private void SendGameChat()
    {
        if (input_field.text == "")
        {
            return;
        }
        else
        {
            string chat = input_field.text;
            NetworkManager.Instance.Send_Game_Chat(chat);

            this.input_field.text = "";
        }
    }

    private void GameChatCallBack(string name, string chat)
    {
        Chat($"{name} : {chat}");
    }

    private void OppnentJoinAlert(int flag)
    {
        if (flag == 2)
        {
            Chat("Someone has joined the room");
        }
    }

    private void OppnentExitAlert(int flag)
    {
        if (flag == 2)
        {
            Chat("The opponent left");
        }
    }

    private void ServerBrcCallback(string str)
    {

    }

    private void Chat(string str)
    {
        GameObject text_box = Instantiate(text_box_prefab, content.transform);
        text_box.GetComponent<TextMeshProUGUI>().text = str;
    }

}
