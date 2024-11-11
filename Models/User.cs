using System;

namespace ClassRegisterApp.Models;

/// <summary>
///     A user object contain username and password
/// </summary>
public class User
{
    private string _userName;

    /// <summary>
    /// </summary>
    /// <param name="userName">Username to login</param>
    public User(string userName)
    {
        _userName = userName;
    }


    /// <summary>
    /// Username
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public string UserName
    {
        get => _userName;
        set => _userName = value ?? throw new ArgumentNullException(nameof(value));
    }
}
