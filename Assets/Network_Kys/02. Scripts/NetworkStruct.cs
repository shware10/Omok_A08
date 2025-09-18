/// <summary> 회원가입 데이터 구조체 </summary>
public struct SignupData
{
    public string username;
    public string password;

    public SignupData(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}

/// <summary> 로그인 데이터 구조체 </summary>
public struct SigninData
{
    public string username;
    public string password;

    public SigninData(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}

/// <summary> Response 파싱 구조체 </summary>
public struct SigninResult
{
    public int result;
}

public struct Room
{
   public int room_id;
   public string room_title;
}