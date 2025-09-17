using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject MainCanvas;
    public GameObject LoginCanvas;
    public GameObject SignUpCanvas;
    public GameObject StoreCanvas;
    void Start()
    {
        ShowLogin();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Reset()
    {
        MainCanvas.SetActive(false);
        LoginCanvas.SetActive(false);
        SignUpCanvas.SetActive(false);
        StoreCanvas.SetActive(false);
    }
    public void ShowMain()
    {
        Reset();
        MainCanvas.SetActive(true);
    }
    public void ShowLogin()
    {
        Reset();
        LoginCanvas.SetActive(true);
    }
    public void ShowSignUp()
    {
        Reset();
        SignUpCanvas.SetActive(true);
    }
    public void ShowStore()
    {
        Reset();
        StoreCanvas.SetActive(true);
    }
}
