using System;
using System.Collections.Generic;

namespace Synaptafin.PlayModeConsole {
  public static class Constants {

    public const string INPUT_FIELD_DEFAULT_STYLE_CLASS = "input-area__text-field";
    public const string COMMAND_MATCHED_STYLE_CLASS = "command-matched";
    public const string COMMAND_UNMATCHED_STYLE_CLASS = "command-unmatched";
    public const string COMMAND_EXECUTABLE_STYLE_CLASS = "command-executable";

    public const string LABEL_SELECTED_STYLE_CLASS = "selected-command";

    public const string COMMAND_REGISTRY_SO_PATH = "PlayModeConsole/PlayModeCommandRegistry";

    public const string COMMAND_ITEM_UXML_PATH = "PlayModeConsole/CommandItem.uxml";
  }

  public static class Utils {
    private static readonly Dictionary<Type, string> s_shortTypeNames = new() {
      { typeof(int), "int" },
      { typeof(float), "float" },
      { typeof(double), "double" },
      { typeof(bool), "bool" },
      { typeof(string), "string" },
      { typeof(char), "char" },
      { typeof(long), "long" },
      { typeof(short), "short" },
      { typeof(byte), "byte" },
      { typeof(uint), "uint" },
      { typeof(ulong), "ulong" },
      { typeof(ushort), "ushort" },
      { typeof(sbyte), "sbyte" },
      { typeof(decimal), "decimal" }
    };

    public static string GetShortTypeName(Type type) {
      return s_shortTypeNames.TryGetValue(type, out string shortName) ? shortName : type.Name;
    }
  }
}
