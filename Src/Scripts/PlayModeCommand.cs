using System;

namespace Synaptafin.PlayModeConsole {

  public abstract class CommandBase {
    protected string _id;
    protected string _group;
  }

  public class Command : CommandBase {
    public string Id => _id;
    public Action Handler { get; }

    public Command(string id, Action handler, string group = "all") {
      _id = id;
      _group = group;

      Handler = handler;
    }

    public void Execute() {
      Handler();
    }
  }

  public class ContextCommand : CommandBase {
    public Action<string> Handler { get; }

    public object Context { get; }

    public ContextCommand(string id, Action<string> handler, string group = "all") {
      _id = id;
      _group = group;

      Handler = handler;
    }

    public void Execute(string context) {
      Handler(context);
    }
  }

  public class Command<T> : CommandBase {
    public Action<T> action;
  }

}
