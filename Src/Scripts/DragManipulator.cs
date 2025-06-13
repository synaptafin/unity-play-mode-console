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
      _targetStartPosition = target.transform.position;
      _pointerStartPosition = evt.position;

      target.CapturePointer(evt.pointerId);
    }

    private void PointerMoveCallback(PointerMoveEvent evt) {
      if (!target.HasPointerCapture(evt.pointerId)) {
        return;
      }

      Vector3 pointerDelta = evt.position - _pointerStartPosition;
      target.transform.position = new Vector2(
        Mathf.Clamp(_targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width - 50),
        Mathf.Clamp(_targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height - 50)
      );
    }

    private void PointerUpCallback(PointerUpEvent evt) {
      target.ReleasePointer(evt.pointerId);
    }
  }
}
