using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ClassRegisterApp.Models;

public class Class
{
    private string id;
    private string secret;
    private List<Class> children = [];

    /// <summary>
    /// Khởi tạo lớp
    /// </summary>
    /// <param name="id">id của class</param>
    /// <param name="secret">mã secret của class</param>
    public Class(string id, string secret)
    {
        this.id = id;
        this.secret = secret;
    }

    /// <summary>
    /// Kiểm tra xem class hiện tại có lớp thực hành hay không
    /// </summary>
    /// <returns></returns>
    public bool HasChildren()
    {
        return children.Count > 0;
    }

    /// <summary>
    /// Lấy lớp thực hành của lớp lý thuyết hiện tại
    /// Nếu không có sẽ trả về null
    /// </summary>
    /// <param name="childId">classId</param>
    /// <returns>Class thực hành</returns>
    public Class? GetChild(string childId)
    {
        return children.Find(c => c.Id == childId);
    }


    /// <summary>
    /// Thêm lớp thực hành vào lớp lý thuyết hiện tại
    /// </summary>
    /// <param name="child">Lớp thực hành</param>
    public void AddChild(Class child)
    {
        children.Add(child);
    }

    public string Id
    {
        get => id;
        set => id = value ?? throw new ArgumentNullException(nameof(value));
    }
    public string Secret
    {
        get => secret;
        set => secret = value ?? throw new ArgumentNullException(nameof(value));
    }
    public List<Class> Children
    {
        get => children;
        set => children = value ?? throw new ArgumentNullException(nameof(value));
    }
}

/// <summary>
/// Class chứa các extension function cho class
/// </summary>
public static class ClassExtension
{
    /// <summary>
    /// Lấy Id của class từ
    /// <paramref name="classes"/> sẽ là chuỗi người dùng nhập vào sẽ có dạng
    /// như <c>"12312312323"</c> hoặc <c>"12312512123-12412313213"</c>
    /// </summary>
    /// <param name="classes">class list</param>
    /// <param name="classInput">Thông tin class của nhập vào từ người dùng sẽ có dạng
    /// như <c>"12312312323"</c> hoặc <c>"12312512123-12412313213"</c></param>
    /// <returns>Trả về hideId dùng để đăng ký môn, nếu không có môn nào được tìm thấy thì sẽ trả về <c>string.Empty</c></returns>
    public static string GetHideId(this Class[] classes, string classInput)
    {
        if (!classInput.Contains('-')) return classes.FirstOrDefault(c => c.Id == classInput)?.Secret ?? string.Empty;

        var splitClass = classInput.Split('-');
        var parentClass = splitClass[0];
        var childClass = splitClass[1];
        var parent = classes.FirstOrDefault(c => c.Id == parentClass);
        var parentSecret = parent?.Secret ?? "";
        var childSecret = parent?.GetChild(childClass)?.Secret ?? "";
        if (string.IsNullOrEmpty(parentClass) || string.IsNullOrEmpty(childClass)) { return ""; }

        var secret = parentSecret + "|" + childSecret + "|";
        return secret;
    }
}
