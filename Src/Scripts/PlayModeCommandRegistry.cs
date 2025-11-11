using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Synaptafin.PlayModeConsole.Constants;

namespace Synaptafin.PlayModeConsole {
  public class PlayModeCommandRegistry : MonoBehaviour {

    [Tooltip("Enable register methods with [Command] attribute in the scene.")]
    public bool enableCommandMark = false;

    [Header("Assets for registering commands cross scene")]
    [Tooltip("Use ScriptableObject to register commands cross scene. Create by: Assets/Create/" + COMMAND_REGISTRY_SO_PATH)]
    [SerializeField]
    private PlayModeCommandRegistrySO _commandRegistrySO;

    public List<Command> Commands { get; private set; }
    public string[] CommandNames => Commands.Select(static c => c.Name).ToArray();

    public static void RegisterGlobalGommand(Command command) {
      PlayModeCommandRegistry instance = FindFirstObjectByType<PlayModeCommandRegistry>();
      if (instance != null) {
        instance.RegisterCommand(command);
      } else {
        Debug.LogWarning("PlayModeCommandRegistry instance not found in the scene.");
      }
    }

    public void Awake() {
      Commands = _commandRegistrySO == null
        ? new List<Command>()
        : _commandRegistrySO.commands;

      // _commands["test"] = new Command("test", static () => { Debug.Log("Test command executed!"); });
    }

    public void RegisterCommand(Command command) {
      bool existed = Commands.Any(c => c.Id == command.Id);
      if (existed) {  // update
        int index = Commands.FindIndex(c => c.Id == command.Id);
        Commands[index] = command;
      } else {
        Commands.Add(command);
      }
    }


    public void RegisterCommand(Delegate handler, string name = default, string description = default) {

      Command command = new(handler);
      if (!string.IsNullOrEmpty(name)) {
        command.Name = name;
      }

      if (!string.IsNullOrEmpty(description)) {
        command.Description = description;
      }

      RegisterCommand(command);
    }

    public void RegisterCommand(Action handler, string name = default, string description = default) {
      RegisterCommand((Delegate)handler, name, description);
    }

    public void RegisterCommand(Action<int> handler, string name = default, string description = default) {
      RegisterCommand((Delegate)handler, name, description);
    }

    public void RegisterCommand(Action<float> handler, string name = default, string description = default) {
      RegisterCommand((Delegate)handler, name, description);
    }

    public void RegisterCommand(Action<string> handler, string name = default, string description = default) {
      RegisterCommand((Delegate)handler, name, description);
    }

    public void RegisterCommand(Action<bool> handler, string name = default, string description = default) {
      RegisterCommand((Delegate)handler, name, description);
    }

    // public void RegisterCommand(Func<IEnumerable<object>> handler, string name = default, string description = default) {
    //   IEnumerator<object> enumerator = handler().GetEnumerator();
    //   RegisterCommand((Delegate)(() => enumerator.MoveNext()), name, description);
    // }

    /// <summary>
    /// register command with any delegate type
    /// </summary>
    public void RegisterCommand<T>(T handler, string name = default, string description = default) where T : Delegate {
      RegisterCommand((Delegate)handler, name, description);
    }

    public Command GetCommandByName(string commandName) {
      return Commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
    }

    public void RemoveCommand(string name) {
      int num = Commands.RemoveAll(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
      if (num > 0) {
        Debug.Log($"Command with id {name} removed.");
      } else {
        Debug.LogWarning($"Command with id {name} not found.");
      }
    }

    private void RegisterCommandAttributeCommand() {
      MonoBehaviour[] mbs = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
      foreach (MonoBehaviour mb in mbs) {
        Type type = mb.GetType();
        MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (MethodInfo method in methods) {

          if (!Attribute.IsDefined(method, typeof(CommandAttribute))) {
            continue;
          }

          if (method.GetParameters().Length > 0 || method.ReturnType != typeof(void)) {
            Debug.LogWarning($"{type.Name}.{method.Name} but is not a parameterless void method. Skipping register.");
            continue;
          }
          Action action = (Action)Delegate.CreateDelegate(typeof(Action), mb, method);
          RegisterCommand(action);
        }

      }
    }
  }
}
