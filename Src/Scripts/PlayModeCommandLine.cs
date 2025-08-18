using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Synaptafin.PlayModeConsole.Constants;

namespace Synaptafin.PlayModeConsole {

  [RequireComponent(typeof(UIDocument))]
  public class PlayModeCommandLine : MonoBehaviour {

    [SerializeField] private UIDocument _uiDocument;

    private VisualElement _root;
    private TextField _inputArea;
    private Label[] _candidateCommandLabels;
    private Button _runButton;

    private string _idOfCommandToBeExecuted;
    private int _selectedCommandIndex = -1;
    private int _candidateCommandCount = 0;

    private PlayModeCommandRegistry _playModeCommandRegistry;

    private void Start() {
      _root = _uiDocument.rootVisualElement;
      _root.style.display = DisplayStyle.None;
      _root.AddManipulator(new DragManipulator(_root));
      _inputArea = _root.Q<TextField>("input-area");
      _candidateCommandLabels = _root.Q<VisualElement>("command-list").Children().OfType<Label>().ToArray();
      _runButton = _root.Q<Button>("run-button");

      _playModeCommandRegistry = GetComponent<PlayModeCommandRegistry>();

      _inputArea.RegisterCallback<ChangeEvent<string>>(TextChangeCallback);
      _runButton.RegisterCallback<ClickEvent>(evt => {
        ExecuteCommand();
      });
    }

    private async void Update() {
      if (Input.GetKeyDown(KeyCode.Slash) && _root.style.display == DisplayStyle.None) {
        _root.style.display = DisplayStyle.Flex;
        _selectedCommandIndex = 0;
        await TextFieldAsyncFocus();
      }

      if (Input.GetKeyDown(KeyCode.Escape)) {
        _root.style.display = DisplayStyle.None;
        _inputArea.value = "";
        _selectedCommandIndex = 0;
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
        if (_inputArea.value.ToLower() == _idOfCommandToBeExecuted.ToLower()) {
          ExecuteCommand();
        }
        _inputArea.value = _candidateCommandLabels[_selectedCommandIndex].text;
        await TextFieldAsyncFocus();
      }
    }

    // When Key which intend to focus on TextField pressed, will also handled by text field
    // Use async operation to focus text field at the end of key pressed event process
    private async Awaitable TextFieldAsyncFocus() {
      await Awaitable.EndOfFrameAsync();
      _inputArea.Focus();
    }

    // Update candidates label based on input text
    private void UpdateCandidatesLabel() {

      // when text input changed always set first label as selected
      _selectedCommandIndex = 0;
      UpdateSelectedLabel();

      _idOfCommandToBeExecuted = "";
      AddModifierClassToInputArea(COMMAND_UNMATCHED_STYLE_CLASS);
      foreach (Label label in _candidateCommandLabels) {
        label.style.display = DisplayStyle.None;
      }

      string inputTextIgnoreCase = _inputArea.value.Trim().ToLower();
      if (string.IsNullOrEmpty(inputTextIgnoreCase)) {
        return;
      }
      _candidateCommandCount = 0;
      string[] commandIds = _playModeCommandRegistry.CommandIds();

      foreach (string id in commandIds) {
        string lowerCaseId = id.ToLower();
        if (!lowerCaseId.Contains(inputTextIgnoreCase)) {
          continue;
        }

        if (lowerCaseId == inputTextIgnoreCase) {
          _idOfCommandToBeExecuted = id;
          AddModifierClassToInputArea(COMMAND_MATCHED_STYLE_CLASS);
        }

        _candidateCommandLabels[_candidateCommandCount].text = id;
        _candidateCommandLabels[_candidateCommandCount].style.display = DisplayStyle.Flex;
        _candidateCommandCount++;
      }

      if (_candidateCommandCount == 1 && inputTextIgnoreCase == _candidateCommandLabels[0].text.ToLower()) {
        _candidateCommandLabels[0].style.display = DisplayStyle.None;
      }
    }

    private void UpdateSelectedLabel() {
      if (_selectedCommandIndex < 0) {
        _selectedCommandIndex = _candidateCommandCount - 1;
      }

      if (_selectedCommandIndex >= _candidateCommandCount) {
        _selectedCommandIndex = 0;
      }

      for (int i = 0; i < _candidateCommandCount; i++) {
        _candidateCommandLabels[i].ClearClassList();
        if (i == _selectedCommandIndex) {
          _candidateCommandLabels[i].AddToClassList(LABEL_SELECTED_STYLE_CLASS);
        }
      }
    }

    private void ExecuteCommand() {
      if (string.IsNullOrEmpty(_idOfCommandToBeExecuted)) {
        return;
      }

      Command command = _playModeCommandRegistry.GetCommand(_idOfCommandToBeExecuted);

      try {
        command.Handler();
      } catch (Exception e) {
        Debug.LogException(e);
      }
    }

    private void AddModifierClassToInputArea(string modifier) {
      _inputArea.ClearClassList();
      _inputArea.AddToClassList(INPUT_FIELD_DEFAULT_STYLE_CLASS);
      _inputArea.AddToClassList(modifier);
    }

    private void TextChangeCallback(ChangeEvent<string> evt) {
      UpdateCandidatesLabel();
    }
  }
}
