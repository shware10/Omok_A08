
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

public struct SigninResult
{
    public int result;
}

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
