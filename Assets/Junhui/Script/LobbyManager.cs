using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public GameObject Match;
    public GameObject AIMatch;
    public GameObject MatchProfile;
    public GameObject Content;
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
        Match.SetActive(true);
        AIMatch.SetActive(false);
    }
    public void ShowAIMatch()
    {
        Match.SetActive(false);
        AIMatch.SetActive(true);
    }

    public void Refresh()
    {
        AddProfile();
    }

    public void AddProfile()
    {
        Debug.Log("add");
        Instantiate(MatchProfile, Content.transform);
    }
    public void RemoveAllProfile()
    {
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
