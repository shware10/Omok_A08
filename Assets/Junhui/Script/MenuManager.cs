using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject MainCanvas;
    public GameObject LoginCanvas;
    public GameObject SignUpCanvas;
    public GameObject StoreCanvas;
    public GameObject LobbyCanvas;
    public TMP_InputField LoginUsername;
    public TMP_InputField LoginPassword;
    public TMP_InputField SignUpUsername;
    public TMP_InputField SignUpPassword;
    public TMP_InputField SignUpPasswordConfirm;
    public TextMeshProUGUI LoginWarning;
    public TextMeshProUGUI SignUpWarning;

    void Start()
    {
        ShowLogin();
    }

    void Update()
    {
        
    }

    private void Reset()
    {
        MainCanvas.SetActive(false);
        LoginCanvas.SetActive(false);
        SignUpCanvas.SetActive(false);
        StoreCanvas.SetActive(false);
        LobbyCanvas.SetActive(false);
        LoginWarning.gameObject.SetActive(false);
        SignUpWarning.gameObject.SetActive(false);
    }
  
    public void SignIn()
    {
        string username = LoginUsername.text;
        string password = LoginPassword.text;
        SigninData str = new SigninData(username,password);
        StartCoroutine(NetworkManager.Instance.Signin(str, ShowMain,SignInFalied));

    }
    public void SignUp()
    {
        string username = SignUpUsername.text;
        string password = SignUpPassword.text;
        SignupData str = new SignupData(username, password);
        StartCoroutine(NetworkManager.Instance.Signup(str, ShowMain, SignInFalied));

    }
    public void SignInFalied(int i)
    {
        if (i == 0) LoginWarning.text = "�̸����� �߸��Ǿ����ϴ�.";
        if (i == 1) LoginWarning.text = "��й�ȣ�� �߸��Ǿ����ϴ�.";
        LoginWarning.gameObject.SetActive(true);
    }
    public void SignUpFalied(int i)
    {
        if (i == 0) SignUpWarning.text = "�̸����� �ߺ��Ǿ����ϴ�..";
        if (i == 1) SignUpWarning.text = "��й�ȣ�� �߸��Ǿ����ϴ�.";
        SignUpWarning.gameObject.SetActive(true);
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
    public void ShowLobby()
    {
        Reset();
        LobbyCanvas.SetActive(true);
    }
}
