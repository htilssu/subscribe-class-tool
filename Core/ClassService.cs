using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassRegisterApp.Models;
using ClassRegisterApp.Infrastructure;

namespace ClassRegisterApp.Core;

public class ClassService
{
    private List<Class> _classes = [];

    public Class? GetClassById(string id)
    {
        return _classes.Find(c => c.Id == id);
    }

    public void AddClass(Class c)
    {
        if (_classes.Any(@class => @class.Id == c.Id))
        {
            _classes.Remove(_classes.Find(@class => @class.Id == c.Id)!);
        }

        _classes.Add(c);
    }

    /// <summary>
    /// Lấy thông tin lớp từ server theo classId nếu có thì trả về, không có thì trả về null
    /// </summary>
    /// <param name="classId">Class Id cần lấy từ server</param>
    /// <returns>Class nếu có từ server và không có lỗi, null nếu không có ở server và có lỗi</returns>
    public async Task<Class?> GetClassFromRemote(string classId)
    {
        var result = await RequestService.GetAsync<Class>($"/v1/secret/{classId}");
        if (result.IsOk)
        {
            return result.Result?.Id == null ? null : result.Result;
        }

        return null;
    }
}
