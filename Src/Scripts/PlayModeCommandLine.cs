using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Synaptafin.PlayModeConsole.Constants;
using CommandUIItem = UnityEngine.UIElements.PlayModeConsoleCommandItemElement;

namespace Synaptafin.PlayModeConsole {

  [RequireComponent(typeof(UIDocument))]
  [RequireComponent(typeof(PlayModeCommandRegistry))]
  public class PlayModeCommandLine : MonoBehaviour {

    private static PlayModeCommandLine s_instance;

    private const int CANDIDATE_LIMIT = 15;

    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private VisualTreeAsset _commandItemAsset;

    private VisualElement _root;
    private VisualElement _mainWindow;
    private TextField _inputArea;
    private CommandUIItem[] _candidateCommandItems = new CommandUIItem[CANDIDATE_LIMIT];
    private Button _runButton;
    private Label _commandDetail;
    private Label _typeHint;

    private PlayModeCommandRegistry _playModeCommandRegistry;

    private string _commandText;
    private string[] _argsText;
    private int _selectedCommandIndex = -1;
    private int _candidateCommandCount = 0;
    private float _typeHintOffset = 0;

    private Action _onTargetCommandSet;
    private Command _targetCommand;
    private Command TargetCommand {
      get => _targetCommand;
      set {
        _targetCommand = value;
        _onTargetCommandSet?.Invoke();
      }
    }

    private void Awake() {
      if (s_instance != null && s_instance != this) {
        Destroy(gameObject);
        return;
      }
      s_instance = this;
      DontDestroyOnLoad(gameObject);
    }

    private void Start() {
      _root = _uiDocument.rootVisualElement;

      _root.style.display = DisplayStyle.None;
      _root.AddManipulator(new DragManipulator(_root));
      _mainWindow = _root.Q<VisualElement>("main-window");
      _mainWindow.RegisterCallback<TransitionEndEvent>(evt => {
        if (_mainWindow.style.top.value.value < 0) {
          _root.style.display = DisplayStyle.None;
        }
      });
      _inputArea = _root.Q<TextField>("input-area");

      _typeHint = _root.Q<Label>("type-hint");
      _typeHintOffset = _inputArea.resolvedStyle.fontSize * 2;  // take decorative single character into account

      _commandDetail = _root.Q<Label>("detail");
      VisualElement commandList = _root.Q<VisualElement>("command-list");
      for (int i = 0; i < CANDIDATE_LIMIT; i++) {
        CommandUIItem item = new(_commandItemAsset.Instantiate());
        item.style.display = DisplayStyle.None;
        item.OnHover += pos => {
#if UNITY_6000_2_OR_NEWER
          _commandDetail.style.translate = pos;
#else
          _commandDetail.transform.position = pos;
#endif          
          _commandDetail.text = item.CommandDetail;
          _commandDetail.style.display = DisplayStyle.Flex;
        };
        commandList.Add(item);
        _candidateCommandItems[i] = item;
      }

      _runButton = _root.Q<Button>("run-button");
      _playModeCommandRegistry = GetComponent<PlayModeCommandRegistry>();

      _root.RegisterCallback<PointerLeaveEvent>(evt => {
        _commandDetail.style.display = DisplayStyle.None;
      });
      _inputArea.RegisterCallback<ChangeEvent<string>>(SearchTextChangeCallback);
      _runButton.RegisterCallback<ClickEvent>(evt => {
        ExecuteCommand();
      });

      _onTargetCommandSet += ParameterHint;
    }

    private async void Update() {
      if (Input.GetKeyDown(KeyCode.Slash)) {
        _root.style.display = DisplayStyle.Flex;
        _selectedCommandIndex = 0;

        await TextFieldAsyncFocus();
        _mainWindow.style.top = Length.Percent(40);  // update uss property after async operation to trigger transition effect
      }

      if (Input.GetKeyDown(KeyCode.Escape)) {
        // _root.style.display = DisplayStyle.None;
        _inputArea.value = "";
        _selectedCommandIndex = 0;
        foreach (CommandUIItem item in _candidateCommandItems) {
          item.style.display = DisplayStyle.None;
        }

        await Awaitable.EndOfFrameAsync();  // update uss property after async operation to trigger transition effect
        _mainWindow.style.top = Length.Percent(-10);
      }

      if (_root.style.display == DisplayStyle.None) {
        return;
      }

      if (Input.GetKeyDown(KeyCode.DownArrow)) {
        _selectedCommandIndex++;
        UpdateSelectedLabel();
      }
      if (Input.GetKeyDown(KeyCode.UpArrow)) {
        _selectedCommandIndex--;
        UpdateSelectedLabel();
      }

      if (Input.GetKeyDown(KeyCode.Return)) {

        if (_commandText != TargetCommand?.Name.ToLower()) {
          _inputArea.value = _candidateCommandItems[_selectedCommandIndex].CommandName;
          _commandText = TargetCommand.Name.ToLower();
          await TextFieldAsyncFocus();
          return;
        }

        if (_argsText.Length == TargetCommand.ParamCount) {
          ExecuteCommand();
        }

        await TextFieldAsyncFocus();
      }
    }

