using System.Drawing;
using System.Linq.Expressions;
using System.Numerics;

namespace Realm.Extensions.RazorGui;

public interface IGuiHelper
{
    string BindAction(string elementId, string actionName);
    string Button(string text, Vector2 position, Size size, string? setId = null);
    string Checkbox(string text, Vector2 position, Size size, string? setId = null);
    string CreateForm(string[] inputs, string formName, string setId);
    string Input(Vector2 position, Size size, string? setId = null, bool masked = false);
    string Label(string text, Vector2 position, Size size, string? id = null);
    IDisposable OnClick(string elementId);
    string RememberFormCheckbox(string formId, string checkboxId);
    string Window(string title, Size size);
}
