using System;

namespace ClassRegisterApp;

public class User
{
    private string _password;

    private string _userName;

    public User(string userName, string password)
    {
        _password = password;
        _userName = userName;
    }

    public string Password
    {
        get => _password;
        set => _password = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string UserName
    {
        get => _userName;
        set => _userName = value ?? throw new ArgumentNullException(nameof(value));
    }
}