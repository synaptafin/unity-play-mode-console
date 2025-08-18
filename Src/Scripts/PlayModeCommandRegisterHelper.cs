using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Synaptafin.PlayModeConsole {

  public class PlayModeCommandRegisterHelper : MonoBehaviour {
    [SerializeField] private PlayModeCommandRegistry _playModeCommandRegistry;
    [SerializeField] private GameObject[] _registeredObjects;
  }

}
