using System;
using System.Linq;
using Synaptafin.PlayModeConsole;

namespace UnityEngine.UIElements {

  [UxmlElement("PlayModeConsoleCommandItem")]
  public partial class PlayModeConsoleCommandItemElement : VisualElement {

    private readonly Label _commandName;
    private readonly Label _commandDescription;

    public event Action<Vector2> OnHover;
    public event Action OnLeave;

    public Command Command { get; private set; }
    public string CommandName => _commandName.text;
    public string CommandDetail { get; set; }

    public PlayModeConsoleCommandItemElement() {
    }
    public PlayModeConsoleCommandItemElement(VisualElement root) : this() {
      Add(root);
      _commandName = root.Q<Label>("name");
      _commandDescription = root.Q<Label>("description");
      OnHoverManipulator hoverManipulator = new(this);
      hoverManipulator.OnHover += pos => {
        OnHover?.Invoke(pos);
      };
      hoverManipulator.OnLeave += () => OnLeave?.Invoke();
      this.AddManipulator(hoverManipulator);
    }

    public void SetData(Command command) {
      Command = command;
      _commandName.text = command.Name;
      _commandDescription.text = command.Description;

      CommandDetail = $"Parameters: {string.Join(", ", command.ParamTypes.Select(static t => t.Name))}\n"
        + $"Group: {command.Group}\n"
        + $"IsMonoBehaviour: {command.IsMonoBehaviour}\n"
        + (command.IsMonoBehaviour ? $"GameObject: {command.GameObjectName}" : string.Empty);
    }
  }
}
