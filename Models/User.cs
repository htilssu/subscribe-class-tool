using System;

namespace ClassRegisterApp.Models;

/// <summary>
///     A user object contain username and password
/// </summary>
public class User
{
    private string _fullname;
    private string _userPw;
    private string _userI;
    private string _cookie;
    private bool _isLogin;
    private bool _isLogByCookie;
    public string Cookie
    {
        get => _cookie;
        set => _cookie = value ?? throw new ArgumentNullException(nameof(value));
    }
    public bool IsLogin
    {
        get => _isLogin;
        set => _isLogin = value;
    }
    public bool IsLogByCookie
    {
        get => _isLogByCookie;
        set => _isLogByCookie = value;
    }

    /// <summary>
    /// Create new userI object
    /// </summary>
    /// <param name="fullname">FullName</param>
    /// <param name="userPw">User Identity Password</param>
    /// <param name="userI">UserId</param>
    public User(string fullname, string userPw, string userI)
    {
        _fullname = fullname;
        _userPw = userPw;
        _userI = userI;
    }
    /// <summary>
    /// Create new user object with empty value
    /// </summary>
    public User()
    {
        _fullname = "";
        _userPw = "";
        _userI = "";
    }
    /// <summary>
    /// Get the user's fullname
    /// </summary>
    public string Fullname
    {
        get => _fullname;
    }
    /// <summary>
    ///Get the user's password
    /// </summary>
    public string UserPw
    {
        get => _userPw;
    }
    /// <summary>
    /// Get the user's identity
    /// <p>Thường là mã số sinh viên</p>
    /// </summary>
    public string UserI
    {
        get => _userI;
    }
}
