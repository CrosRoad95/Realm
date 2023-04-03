using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis;
using System.Drawing;
using System.Linq.Expressions;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace Realm.Extensions.RazorGui;

public class DisposableHelper : IDisposable
{
    private Action end;

    public DisposableHelper(Action end)
    {
        this.end = end;
    }

    public void Dispose()
    {
        end();
    }
}

internal class GuiHelper : IGuiHelper, IViewContextAware
{
    private readonly IHtmlHelper _htmlHelper;
    private Dictionary<string, string> _elementsIds = new();
    public GuiHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper;
    }

    void IViewContextAware.Contextualize(ViewContext viewContext)
    {
        if (_htmlHelper is IViewContextAware)
        {
            ((IViewContextAware)_htmlHelper).Contextualize(viewContext);
        }
    }

    private string AssignId(string id)
    {
        if (_elementsIds.ContainsKey(id))
            throw new Exception($"Id '{id}' already in use");

        var generatedId = "_" + Guid.NewGuid().ToString().Replace("-", "");
        _elementsIds[id] = generatedId;
        return generatedId;
    }

    public string Window(string title, Size size)
    {
        _htmlHelper.ViewContext.Writer.Write($"\tlocal window = guiProvider.window(\"{title}\", 0, 0, {size.Width}, {size.Height});");
        _htmlHelper.ViewContext.Writer.WriteLine($"\tguiProvider.centerWindow(window);");
        return string.Empty;
    }
    
    public string Label(string text, Vector2 position, Size size, string? setId = null)
    {
        if(setId != null)
        {
            var id = AssignId(setId);
            _htmlHelper.ViewContext.Writer.Write($"\tlocal {id} = guiProvider.label(\"{text}\", {position.X}, {position.Y}, {size.Width}, {size.Height}, window);");
        }
        else
        {
            _htmlHelper.ViewContext.Writer.Write($"\tguiProvider.label(\"{text}\", {position.X}, {position.Y}, {size.Width}, {size.Height}, window);");
        }
        return string.Empty;
    }

    public string Input(Vector2 position, Size size, string? setId = null, bool masked = false)
    {
        if(setId != null)
        {
            var id = AssignId(setId);
            _htmlHelper.ViewContext.Writer.Write($"\tlocal {id} = guiProvider.input({position.X}, {position.Y}, {size.Width}, {size.Height}, window);");
            if(masked)
            {
                _htmlHelper.ViewContext.Writer.Write($"\tguiProvider.setMasked({id}, true)");
            }
        }
        else
        {
            if (masked)
                throw new Exception("Maked require id");
            _htmlHelper.ViewContext.Writer.Write($"\tguiProvider.input({position.X}, {position.Y}, {size.Width}, {size.Height}, window);");
        }
        return string.Empty;
    }

    public string Checkbox(string text, Vector2 position, Size size, string? setId = null)
    {
        if (setId != null)
        {
            var id = AssignId(setId);
            _htmlHelper.ViewContext.Writer.Write($"\tlocal {id} = guiProvider.checkbox(\"{text}\", {position.X}, {position.Y}, {size.Width}, {size.Height}, false, window);");
        }
        else
        {
            _htmlHelper.ViewContext.Writer.Write($"\tguiProvider.checkbox(\"{text}\", {position.X}, {position.Y}, {size.Width}, {size.Height}, false, window);");
        }
        return string.Empty;
    }
    
    public string Button(string text, Vector2 position, Size size, string? setId = null)
    {
        if (setId != null)
        {
            var id = AssignId(setId);
            _htmlHelper.ViewContext.Writer.Write($"\tlocal {id} = guiProvider.button(\"{text}\", {position.X}, {position.Y}, {size.Width}, {size.Height}, window);");
        }
        else
        {
            _htmlHelper.ViewContext.Writer.Write($"\tguiProvider.button(\"{text}\", {position.X}, {position.Y}, {size.Width}, {size.Height}, window);");
        }
        return string.Empty;
    }

    public string CreateForm(string[] inputs, string formName, string setId)
    {
        if (inputs.Length == 0)
            throw new Exception("No inputs");

        var id = AssignId(setId);
        _htmlHelper.ViewContext.Writer.WriteLine($"\tlocal {id} = createForm(\"{formName}\", {{");
        foreach (var input in inputs)
        {
            _htmlHelper.ViewContext.Writer.WriteLine($"\t\t[\"{input}\"] = {_elementsIds[input]},");
        }
        _htmlHelper.ViewContext.Writer.Write("\t});");
        return string.Empty;
    }
    
    public string RememberFormCheckbox(string formId, string checkboxId)
    {
        var id1 = _elementsIds[formId];
        var id2 = _elementsIds[checkboxId];
        _htmlHelper.ViewContext.Writer.WriteLine($"\tif(guiProvider.tryLoadRememberedForm({id1}))then");
        _htmlHelper.ViewContext.Writer.WriteLine($"\t\tguiProvider.setSelected({id2}, true)");
        _htmlHelper.ViewContext.Writer.Write($"\tend");
        return string.Empty;
    }

    public string BindAction(string elementId, string actionName)
    {
        var id = _elementsIds[elementId];
        _htmlHelper.ViewContext.Writer.WriteLine($"\tguiProvider.onClick({id}, function()");
        _htmlHelper.ViewContext.Writer.WriteLine($"\t\tguiProvider.invokeAction(\"{actionName}\");");
        _htmlHelper.ViewContext.Writer.WriteLine($"\tend)");
        return string.Empty;
    }

    public string IsSelected(string elementId)
    {
        var id = _elementsIds[elementId];
        return $"guiProvider.getSelected({id})";
    }

    public IDisposable OnClick(string elementId)
    {
        var id = _elementsIds[elementId];
        _htmlHelper.ViewContext.Writer.WriteLine("\tguiProvider.onClick({id}, function()");
        return new DisposableHelper(EndOnClick);
    }
    
    //public string SubmitForm(string elementId)
    //{
    //    return $"{id}.submit()";
    //}

    public void EndOnClick()
    {
        _htmlHelper.ViewContext.Writer.Write("\nend)");
    }
    //public IDisposable Window(string title)
    //{
    //    _htmlHelper.ViewContext.Writer.WriteLine("\tBEGIN WINDOW " + title + Environment.NewLine);
    //    return new DisposableHelper(EndWindow);
    //}

    //public void EndWindow()
    //{
    //    _htmlHelper.ViewContext.Writer.Write(Environment.NewLine + "END WINDOW");
    //}
}
