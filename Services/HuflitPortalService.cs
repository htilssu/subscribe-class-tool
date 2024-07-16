using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using ClassRegisterApp.Models;
using ClassRegisterApp.Pages;
using HtmlAgilityPack;

namespace ClassRegisterApp.Services;

/// <summary>
/// Lớp hỗ trợ tương tác với portal của trường
/// </summary>
public class HuflitPortal
{
    private const string Url = "https://portal.huflit.edu.vn/";

    /// <summary>
    ///     Child hide id of class
    /// </summary>
    /// <remarks>key: classCode | value: dictionary child class code</remarks>
    private readonly Dictionary<string, Dictionary<string, string>> _classChild = new();

    /// <summary>
    ///     Secret id dictionary
    /// </summary>
    /// <remarks>key: classCode | value: secret code</remarks>
    private readonly Dictionary<string, string> _classHideId = new();

    private readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromMinutes(30)
    };

    private readonly Dictionary<string, string> _secretClassList = new();

    bool _isRegisterCookie;

    private readonly Dictionary<string, bool> _isRegistered = new();

    public bool IsRegisterCookie
    {
        get => _isRegisterCookie;
        set => _isRegisterCookie = value;
    }

    private List<string>? _targetRegisterClass;


    private string _cookie = "";

    private ListBox? _listBoxx;

    internal Main.SubscribeType SubscribeType { get; init; }


    public int Delay { get; init; }


    /// <summary>
    /// Đặt lại cookie cho HttpClient bằng cách xóa cookie cũ mà set lại cookie mới được thêm vào
    /// </summary>
    /// <param name="cookie">cookie mới</param>
    public void SetCookie(string cookie)
    {
        _cookie = cookie.Trim();
        if (!_cookie.EndsWith(';')) _cookie += ";";
        _client.DefaultRequestHeaders.Remove("Cookie");
        _client.DefaultRequestHeaders.Add("Cookie", _cookie);
    }


    /// <summary>
    /// <para>Chạy đăng ký tất cả các môn cùng lúc</para>
    /// </summary>
    /// <param name="classListCode">Danh sách lớp học cần đăng ký</param>
    /// <param name="listBox">listBox hiển thị trạng thái đăng ký</param>
    public async void RunOptimized(List<string> classListCode, ListBox listBox)
    {
        _listBoxx = listBox;
        classListCode.ForEach((c) =>
        {
            _isRegistered.TryAdd(c, false); //mark all class as not registered
        });
        _targetRegisterClass = new List<string>(classListCode);
        var subjectIdList = await GetSubjectIdList();
        if (subjectIdList == null)
        {
            listBox.Items.Add("Chưa mở đăng ký, đừng spam nữa");
            return;
        }

        StartFetchSecret();
        RegistrySubject(subjectIdList, classListCode, listBox);
    }

    private async void RegistrySubject(IEnumerable<string> subjectIdList
        , List<string> classListCode
        , ItemsControl listBox)
    {
        var tasks = subjectIdList.Select(Register).ToList();
        await Task.Delay(Delay);
        await Task.WhenAll(tasks);
        return;

        async Task Register(string classId)
        {
            var newClient = GetHttpRequest(_cookie);
            var subscribeType = SubscribeType == Main.SubscribeType.KH ? "KH" : "NKH";
            HttpResponseMessage response;
            try
            {
                response =
                    await newClient.GetAsync(
                        $"https://dkmh.huflit.edu.vn/DangKyHocPhan/DanhSachLopHocPhan?id={classId}&registType={subscribeType}");
            } catch (Exception)
            {
                _listBoxx?.Items.Add($"Lỗi khi lấy danh lớp học phần của {classId}");
                return;
            }

            listBox.Items.Add($"Đang lấy thông tin {classId}");
            var content = await response.Content.ReadAsStringAsync();
            var contentDocument = new HtmlDocument();
            contentDocument.LoadHtml(content);

            var inputList = contentDocument.DocumentNode.SelectNodes("//form//tbody//input[@type='radio']");

            if (inputList != null)
                foreach (var node in inputList)
                {
                    var secret = node.GetAttributeValue("id", "");
                    var onClickAttributeValue = node.GetAttributeValue("onclick", "");
                    var childHideIdDictionary = new Dictionary<string, string>();
                    var classCode = "";
                    if (onClickAttributeValue != "")
                    {
                        classCode = GetSubjectId(onClickAttributeValue);
                        var childList =
                            contentDocument.DocumentNode.SelectNodes(
                                $"//form//tr[@id='tr-of-{classCode}']//input[@type='radio']");

                        var childClassList =
                            contentDocument.DocumentNode.SelectNodes($"//form//tr[@id='tr-of-{classCode}']//tr");

                        if (childList != null)
                        {
                            var index = 1;
                            foreach (var child in childList)
                            {
                                var tdList = childClassList[index].SelectNodes(".//td");
                                var childHideId = child.GetAttributeValue("id", "");
                                if (childHideId != "") childHideIdDictionary.TryAdd(tdList[1].InnerText, childHideId);
                                index++;
                            }
                        }
                    }

                    if (secret.Contains("tr-of-") || classCode == "") continue;
                    _classHideId.TryAdd(classCode, secret);
                    _classChild.TryAdd(classCode, childHideIdDictionary);
                }

            foreach (var code in classListCode)
                if (_classHideId.ContainsKey(code) || _classHideId.ContainsKey(code.Split('-')[0]))
                {
                    var registerHide = "";
                    if (code.Contains('-'))
                    {
                        var split = code.Split('-');
                        var lt = split[0];
                        var th = split[1];
                        _classHideId.TryGetValue(lt, out var ltHideId);
                        var isHasChildHide = _classChild.TryGetValue(lt, out var dicChildHide);
                        if (isHasChildHide)
                        {
                            dicChildHide!.TryGetValue(th, out var thHideId);
                            registerHide = ltHideId + "|" + thHideId + "|";
                        }
                    }
                    else
                    {
                        _classHideId.TryGetValue(code, out var value);
                        registerHide = value;
                    }

                    listBox.Items.Add($"Đang đăng ký {code}");
                    try
                    {
                        var responseRegistry = await newClient.GetAsync(
                            $"https://dkmh.huflit.edu.vn/DangKyHocPhan/RegistUpdateScheduleStudyUnit?Hide={registerHide}&ScheduleStudyUnitOld=&acceptConflict=");

                        var classList = SecretService.ConvertDicToClass(_classHideId, _classChild);
                        if (classList != null)
                        {
                            foreach (var @class in classList)
                            {
                                _ = await SecretService.AddSecret(@class);
                            }
                        }

                        var status = await responseRegistry.Content.ReadFromJsonAsync<PortalResponseStatus>();

                        listBox.Items.Add(status?.Msg + $" {code}");
                    } catch (HttpRequestException e)
                    {
                        listBox.Items.Add($"Lỗi khi đăng ký {code}");
                        Console.WriteLine(e);
                    } catch (Exception e)
                    {
                        listBox.Items.Add($"Lỗi khi đăng ký {code}");
                        Console.WriteLine(e);
                    }

                    break;
                }
        }
    }


    /// <summary>
    ///     Redirect to DKMH page
    ///     <remarks>
    ///         Must call after
    ///     </remarks>
    /// </summary>
    public async Task RegisterCookieToServer()
    {
        if (!_isRegisterCookie)
        {
            var registerResponse = await _client.GetAsync(Url + "/Home/DangKyHocPhan");
            registerResponse.Headers.TryGetValues("Set-Cookie", out var cookie);


            if (cookie != null)
            {
                var cookieDic = ParseCookie(cookie.Aggregate("", (s, s1) => s + s1));
                if (cookieDic.TryGetValue("user", out var user)
                    && cookieDic.TryGetValue("UserPW", out var userPw))
                {
                    var response = await DataService.SendRequest(HttpMethod.Get, $"/api/v1/user?user={user}", null);
                    try
                    {
                        var userData = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(userData))
                        {
                            await DataService.SendRequest(HttpMethod.Post
                                , "/api/v1/user"
                                , $"{{\"User\":\"{user}\",\"UserPW\":\"{userPw}\"}}");
                        }
                    } catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    SetCookie(registerResponse.Headers);
                    await ConnectToDkmh(); //get register cookie

                    _isRegisterCookie = true;
                }
            }
        }
    }

    private void SetCookie(HttpResponseHeaders responseHeaders)
    {
        if (responseHeaders.TryGetValues("Set-Cookie", out var cookie))
        {
            _cookie = cookie.Aggregate("", (current, value) => current + value);
        }
    }

    /// <summary>
    /// Kết nối tới trang ĐKMH với USER và PASSWORD khi kết nối sẽ được server
    /// trả về cookie mới để đăng ký môn học, cookie này sẽ là định danh để đăng ký môn học
    /// </summary>
    public async Task ConnectToDkmh()
    {
        if (!_isRegisterCookie)
        {
            var res = await _client.GetAsync("https://dkmh.huflit.edu.vn/DangKyHocPhan");
            SetCookie(res.Headers);
            if (!_cookie.EndsWith(';')) _cookie += ";";
            _isRegisterCookie = true;
        }
    }


    /// <summary>
    ///     Get subject ID
    /// </summary>
    /// <returns>A list contain Subject ID to get SecretCode</returns>
    private async Task<List<string>?> GetSubjectIdList()
    {
        _listBoxx?.Items.Add("Đang lấy danh sách học phần");
        var subscribeType = SubscribeType == Main.SubscribeType.KH ? "KH" : "NKH";
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync(
                $"https://dkmh.huflit.edu.vn/DangKyHocPhan/DanhSachHocPhan?typeId={subscribeType}&id=");
        } catch (Exception)
        {
            _listBoxx?.Items.Add("Lỗi kết nối");
            return null;
        }

        var document = new HtmlDocument();
        var subjectIdList = new List<string>();
        var content = await response.Content.ReadAsStringAsync();

        document.LoadHtml(content);

        var courseNodes = document.DocumentNode.SelectNodes("//tbody/tr/td[2]");
        if (courseNodes == null) return null;
        var courseList = courseNodes.Select(node => node.InnerText.Trim());
        var linkNodes = document.DocumentNode.SelectNodes("//td/a");

        if (linkNodes == null) return subjectIdList;

        subjectIdList.AddRange(linkNodes.Select(node => node.GetAttributeValue("href", ""))
            .Select(GetSubjectId));

        _ = courseList.Select((s, i) => _secretClassList.TryAdd(s, subjectIdList[i]));

        return subjectIdList;
    }


    private string GetSubjectId(string href)
    {
        var startIndex = href.IndexOf('\'') + 1;
        var subHrefData = href[startIndex..];
        var endIndex = subHrefData.IndexOf('\'');
        var subjectId = href.Substring(startIndex, endIndex);
        return subjectId;
    }


    /// <summary>
    /// Kiểm tra cookie của portal xem có hợp lệ hay không bằng cách lấy tên của người dùng
    /// </summary>
    /// <returns>Đối tượng người dùng chứa thông tin của người dùng đã đăng ký</returns>
    public async Task<User?> CheckCookie()
    {
        _isRegisterCookie = false;
        var res = await _client.GetAsync(Url);
        if (!res.IsSuccessStatusCode) return null;
        var responseContent = await res.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(responseContent);
        var info = document.DocumentNode.SelectNodes("//*[@id=\"menu\"]/ul[2]/li/span/a");
        if (info == null || info[0].InnerText == "Đăng nhập") return null;

        var user = new User(info[0].InnerText);
        return user;
    }

    /// <summary>
    /// Lấy request mới với cookie hiện tại để thực hiện các request khác
    /// </summary>
    /// <param name="cookie">cookie được set cho request mới</param>
    /// <returns>HttpClient mới để thực hiện gửi request</returns>
    private HttpClient GetHttpRequest(string cookie)
    {
        var newRequest = new HttpClient();
        try
        {
            newRequest.DefaultRequestHeaders.Add("Cookie", cookie);
            newRequest.Timeout = TimeSpan.FromMinutes(10);
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return newRequest;
    }

    public string GetCookie()
    {
        return _cookie;
    }

    static Dictionary<string, string> ParseCookie(string cookie)
    {
        var dictionary = new Dictionary<string, string>();
        var pairs = cookie.Split(";", StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split(['='], 2);

            if (keyValue.Length != 2) continue;
            var key = keyValue[0].Trim();
            var value = keyValue[1].Trim();

            dictionary[key] = value;
        }

        return dictionary;
    }

    //len lich cu sau 1 khoang thoi gian thi fetch secret tu secretService  ve
    private async void FetchSecret()
    {
        var count = 30;
        while (count-- > 0)
        {
            var classList = await SecretService.GetAllSecrets();
            if (classList.Length == 0) return;
            foreach (var @class in classList)
            {
                _secretClassList.TryAdd(@class.Id, @class.Secret);
                var childDic
                    = @class.Children.ToDictionary(classChild => classChild.Id, classChild => classChild.Secret);
                _classChild.TryAdd(@class.Id, childDic);
            }

            if (_targetRegisterClass != null)
            {
                foreach (var se in _targetRegisterClass)
                {
                    if (!_isRegistered.TryGetValue(se, out var status)) continue;
                    if (!status)
                    {
                        var statusRegisterCookieByClassCodeFormat = await RegisterCookieByClassCodeFormat(se);
                        _isRegistered[se] = statusRegisterCookieByClassCodeFormat;
                    }
                }
            }


            await Task.Delay(3000);
        }
    }

    private async Task<bool> RegisterCookieByClassCodeFormat(string se)
    {
        string secretCode = "";

        var classList = se.Split("-");
        if (_classHideId.TryGetValue(classList[0], out var secret))
        {
            secretCode += secret;
        }

        if (classList.Length >= 2)
        {
            if (_classHideId.TryGetValue(classList[1], out secret))
            {
                secretCode += "|" + secret + "|";
            }
        }

        return await RegisterBySecret(secretCode);
    }

    private async Task<bool> RegisterBySecret(string secret)
    {
        var httpClient = GetHttpRequest(_cookie);
        try
        {
            var response = await httpClient.GetAsync(
                $"https://dkmh.huflit.edu.vn/DangKyHocPhan/RegistUpdateScheduleStudyUnit?Hide={secret}&ScheduleStudyUnitOld=&acceptConflict=");

            var status = await response.Content.ReadFromJsonAsync<PortalResponseStatus>();

            _listBoxx?.Items.Add(status?.Msg);
            return true;
        } catch (Exception e)
        {
            _listBoxx?.Items.Add("Lỗi khi đăng ký");
            Console.WriteLine(e);
        }

        return false;
    }


    /// <summary>
    /// Bắt đầu fetch secret từ server
    /// </summary>
    private void StartFetchSecret()
    {
        Thread fetchSecret = new(FetchSecret);
        try
        {
            fetchSecret.Start();
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
