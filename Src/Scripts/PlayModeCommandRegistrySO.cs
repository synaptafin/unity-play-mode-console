using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static Synaptafin.PlayModeConsole.Constants;

namespace Synaptafin.PlayModeConsole {
  /// <summary>
  /// Use ScriptableObject to scene-independently store commands 
  /// </summary>
  [CreateAssetMenu(fileName = "PlayModeCommandRegistry", menuName = COMMAND_REGISTRY_SO_PATH)]
  public class PlayModeCommandRegistrySO : ScriptableObject {
    public readonly Dictionary<string, Command> commandDict = new();
  }
}
