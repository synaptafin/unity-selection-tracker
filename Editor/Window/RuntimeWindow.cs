using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Synaptafin.Editor.SelectionTracker {
  // Can't EditorWindow.GetWindow<BaseWindow<T>>()
  // Create derived classes for each window type
  public class HistoryWindow : BaseWindow<HistoryService>, IHasCustomMenu {

    public void AddItemsToMenu(GenericMenu menu) {
      menu.AddItem(
        new GUIContent("Hide Unloaded"),
        !_refStateFilter.HasFlag(RefState.Unloaded),
        () => ToggleStateFilterFlag(RefState.Unloaded)
      );
      menu.AddItem(new GUIContent("Clear/All", "Clear tooltips"), false, RemoveAll);
      menu.AddItem(new GUIContent("Clear/Deleted", "Clear deleted tooltips"), false, RemoveDeleted);
      menu.AddItem(new GUIContent("Clear/Destroyed", "Clear destroyed tooltips"), false, RemoveDestroyed);
    }

    protected override void AddContextMenu() {
      ContextualMenuManipulator contextMenuManipulator = new((evt) => {
        evt.menu.AppendAction("Show Unloaded", null, DropdownMenuAction.AlwaysEnabled);
        evt.menu.AppendAction("Show Destroyed", null, DropdownMenuAction.AlwaysEnabled);
        evt.menu.AppendAction("Clear", (_) => RemoveAll(), DropdownMenuAction.AlwaysEnabled);
      });
      _windowRoot.AddManipulator(contextMenuManipulator);
    }

    private void RemoveAll() {
      if (EditorUtility.DisplayDialog("Confirm", "Clear Records?", "Yes", "No")) {
        _entryService.RemoveAll();
      }
    }

    private void RemoveDeleted() {
      if (EditorUtility.DisplayDialog("Confirm", "Clear Deleted Records?", "Yes", "No")) {
        _entryService.RemoveAll(static (entry) => entry.RefState.HasFlag(RefState.Deleted));
      }
    }
    private void RemoveDestroyed() {
      if (EditorUtility.DisplayDialog("Confirm", "Clear Destroyed Records?", "Yes", "No")) {
        _entryService.RemoveAll(static (entry) => entry.RefState.HasFlag(RefState.Destroyed));
      }
    }
    private void ToggleStateFilterFlag(RefState state) {
      if (_refStateFilter.HasFlag(state)) {
        _refStateFilter &= ~state;
      } else {
        _refStateFilter |= state;
      }
      ReloadView();
    }
  }

  public class SceneComponentsWindow : BaseWindow<SceneComponentsService>, IHasCustomMenu {

    public new void OnEnable() {
      base.OnEnable();

      Scene activeScene = SceneManager.GetActiveScene();
      Utils.ScanAllComponentsInScene(activeScene);
    }

    public void AddItemsToMenu(GenericMenu menu) {
      menu.AddItem(
        new GUIContent("Refresh"),
        false,
        () => Refresh()
      );
    }

    protected override void AddContextMenu() {
      ContextualMenuManipulator contextMenuManipulator = new((evt) => {
        evt.menu.AppendAction("Refresh", (_) => Refresh(), DropdownMenuAction.AlwaysEnabled);
      });
      _windowRoot.AddManipulator(contextMenuManipulator);
    }

    private void Refresh() {

    }

  }
}
