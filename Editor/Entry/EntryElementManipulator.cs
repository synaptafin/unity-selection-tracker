using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Synaptafin.Editor.SelectionTracker.Constants;

namespace Synaptafin.Editor.SelectionTracker {

  public class DragAndLeftClickManipulator : PointerManipulator {

    private readonly EntryElement _entryElement;
    private bool _isDragging = false;
    private bool _isPress = false;

    private Entry Entry => _entryElement.Entry;

    public DragAndLeftClickManipulator(EntryElement entryElement) {
      target = entryElement;
      _entryElement = entryElement;
    }

    protected override void RegisterCallbacksOnTarget() {
      target.RegisterCallback<PointerDownEvent>(PointerDownCallback);
      target.RegisterCallback<PointerUpEvent>(PointerUpCallback);
      target.RegisterCallback<PointerMoveEvent>(PointerMoveCallback);

      target.RegisterCallback<ClickEvent>(ClickCallback);
    }

    protected override void UnregisterCallbacksFromTarget() {
      target.UnregisterCallback<PointerDownEvent>(PointerDownCallback);
      target.UnregisterCallback<PointerUpEvent>(PointerUpCallback);
      target.UnregisterCallback<PointerMoveEvent>(PointerMoveCallback);
      target.UnregisterCallback<ClickEvent>(ClickCallback);
    }

    private void PointerDownCallback(PointerDownEvent evt) {
      if (evt.button != 0) {
        return;
      }
      _isPress = true;
      _isDragging = false;
      target.CapturePointer(0);
    }

    private void PointerUpCallback(PointerUpEvent evt) {
      _isPress = false;
      target.ReleasePointer(0);
    }

    private void PointerMoveCallback(PointerMoveEvent evt) {

      if (!_isPress) {
        return;
      }

      if (_isDragging) {
        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
        return;
      }

      // Condition Check after pointer is pressed
      if ((evt.localPosition - evt.position).magnitude < 5f) {
        return;
      }

      if (Entry == null) {
        return;
      }

      _isDragging = true;
      target.ReleasePointer(0);  // release if pointer is moved
      DragAndDrop.PrepareStartDrag();
      DragAndDrop.objectReferences = new[] { Entry.Ref };
      DragAndDrop.paths = new[] { AssetDatabase.GetAssetPath(Entry.Ref) };
      DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(Entry.Ref));
    }


    private async void ClickCallback(ClickEvent evt) {
      if (Entry == null) {
        return;
      }

      target.ReleasePointer(0);

      if (evt.button == 0 && evt.clickCount == 1) {
        bool unSelectable = Entry.RefState.HasFlag(RefState.Deleted)
          || Entry.RefState.HasFlag(RefState.Destroyed)
          || Entry.RefState.HasFlag(RefState.Unstaged)
          || Entry.RefState.HasFlag(RefState.Unloaded);

        if (unSelectable) {
          return;
        }


        if (PreferencePersistence.instance.GetToggleValue(UPDATE_WHEN_SELECTION_IN_TRACKER)) {
          // when VisualElement selected, pass the index to the service
          _entryElement.GetEntryService().CurrentSelectionIndex = _entryElement.Index;
        }

        await Task.Delay(700);
        Selection.activeObject = Entry.Ref;
      }

      if (evt.button == 0 && evt.clickCount == 2) {
        _entryElement.GetEntryService().ResetCurrentSelection();
        Entry.Open();
      }
    }
  }

  public class OnHoverManipulator : PointerManipulator {

    public class DetailInfoContent : PopupWindowContent {
      private VisualElement _root;

      public override VisualElement CreateGUI() {
        _root = UIAssetLocator.Instance.DetailInfoTemplate.Instantiate();
        return _root;
      }
    }

    private bool _isHover = false;
    private CancellationTokenSource _cts;

    public OnHoverManipulator(EntryElement entryElement) {
      target = entryElement;
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

      if (!PreferencePersistence.instance.GetToggleValue(DETAIL_ON_HOVER_KEY)) {
        return;
      }
      _cts?.Cancel();
      _cts = new CancellationTokenSource();
      CancellationToken token = _cts.Token;

      Task.Delay(1000, token).Wait();
      if (token.IsCancellationRequested || !_isHover) {
        return;
      }
    }

    public void PointerEnterCallback(PointerEnterEvent evt) {
      if (evt.button < 0) {
        _isHover = true;
      }
    }

    public void PointerLeaveCallback(PointerLeaveEvent evt) {
      _isHover = false;
    }
  }
}

