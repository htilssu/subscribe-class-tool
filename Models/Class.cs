using System;
using System.Collections;
using System.Collections.Generic;

namespace ClassRegisterApp.Models;

public class Class
{
    private string id;
    private string secret;
    private List<Class> children = [];

    public Class(string id, string secret)
    {
        this.id = id;
        this.secret = secret;
    }
    
    public bool HasChildren()
    {
        return children.Count > 0;
    }
    public Class? GetChild(string childId)
    {
        return children.Find(c => c.Id == childId);
    }
    
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
