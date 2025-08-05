# Play Mode Console

## Add Prefab To Scene

Prefab Location: `./Src/Prefab/PlayModeConsole.prefab`

## Reference Prefab Instance In Arbitrary MonoBehaviour

Reference the prefab instance as `PlayModeCommandRegistry` 

```cs
public class CommandRegisterHelper: MonoBehaviour {
    [SerializeField] private PlayModeCommandRegistry _playModeCommandRegistry;
    [SerializeField] private GameSettings _gameSettings;

    private void Start() {
        _playModeCommandRegistry..RegisterCommand("CommandName", () => {
          _gameSettings.SetVolume(100);
        });
    }
}
```

## Work With VContainer

