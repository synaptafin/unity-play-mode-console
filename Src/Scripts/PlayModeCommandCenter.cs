using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Synaptafin.PlayModeConsole {

  public class PlayModeCommandCenter : MonoBehaviour {
    [FormerlySerializedAs("_commandRegistry")] [SerializeField] private PlayModeCommandRegistry _playModeCommandRegistry;
  }

}
