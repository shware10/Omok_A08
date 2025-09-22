using NUnit.Framework;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class LobbyManager : MonoBehaviour
{
    [Header("Match")]
    public GameObject Match;
    public GameObject MatchProfile;
    public GameObject Content;

    [Header("New Room")]
    public GameObject NewRoom;
    public TMP_InputField Title;

    [Header("ETC Button")]
    [SerializeField] private Button AI_play_Btn;

    [Header("Chat")]
    [SerializeField] private TMP_InputField input_field;
    [SerializeField] private RectTransform chat_content_tf;
    [SerializeField] private Button chat_confirm_btn;
    [SerializeField] private GameObject text_box_prefab;

    void Start()
    {
        ShowMatch();
        input_field.onSubmit.AddListener((str) => SendLobbyChat());
        this.AI_play_Btn.onClick.AddListener(OnAIPlayBtn);
        this.chat_confirm_btn.onClick.AddListener(SendLobbyChat);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendLobbyChat();
        }
    }

    void OnEnable()
    {
        NetworkManager.Instance.room_res_act += CreateProfiles;
        NetworkManager.Instance.lobby_chat_act += LobbyChatCallBack;
    }

    void OnDisable()
    {
        NetworkManager.Instance.room_res_act -= CreateProfiles;
        NetworkManager.Instance.lobby_chat_act -= LobbyChatCallBack;
    }

    public void ShowMatch()
    {
        Reset();

        Match.SetActive(true);

        Refresh();
    }

    public void ShowNewRoom()
    {
        Reset();
        NewRoom.SetActive(true);
    }

    private void Reset()
    {
        Match.SetActive(false);
        NewRoom.SetActive(false);
    }

    public void CreateRoom()
    {
        if (Title.text != null)
        {
            NetworkManager.Instance.Send_Room_Crate(Title.text);
        }
    }

    public void JoinRoom(int room_id)
    {
        NetworkManager.Instance.Send_Room_Join(room_id);
    }


    public void Refresh()
    {
        NetworkManager.Instance.Send_Get_Room(0);
        List<Room> list = new List<Room>();
        CreateProfiles(list);
    }

    public void CreateProfiles(List<Room> Rooms)
    {
        RemoveAllProfile();
        for (int i = 0; i < Rooms.Count; i++)
        {
            AddProfile(Rooms[i].room_id, Rooms[i].room_title);
        }
    }


    public void AddProfile(int room_id, string room_title)
    {
        GameObject obj = Instantiate(MatchProfile, Content.transform);
        obj.GetComponent<MatchProfile>().room_id = room_id;
        obj.GetComponent<MatchProfile>().room_title = room_title;

    }
    public void RemoveAllProfile()
    {
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnAIPlayBtn()
    {
        SceneManager.LoadScene(2);
    }

    private void SendLobbyChat()
    {
        if (input_field.text == "")
        {
            return;
        }
        else
        {
            string chat = input_field.text;
            NetworkManager.Instance.Send_Lobby_Chat(chat);

            this.input_field.text = "";

            // 포커스 유지
            input_field.ActivateInputField();
            input_field.MoveTextEnd(false);
        }
    }


    private void LobbyChatCallBack(string name, string chat)
    {
        Chat($"{name} : {chat}");
    }

    private void Chat(string str)
    {
        GameObject text_box = Instantiate(text_box_prefab, chat_content_tf);

        text_box.GetComponent<TextMeshProUGUI>().text = str;
    }

}
