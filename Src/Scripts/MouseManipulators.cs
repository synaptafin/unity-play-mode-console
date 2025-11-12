using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Synaptafin.PlayModeConsole {
  public class DragManipulator : PointerManipulator {
    private Vector2 _targetStartPosition;
    private Vector3 _pointerStartPosition;

    public DragManipulator(VisualElement elt) {
      target = elt;
    }

    protected override void RegisterCallbacksOnTarget() {
      target.RegisterCallback<PointerDownEvent>(PointerDownCallback);
      target.RegisterCallback<PointerMoveEvent>(PointerMoveCallback);
      target.RegisterCallback<PointerUpEvent>(PointerUpCallback);
    }

    protected override void UnregisterCallbacksFromTarget() {
      target.UnregisterCallback<PointerDownEvent>(PointerDownCallback);
      target.UnregisterCallback<PointerMoveEvent>(PointerMoveCallback);
      target.UnregisterCallback<PointerUpEvent>(PointerUpCallback);
    }

    private void PointerDownCallback(PointerDownEvent evt) {

#if UNITY_6000_2_OR_NEWER
      _targetStartPosition = target.resolvedStyle.translate;
#else
      _targetStartPosition = target.transform.position;
#endif
      _pointerStartPosition = evt.position;

      target.CapturePointer(evt.pointerId);
    }

    private void PointerMoveCallback(PointerMoveEvent evt) {
      if (!target.HasPointerCapture(evt.pointerId)) {
        return;
      }

      Vector3 pointerDelta = evt.position - _pointerStartPosition;
#if UNITY_6000_2_OR_NEWER
      target.style.translate = new Vector2(
#else
      target.transform.position = new Vector2(
#endif
        Mathf.Clamp(_targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width - 50),
        Mathf.Clamp(_targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height - 50)
      );
    }

    private void PointerUpCallback(PointerUpEvent evt) {
      target.ReleasePointer(evt.pointerId);
    }
  }

  public class OnHoverManipulator : PointerManipulator {
    private bool _isHover = false;
    private CancellationTokenSource _cts;

    public event Action<Vector2> OnHover;
    public event Action OnLeave;

    public OnHoverManipulator(VisualElement elt) {
      target = elt;
    }

    protected override void RegisterCallbacksOnTarget() {
      target.RegisterCallback<PointerDownEvent>(PointerDownCallback);
      target.RegisterCallback<PointerUpEvent>(PointerUpCallback);
      target.RegisterCallback<PointerMoveEvent>(PointerMoveCallback);
      target.RegisterCallback<PointerEnterEvent>(PointerEnterCallback);
      target.RegisterCallback<PointerLeaveEvent>(PointerLeaveCallback);
    }

    protected override void UnregisterCallbacksFromTarget() {
      target.UnregisterCallback<PointerDownEvent>(PointerDownCallback);
      target.UnregisterCallback<PointerUpEvent>(PointerUpCallback);
      target.UnregisterCallback<PointerMoveEvent>(PointerMoveCallback);
      target.UnregisterCallback<PointerEnterEvent>(PointerEnterCallback);
      target.UnregisterCallback<PointerLeaveEvent>(PointerLeaveCallback);
    }

    public void PointerDownCallback(PointerDownEvent evt) {
      _isHover = false;
    }

    public void PointerUpCallback(PointerUpEvent evt) {
      _isHover = true;
    }

    public void PointerMoveCallback(PointerMoveEvent evt) {
      _cts?.Cancel();
      _cts = new CancellationTokenSource();
      CancellationToken token = _cts.Token;

      Task.Delay(500, token).Wait();
      if (token.IsCancellationRequested || !_isHover) {
        return;
      }

      // Update the position of the _detail element to follow the mouse
      Vector2 mousePosition = evt.position;
      OnHover?.Invoke(mousePosition);
    }

    public void PointerEnterCallback(PointerEnterEvent evt) {
      if (evt.button < 0) {
        _isHover = true;
      }
    }

    public void PointerLeaveCallback(PointerLeaveEvent evt) {
      _cts?.Cancel();
      _isHover = false;
      OnLeave?.Invoke();
    }
  }
}
