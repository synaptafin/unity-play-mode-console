using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Synaptafin.PlayModeConsole.Constants;

namespace Synaptafin.PlayModeConsole {
  public class PlayModeCommandRegistry : MonoBehaviour {

    [Header("Assets for registering commands cross scene")]
    [Tooltip("Use ScriptableObject to register commands cross scene. Create by: Assets/Create/" + COMMAND_REGISTRY_SO_PATH)]
    [SerializeField]
    private PlayModeCommandRegistrySO _commandRegistrySO;

    private Dictionary<string, Command> _commands;

    public static void RegisterGlobalGommand(string commandId, Action command) {
      PlayModeCommandRegistry instance = FindFirstObjectByType<PlayModeCommandRegistry>();
      if (instance != null) {
        instance.RegisterCommand(commandId, command);
      } else {
        Debug.LogWarning("PlayModeCommandRegistry instance not found in the scene.");
      }
    }

    public void Awake() {
      _commands = _commandRegistrySO == null
        ? new Dictionary<string, Command>()
        : _commandRegistrySO.commandDict;

      // _commands["test"] = new Command("test", static () => { Debug.Log("Test command executed!"); });
    }


    public void RegisterCommand(Command command) {
      bool flag = _commands.TryAdd(command.Id, command);
      if (!flag) {
        Debug.LogWarning($"Command with id {command.Id} already exists!");
      }
    }

    public void RegisterCommand(string commandId, Action command) {
      RegisterCommand(new Command(commandId, command));
    }

    public Command GetCommand(string commandId) {
      return _commands.GetValueOrDefault(commandId);
    }

    public void RemoveCommand(string commandId) {
      if (_commands.Remove(commandId)) {
        Debug.Log($"Command with id {commandId} removed.");
      } else {
        Debug.LogWarning($"Command with id {commandId} not found.");
      }
    }

    public string[] CommandIds() {
      return _commands.Keys.ToArray();
    }
  }
}