    // When intend to focus on TextField with Key Return, will also handled by TextField which lead to unexpected behavior.
    // Use async operation to focus text field at the end of key pressed event process
    private async Awaitable TextFieldAsyncFocus() {
      await Awaitable.EndOfFrameAsync();
      _inputArea.Focus();
      _inputArea.cursorIndex = _inputArea.value.Length;
      _inputArea.selectIndex = _inputArea.value.Length;
    }

    private void UpdateSelectedLabel() {
      if (_selectedCommandIndex < 0) {
        _selectedCommandIndex = _candidateCommandCount - 1;
      }

      if (_selectedCommandIndex >= _candidateCommandCount) {
        _selectedCommandIndex = 0;
      }

      for (int i = 0; i < _candidateCommandCount; i++) {
        _candidateCommandItems[i].ClearClassList();
        if (i == _selectedCommandIndex) {
          _candidateCommandItems[i].AddToClassList(LABEL_SELECTED_STYLE_CLASS);
        }
      }
      TargetCommand = _candidateCommandItems[_selectedCommandIndex].Command;
    }

    private void ParameterHint() {
      if (TargetCommand?.ParamCount > 0) {
        _typeHint.text = string.Join(' ', TargetCommand.ParamTypes.Select(static t => Utils.GetShortTypeName(t)));
        float inputLength = _inputArea.MeasureTextSize(_inputArea.value, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).x;
        _typeHint.style.left = inputLength + _typeHintOffset;
        _typeHint.style.display = DisplayStyle.Flex;
      } else {
        _typeHint.style.display = DisplayStyle.None;
      }
    }

    private void ExecuteCommand() {
      if (string.IsNullOrEmpty(_commandText)) {
        return;
      }

      TargetCommand.Execute(_argsText);
    }

    private void AddModifierClassToInputArea(string modifier) {
      _inputArea.ClearClassList();
      _inputArea.AddToClassList(INPUT_FIELD_DEFAULT_STYLE_CLASS);
      _inputArea.AddToClassList(modifier);
    }

    private void SearchTextChangeCallback(ChangeEvent<string> evt) {

      // when text input changed always set first label as selected
      _selectedCommandIndex = 0;
      AddModifierClassToInputArea(COMMAND_UNMATCHED_STYLE_CLASS);
      foreach (CommandUIItem item in _candidateCommandItems) {
        item.style.display = DisplayStyle.None;
      }

      string[] ignoreCaseParts = _inputArea.value.Split(' ');
      if (ignoreCaseParts.Length == 0) {
        _commandText = "";
        return;
      }

      _commandText = ignoreCaseParts[0].ToLower();
      _argsText = ignoreCaseParts.Length > 1
        ? ignoreCaseParts[1..].Select(static s => s.Trim()).ToArray()
        : Array.Empty<string>();

      string[] commandNames = _playModeCommandRegistry.CommandNames;
      IEnumerable<Command> matchedCommands = _playModeCommandRegistry.Commands
        .Where(c => c.Name.ToLower().Contains(_commandText.ToLower()));

      _candidateCommandCount = 0;
      foreach (Command c in matchedCommands) {
        if (c.Name.ToLower() == _commandText.ToLower()) {
          AddModifierClassToInputArea(COMMAND_MATCHED_STYLE_CLASS);
        }
        _candidateCommandItems[_candidateCommandCount].SetData(c);
        _candidateCommandItems[_candidateCommandCount].style.display = DisplayStyle.Flex;
        _candidateCommandCount++;
        if (_candidateCommandCount >= CANDIDATE_LIMIT) {
          break;
        }
      }

      if (_candidateCommandCount == 0) {
        TargetCommand = null;
        return;
      }

      // when there is only one, hide all items
      if (_candidateCommandCount == 1 && _commandText == _candidateCommandItems[0].CommandName.ToLower()) {
        _candidateCommandItems[0].style.display = DisplayStyle.None;
      }

      UpdateSelectedLabel();
    }
  }
}
