using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Synaptafin.PlayModeConsole {

  public abstract class CommandBase {
    protected string _id;
    protected string _name;
    protected string _description;
    protected string _group;
    protected bool _isMonoBehaviour;
    protected string _gameObjectName;
  }

  public class Command : CommandBase {

    public string Id => _id;
    public Delegate Handler { get; }
    public int ParamCount { get; set; }
    public Type[] ParamTypes { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; }

    public string Group {
      get => _group;
      set => _group = value;
    }

    public bool IsMonoBehaviour => _isMonoBehaviour;
    public string GameObjectName => _gameObjectName;

    public Command(Delegate handler, string group = "all") {

      MethodInfo method = handler.Method;
      ParameterInfo[] parameters = method.GetParameters();
      foreach (ParameterInfo param in parameters) {
        if (!(param.ParameterType.IsPrimitive || param.ParameterType == typeof(string))) {
          Debug.LogWarning($"Parameter type {param.ParameterType} on method {method.Name} is not supported. Only primitive types. Skipped");
          return;
        }
      }

      Handler = handler;
      ParamCount = parameters.Length;
      ParamTypes = parameters.Where(static p => p.ParameterType.IsPrimitive || p.ParameterType == typeof(string)).Select(static p => p.ParameterType).ToArray();

      _id = $"{method.DeclaringType?.FullName}.{method.Name}";
      Name = method.Name;

      bool hasLambdaLikeName = method.Name.Contains("b__") || method.Name.StartsWith('<');
      bool isTypeCompilerGenerated = method.DeclaringType != null && method.DeclaringType.IsDefined(typeof(CompilerGeneratedAttribute), false);
      bool isLambdaExpression = hasLambdaLikeName || isTypeCompilerGenerated;

      Description = isLambdaExpression ? "Lambda Expression" : $"{method.DeclaringType?.Name}.{method.Name}";
      _isMonoBehaviour = method.DeclaringType != null && method.DeclaringType.IsSubclassOf(typeof(MonoBehaviour));
      _gameObjectName = _isMonoBehaviour && handler.Target is MonoBehaviour mb
        ? mb.gameObject.name
        : string.Empty;

      _group = group;
    }

    public void Execute(string[] args) {
      if (args.Length != ParamCount) {
        Debug.LogWarning($"Command '{Name}' expects {ParamCount} parameters, but got {args.Length}.");
        return;
      }

      object[] parsedParams = new object[ParamCount];
      for (int i = 0; i < ParamCount; i++) {
        object parsed = ParseParameters(args[i], ParamTypes[i]);
        if (parsed == null && ParamTypes[i].IsValueType) {
          Debug.LogWarning($"Failed to parse parameter '{args[i]}' to type {ParamTypes[i]}.");
          return;
        }
        parsedParams[i] = parsed;
      }

      try {
        Handler.DynamicInvoke(parsedParams);
      } catch (Exception ex) {
        Debug.LogWarning($"Error executing command '{Name}': {ex.Message}");
      }
    }

    private object ParseParameters(string args, Type paramType) {
      try {
        if (paramType == typeof(string)) {
          return args;
        } else if (paramType == typeof(int)) {
          return int.Parse(args);
        } else if (paramType == typeof(float)) {
          return float.Parse(args);
        } else if (paramType == typeof(bool)) {
          return bool.Parse(args);
        } else if (paramType == typeof(double)) {
          return double.Parse(args);
        } else if (paramType == typeof(long)) {
          return long.Parse(args);
        } else if (paramType == typeof(short)) {
          return short.Parse(args);
        } else if (paramType == typeof(byte)) {
          return byte.Parse(args);
        } else if (paramType == typeof(char)) {
          return char.Parse(args);
        } else if (paramType == typeof(uint)) {
          return uint.Parse(args);
        } else if (paramType == typeof(ulong)) {
          return ulong.Parse(args);
        } else if (paramType == typeof(ushort)) {
          return ushort.Parse(args);
        } else if (paramType == typeof(sbyte)) {
          return sbyte.Parse(args);
        }
      } catch (Exception ex) {
        Debug.LogError($"Failed to parse parameter '{args}' to type {paramType}: {ex.Message}");
      }
      return null;
    }

  }

  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
  public class CommandAttribute : Attribute {
    public string Id { get; }
    public string Group { get; }
    public string Description { get; }

    public CommandAttribute(string id = "test", string group = "all", string description = "") {
      Id = id;
      Group = group;
      Description = description;
    }
  }
}
