using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Synaptafin.PlayModeConsole.Constants;
using CommandUIItem = UnityEngine.UIElements.PlayModeConsoleCommandItemElement;

namespace Synaptafin.PlayModeConsole {

  [RequireComponent(typeof(UIDocument))]
  public class PlayModeCommandLine : MonoBehaviour {

    private const int CANDIDATE_LIMIT = 15;

    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private VisualTreeAsset _commandItemAsset;

    private VisualElement _root;
    private VisualElement _mainWindow;
    private TextField _inputArea;
    private CommandUIItem[] _commandItems = new CommandUIItem[CANDIDATE_LIMIT];
    private Button _runButton;
    private Label _commandDetail;
    private Label _typeHint;

    private PlayModeCommandRegistry _playModeCommandRegistry;

    private string _commandText;
    private string[] _argsText;
    private Command _targetCommand;
    private int _selectedCommandIndex = -1;
    private int _candidateCommandCount = 0;
    private float _typeHintOffset = 0;

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
          _commandDetail.transform.position = pos;
          _commandDetail.text = item.CommandDetail;
          _commandDetail.style.display = DisplayStyle.Flex;
        };
        commandList.Add(item);
        _commandItems[i] = item;
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
        foreach (CommandUIItem item in _commandItems) {
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

        if (_commandText != _targetCommand?.Name.ToLower()) {
          _inputArea.value = _commandItems[_selectedCommandIndex].CommandName;
          _commandText = _targetCommand.Name.ToLower();
          await TextFieldAsyncFocus();
          return;
        }

        if (_argsText.Length == _targetCommand.ParamCount) {
          ExecuteCommand();
        }

        await TextFieldAsyncFocus();
      }
    }

    // When Key which intend to focus on TextField pressed, will also handled by text field
    // Use async operation to focus text field at the end of key pressed event process
    private async Awaitable TextFieldAsyncFocus() {
      await Awaitable.EndOfFrameAsync();
      _inputArea.Focus();
    }

    private void UpdateSelectedLabel() {
      if (_selectedCommandIndex < 0) {
        _selectedCommandIndex = _candidateCommandCount - 1;
      }

      if (_selectedCommandIndex >= _candidateCommandCount) {
        _selectedCommandIndex = 0;
      }

      for (int i = 0; i < _candidateCommandCount; i++) {
        _commandItems[i].ClearClassList();
        if (i == _selectedCommandIndex) {
          _commandItems[i].AddToClassList(LABEL_SELECTED_STYLE_CLASS);
        }
      }

      _targetCommand = _commandItems[_selectedCommandIndex].Command;
      _typeHint.style.display = DisplayStyle.Flex;

      if (_targetCommand?.ParamCount > 0) {
        _typeHint.text = string.Join(' ', _targetCommand.ParamTypes.Select(static t => Utils.GetShortTypeName(t)));
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

      _targetCommand.Execute(_argsText);
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
      foreach (CommandUIItem item in _commandItems) {
        item.style.display = DisplayStyle.None;
      }

      string[] ignoreCaseParts = _inputArea.value.ToLower().Split(' ');
      if (ignoreCaseParts.Length == 0) {
        _commandText = "";
        return;
      }

      _commandText = ignoreCaseParts[0];
      _argsText = ignoreCaseParts.Length > 1
        ? ignoreCaseParts[1..].Select(static s => s.Trim()).ToArray()
        : Array.Empty<string>();

      _candidateCommandCount = 0;
      string[] commandNames = _playModeCommandRegistry.CommandNames;
      IEnumerable<Command> matchedCommands = _playModeCommandRegistry.Commands
        .Where(c => c.Name.ToLower().Contains(_commandText.ToLower()));

      foreach (Command c in matchedCommands) {
        if (c.Name.ToLower() == _commandText.ToLower()) {
          AddModifierClassToInputArea(COMMAND_MATCHED_STYLE_CLASS);
        }
        _commandItems[_candidateCommandCount].SetData(c);
        _commandItems[_candidateCommandCount].style.display = DisplayStyle.Flex;
        _candidateCommandCount++;
      }

      // when there is only one, hide all items
      if (_candidateCommandCount == 1 && _commandText == _commandItems[0].CommandName.ToLower()) {
        _commandItems[0].style.display = DisplayStyle.None;
      }

      UpdateSelectedLabel();
    }
  }
}
