using NUnit.Framework;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LobbyManager : MonoBehaviour
{
    public GameObject Match;
    public GameObject NewRoom;
    public GameObject AIMatch;
    public GameObject MatchProfile;
    public GameObject Content;
    public TMP_InputField Title;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowMatch();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMatch()
    {
        Reset();
        Match.SetActive(true);
    }
    public void ShowAIMatch()
    {
        Reset();
        AIMatch.SetActive(true);
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
        AIMatch.SetActive(false);
    }


    public void CreateRoom()
    {
        if (Title.text != null) NetworkManager.Instance.Send_Room_Crate(Title.text);
    }

    public void JoinRoom(int room_id)
    {
        NetworkManager.Instance.Send_Room_Join(room_id);
    }

    public void Refresh()
    {
        NetworkManager.Instance.Send_Get_Room(0);
        List list = new List();
        list.Add(0);
        CreateProfiles(list);
    }

    public void CreateProfiles(List Rooms)
    {
        Rooms.count;
        for (int i = 0; i < Rooms.Count; i++)
        {
            AddProfile(Rooms)
        }
    }


    public void AddProfile(int room_id,string room_title)
    {
        GameObject obj = Instantiate(MatchProfile, Content.transform);
    }
    public void RemoveAllProfile()
    {
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
