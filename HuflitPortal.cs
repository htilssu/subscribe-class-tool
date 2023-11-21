using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Controls;
using HtmlAgilityPack;

namespace ClassRegisterApp;

internal class HuflitPortal
{
    /// <summary>
    ///     Child hide id of class
    /// </summary>
    /// <remarks>key: classCode | value: dictionary child class code</remarks>
    private readonly Dictionary<string, Dictionary<string, string>> _classChild = new();

    private readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromHours(1)
    };

    private readonly string _password;
    private readonly string _url = @"https://portal.huflit.edu.vn";

    /// <summary>
    ///     Secret id dictionary
    /// </summary>
    /// <remarks>key: classCode | value: secret code</remarks>
    private Dictionary<string, string> _classHideId = new();

    private string _cookie = "";


    public HuflitPortal(string userName, string password)
    {
        UserName = userName;
        _password = password;
    }


    public int Delay { get; init; }

    public string UserName { get; }


    /// <summary>
    ///     Start register course
    /// </summary>
    /// <remarks>You must call <see cref="ConnectToDkmh" /> before registry course</remarks>
    /// <param name="path">path that link to class list folder</param>
    public async Task Run(string path)
    {
        var classListCode = GetClassCodeListFromFile(path);
        var subjectIdList = await GetSubjectIdList();
        if (classListCode == null) return;
        _classHideId = await GetHideId(subjectIdList!);
        await Task.Delay(Delay);
        await RegisterSubject(classListCode, _classHideId);
    }

    /// <summary>
    ///     Start registry course
    /// </summary>
    /// <remarks>You must call <see cref="ConnectToDkmh" /> before registry course</remarks>
    /// <param name="classListCode">List of class code</param>
    /// <param name="listBox"></param>
    public async Task Run(List<string>? classListCode, ListBox listBox)
    {
        var subjectIdList = await GetSubjectIdList();
        if (classListCode == null || subjectIdList == null) return;
        _classHideId = await GetHideId(subjectIdList);
        await Task.Delay(Delay);
        await RegisterSubject(classListCode, _classHideId, listBox);
    }

    /// <summary>
    ///     Single registry (when a request get successfully hideId)
    /// </summary>
    /// <param name="classListCode"></param>
    /// <param name="listBox"></param>
    public async Task RunOptimized(List<string>? classListCode, ListBox listBox)
    {
        var subjectIdList = await GetSubjectIdList();
        if (classListCode == null || subjectIdList == null) return;
        // _classHideId = await GetHideId(subjectIdList);
        // await Task.Delay(Delay);
        // await RegisterSubject(classListCode, _classHideId, listBox);
        await RegistSubject(subjectIdList, classListCode, listBox);
    }

    private async Task RegistSubject(List<string> subjectIdList, List<string> classListCode, ListBox listBox)
    {
        var tasks = subjectIdList.Select(Body).ToList();
        await Task.WhenAll(tasks);
        return;


        async Task Body(string classId)
        {
            var hideId = new Dictionary<string, string>();
            var childHide = new Dictionary<string, Dictionary<string, string>>();
            _client.Timeout = TimeSpan.FromMinutes(20);
            var response =
                await _client.GetAsync(
                    $"https://dkmh.huflit.edu.vn/DangKyHocPhan/DanhSachLopHocPhan?id={classId}&registType=KH");

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
                        var LT = split[0];
                        var TH = split[1];
                        hideId.TryGetValue(LT, out var LTHideId);
                        var isHasChildHide = childHide.TryGetValue(LT, out var dicChildHide);
                        if (isHasChildHide)
                        {
                            dicChildHide!.TryGetValue(TH, out var THHideId);
                            registerHide = LTHideId + "|" + THHideId + "|";
                        }
                    }
                    else
                    {
                        hideId.TryGetValue(code, out var value);
                        registerHide = value;
                    }

                    var responseRegistry = await _client.GetAsync(
                        $"https://dkmh.huflit.edu.vn/DangKyHocPhan/RegistUpdateScheduleStudyUnit?Hide={registerHide}&ScheduleStudyUnitOld=&acceptConflict=");

                    var status = await responseRegistry.Content.ReadFromJsonAsync<PortalResponseStatus>();
                    listBox.Items.Add(status?.Msg + $" {code}");
                    break;
                }
        }
    }

    /// <summary>
    ///     Login to portal website
    /// </summary>
    /// <returns></returns>
    public async Task<HttpStatusCode> Login()
    {
        HttpResponseMessage response;

        if (_cookie == "")
        {
            response = await _client.GetAsync(_url + "/Login");

            var hasCookie = response.Headers.TryGetValues("Set-Cookie", out var cookieValue);
            if (hasCookie)
                foreach (var value in cookieValue!)
                    _cookie = $"{value}";


            _client.DefaultRequestHeaders.Add("Cookie", _cookie);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(UserName), "txtTaiKhoan");
            content.Add(new StringContent(_password), "txtMatKhau");
            response = await _client.PostAsync(_url + "/Login", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(responseContent);
            var spanStatusLogin = document.DocumentNode.SelectNodes(@"//div[@class='loginbox-forgot']//span");
            if (spanStatusLogin is { Count: 1 })
                return HttpStatusCode.Redirect;
            return HttpStatusCode.OK;
        }
        else
        {
            _client.DefaultRequestHeaders.Add("Cookie", _cookie);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(UserName), "txtTaiKhoan");
            content.Add(new StringContent(_password), "txtMatKhau");
            await _client.PostAsync(_url + "Login", content);
        }

        return HttpStatusCode.Redirect;
    }

    public async Task Test(string path)
    {
        var response = await _client.GetAsync(path);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    ///     Redirect to DKMH page
    ///     <remarks>
    ///         Must call after
    ///         <see cref="Login" />
    ///     </remarks>
    /// </summary>
    public async Task ConnectToDkmh()
    {
        await _client.GetAsync(_url + "/Home/DangKyHocPhan");
    }

    private async Task RegisterSubject(List<string> subjectCode, Dictionary<string, string> hideId)
    {
        foreach (var code in subjectCode)
        {
            string registerHide;
            if (code.Contains('-'))
            {
                var split = code.Split('-');
                var LT = split[0];
                var TH = split[1];
                if (!_classChild.ContainsKey(LT) || !_classChild[LT].TryGetValue(TH, out var value)) return;

                var THHideID = value;
                registerHide = hideId[LT] + "|" + THHideID + "|";
            }
            else
            {
                registerHide = hideId[code];
            }

            var response = await _client.GetAsync(
                $"https://dkmh.huflit.edu.vn/DangKyHocPhan/RegistUpdateScheduleStudyUnit?Hide={registerHide}&ScheduleStudyUnitOld=&acceptConflict=");

            var status = await response.Content.ReadFromJsonAsync<PortalResponseStatus>();
            Console.WriteLine(status?.Msg + $" {code}");
        }
    }

    /// <summary>
    ///     Register subject and add it into a listbox
    /// </summary>
    /// <param name="subjectCode"></param>
    /// <param name="hideId"></param>
    /// <param name="listBox">A list box display registry status</param>
    private async Task RegisterSubject(List<string> subjectCode, Dictionary<string, string> hideId, ListBox listBox)
    {
        foreach (var code in subjectCode)
        {
            string registerHide;
            if (code.Contains('-'))
            {
                var split = code.Split('-');
                var LT = split[0];
                var TH = split[1];
                var THHideID = _classChild[LT][TH];
                registerHide = hideId[LT] + "|" + THHideID + "|";
            }
            else
            {
                registerHide = hideId[code];
            }

            var response = await _client.GetAsync(
                $"https://dkmh.huflit.edu.vn/DangKyHocPhan/RegistUpdateScheduleStudyUnit?Hide={registerHide}&ScheduleStudyUnitOld=&acceptConflict=");

            var status = await response.Content.ReadFromJsonAsync<PortalResponseStatus>();
            listBox.Items.Add(status?.Msg + $" {code}");
        }
    }


    /// <summary>
    ///     Get subject ID
    /// </summary>
    /// <returns>A list contain Subject ID to get SecretCode</returns>
    private async Task<List<string>?> GetSubjectIdList()
    {
        var response =
            await _client.GetAsync(@"https://dkmh.huflit.edu.vn/DangKyHocPhan/DanhSachHocPhan?typeId=KH&id=");
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

    private List<string>? GetClassCodeListFromFile(string path)
    {
        var classCodeList = new List<string>();

        var classList = File.ReadAllText(path);

        if (string.IsNullOrEmpty(classList))
        {
            Console.WriteLine("Bạn chưa nhập lớp học phần");
            Console.WriteLine("Đang kết thúc chương trình...");
            return null;
        }


        var classListSpread = classList.Split('\n');
        foreach (var classCode in classListSpread)
        {
            var classCodeNoReplace = classCode.Replace("\r", "");
            classCodeList.Add(classCodeNoReplace);
        }

        return classCodeList;
    }

    /// <summary>
    /// </summary>
    /// <param name="subjectIdList">Subject id</param>
    /// <returns>Dictionary contain hideId</returns>
    private async Task<Dictionary<string, string>> GetHideId(IEnumerable<string> subjectIdList)
    {
        var secretCode = new Dictionary<string, string>();

        var tasks = subjectIdList.Select(Body).ToList();
        await Task.WhenAll(tasks);

        //Get hideId using loop

        /*foreach (var classId in subjectIdList)
        {
            var response = await
                _client.GetAsync(
                    $"https://dkmh.huflit.edu.vn/DangKyHocPhan/DanhSachLopHocPhan?id={classId}&registType=KH");

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
                        var childList = contentDocument.DocumentNode
                            .SelectNodes($"//form//tr[@id='tr-of-{classCode}']//input[@type='radio']");

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
                    if (secretCode.ContainsKey(classCode)) continue;
                    secretCode.TryAdd(classCode, secret);
                    _classChild.TryAdd(classCode, childHideIdDictionary);
                }
        }*/


        return secretCode;

        async Task Body(string classId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cookie", _cookie);
            var response =
                await client.GetAsync(
                    $"https://dkmh.huflit.edu.vn/DangKyHocPhan/DanhSachLopHocPhan?id={classId}&registType=KH");

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
                    if (secretCode.ContainsKey(classCode)) continue;
                    secretCode.TryAdd(classCode, secret);
                    _classChild.TryAdd(classCode, childHideIdDictionary);
                }
        }
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
}