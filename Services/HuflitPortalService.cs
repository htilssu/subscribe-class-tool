using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Controls;
using ClassRegisterApp.Models;
using ClassRegisterApp.Pages;
using HtmlAgilityPack;

namespace ClassRegisterApp.Services;

internal class HuflitPortal
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


    private string _cookie = "";

    private ListBox? _listBoxx;

    public Main.SubscribeType SubscribeType { get; set; }


    public int Delay { get; init; }


    public void SetCookie(string cookie, bool loginType)
    {
        if (loginType)
            _cookie = "ASP.NET_SessionId=" + cookie;
        else
            _cookie = cookie;

        _cookie = _cookie.Trim();
        _client.DefaultRequestHeaders.Remove("Cookie");
        _client.DefaultRequestHeaders.Add("Cookie", _cookie);
    }


    /// <summary>
    ///     Single registry (when a request get successfully hideId)
    /// </summary>
    /// <param name="classListCode"></param>
    /// <param name="listBox"></param>
    public async void RunOptimized(List<string>? classListCode, ListBox listBox)
    {
        _listBoxx = listBox;
        var subjectIdList = await GetSubjectIdList();
        if (classListCode == null || subjectIdList == null) return;
        RegistrySubject(subjectIdList, classListCode, listBox);
    }

    private async void RegistrySubject(IEnumerable<string> subjectIdList, List<string> classListCode,
        ItemsControl listBox)
    {
        var tasks = subjectIdList.Select(Register).ToList();
        await Task.Delay(Delay);
        await Task.WhenAll(tasks);
        return;

        async Task Register(string classId)
        {
            var newClient = GetHttpRequest(_cookie);
            var hideId = new Dictionary<string, string>();
            var childHide = new Dictionary<string, Dictionary<string, string>>();
            var subscribeType = SubscribeType == Main.SubscribeType.KH ? "KH" : "NKH";
            var response =
                await newClient.GetAsync(
                    $"https://dkmh.huflit.edu.vn/DangKyHocPhan/DanhSachLopHocPhan?id={classId}&registType={subscribeType}");
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
                        classCode = GetFirstStringInJavaScripts(onClickAttributeValue);
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
                    hideId.TryAdd(classCode, secret);
                    childHide.TryAdd(classCode, childHideIdDictionary);
                }

            foreach (var code in classListCode)
                if (hideId.ContainsKey(code) || hideId.ContainsKey(code.Split('-')[0]))
                {
                    var registerHide = "";
                    if (code.Contains('-'))
                    {
                        var split = code.Split('-');
                        var lt = split[0];
                        var th = split[1];
                        hideId.TryGetValue(lt, out var ltHideId);
                        var isHasChildHide = childHide.TryGetValue(lt, out var dicChildHide);
                        if (isHasChildHide)
                        {
                            dicChildHide!.TryGetValue(th, out var thHideId);
                            registerHide = ltHideId + "|" + thHideId + "|";
                        }
                    }
                    else
                    {
                        hideId.TryGetValue(code, out var value);
                        registerHide = value;
                    }

                    listBox.Items.Add($"Đang đăng ký {code}");
                    var responseRegistry = await _client.GetAsync(
                        $"https://dkmh.huflit.edu.vn/DangKyHocPhan/RegistUpdateScheduleStudyUnit?Hide={registerHide}&ScheduleStudyUnitOld=&acceptConflict=");

                    var status = await responseRegistry.Content.ReadFromJsonAsync<PortalResponseStatus>();
                    listBox.Items.Add(status?.Msg + $" {code}");
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
        await _client.GetAsync(Url + "/Home/DangKyHocPhan");
    }

    public async Task ConnectToDKMH()
    {
        var res = await _client.GetAsync("https://dkmh.huflit.edu.vn/DangKyHocPhan");
        res.Content.Headers.TryGetValues("Set-Cookie", out var cookie);
        if (cookie == null) return;
        var newCookie = cookie.Aggregate("", (current, value) => current + value);
        if (!_cookie.EndsWith(';')) _cookie += ";";
        _cookie += newCookie;
        SetCookie(_cookie, false);
    }


    /// <summary>
    ///     Get subject ID
    /// </summary>
    /// <returns>A list contain Subject ID to get SecretCode</returns>
    private async Task<List<string>?> GetSubjectIdList()
    {
        _listBoxx?.Items.Add("Đang lấy danh sách học phần");
        var subscribeType = SubscribeType == Main.SubscribeType.KH ? "KH" : "NKH";
        var response =
            await _client.GetAsync(
                $@"https://dkmh.huflit.edu.vn/DangKyHocPhan/DanhSachHocPhan?typeId={subscribeType}&id=");
        var document = new HtmlDocument();
        var subjectIdList = new List<string>();
        var content = await response.Content.ReadAsStringAsync();

        document.LoadHtml(content);

        var linkNodes = document.DocumentNode.SelectNodes("//td/a");

        if (linkNodes != null)
            foreach (var node in linkNodes)
            {
                var hrefData = node.GetAttributeValue("href", "");
                subjectIdList.Add(GetSubjectId(hrefData));
            }

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

    private string GetSubjectName(string href)
    {
        var subString = href[(href.IndexOf(',') + 2)..];
        subString = subString[..subString.IndexOf('\'')];
        var decoded = HttpUtility.HtmlDecode(subString);
        return decoded;
    }

    private string GetFirstStringInJavaScripts(string href)
    {
        var startIndex = href.IndexOf('\'') + 1;
        var subHrefData = href[startIndex..];
        var endIndex = subHrefData.IndexOf('\'');
        var subjectId = href.Substring(startIndex, endIndex);
        return subjectId;
    }

    public async Task<User?> CheckCookie()
    {
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

    private HttpClient GetHttpRequest(string cookie)
    {
        var newRequest = new HttpClient();
        newRequest.DefaultRequestHeaders.Add("Cookie", cookie);
        newRequest.Timeout = TimeSpan.FromMinutes(10);
        return newRequest;
    }
}