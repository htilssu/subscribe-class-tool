using System;

namespace ClassRegisterApp;

/// <summary>
///     A user object contain username and password
/// </summary>
internal class User
{
    private string _password;

    private string _userName;

    /// <summary>
    /// </summary>
    /// <param name="userName">Username to login</param>
    /// <param name="password">Password to login</param>
    public User(string userName, string password)
    {
        _password = password;
        _userName = userName;
    }

    public int Delay { get; set; }

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