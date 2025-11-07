using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Synaptafin.Editor.SelectionTracker {

  public class BaseWindow<T> : EditorWindow where T : IEntryService {

    public VisualTreeAsset rootVisualTreeAsset;

    protected T _entryService;
    protected VisualElement _windowRoot;
    protected RefState _refStateFilter = RefState.All;

    private VisualElement _entryContainer;
    private readonly List<EntryElement> _entryElementsCache = new();
    private string _searchText;

    public void OnEnable() {
      PreferencePersistence.instance.onUpdated += PreferencesUpdatedCallback;
      EntryServicePersistence.instance.TryGetService(out _entryService);
      _entryService?.OnUpdated.AddListener(EntryServiceUpdatedCallback);
    }

    public void OnDisable() {
      PreferencePersistence.instance.onUpdated -= PreferencesUpdatedCallback;
      _entryService?.OnUpdated.RemoveListener(EntryServiceUpdatedCallback);
    }

    public void CreateGUI() {
      if (_entryService == null) {
        return;
      }
      VisualElement root = rootVisualElement;
      if (rootVisualTreeAsset == null) {
        rootVisualTreeAsset = UIAssetLocator.Instance.WindowTemplate;
      }
      _windowRoot = rootVisualTreeAsset.CloneTree();
      root.Add(_windowRoot);

      _windowRoot.style.width = new StyleLength(Length.Percent(100));
      _windowRoot.style.height = new StyleLength(Length.Percent(100));

      ToolbarSearchField searchBar = _windowRoot.Q<ToolbarSearchField>("SearchField");
      searchBar.RegisterValueChangedCallback(evt => {
        _searchText = evt.newValue;
        ReloadView();
      });

      _entryContainer = _windowRoot.Q<VisualElement>("EntryContainer");

      for (int i = 0; i < _entryService.SizeLimit; i++) {
        EntryElement entryElement = new(i, _entryService);
        entryElement.style.display = DisplayStyle.None;
        _entryContainer.Add(entryElement);
        _entryElementsCache.Add(entryElement);
      }

      ReloadEntryList();
      ReloadView();
      AddContextMenu();
    }

    protected void ReloadView() {
      foreach (EntryElement elt in _entryElementsCache) {
        bool show = elt.Entry != null && IsMatch(elt) && PassFilter(elt.Entry);
        elt.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
      }
    }

    protected virtual void AddContextMenu() { }

    private void ReloadEntryList() {
      List<Entry> entries = _entryService.GetEntries;
      for (int i = 0; i < _entryElementsCache.Count; i++) {
        if (i < entries.Count) {
          _entryElementsCache[i].Entry = entries[i];
        } else {
          _entryElementsCache[i].Reset();
        }
      }
    }

    private bool IsMatch(EntryElement elt) {
      if (elt == null) {
        return false;
      }

      if (string.IsNullOrEmpty(_searchText)) {
        return true;
      }

      if (string.IsNullOrEmpty(elt.EntryText)) {
        return false;
      }

      string[] keywords = _searchText.Split(' ');
      bool isMatch = false;
      foreach (string keyword in keywords) {
        if (elt.EntryText.ToLower().Contains(keyword)) {
          isMatch = true;
          break;
        }
      }
      return isMatch;
    }

    private bool PassFilter(Entry entry) {
      if (entry == null) {
        return false;
      }

      if (_refStateFilter != 0 && _refStateFilter.HasFlag(entry.RefState)) {
        return true;
      }

      if (_refStateFilter == 0 && PreferencePersistence.instance.RefStateFilter == RefState.All) {
        return true;
      }

      return false;
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

    private void PreferencesUpdatedCallback() {
      ReloadView();
    }

    private void EntryServiceUpdatedCallback() {
      ReloadEntryList();
      ReloadView();
    }

  }

  public class MostVisitedWindow : BaseWindow<MostVisitedService> { }
  public class FavoritesWindow : BaseWindow<FavoritesService> {

    public new void OnEnable() {
      base.OnEnable();
      _entryService.OnUpdated.AddListener(OnFavoritesUpdated);
    }

    public new void CreateGUI() {
      base.CreateGUI();
      _entryService.StoreOriginalFavorites();
      _windowRoot.Q<VisualElement>("EditConfirm").style.display = DisplayStyle.Flex;

      Button _applyChangesButton = _windowRoot.Q<Button>("ApplyChanges");
      Button _discardChangesButton = _windowRoot.Q<Button>("DiscardChanges");

      _applyChangesButton.RegisterCallback<ClickEvent>((evt) => SaveChanges());
      _discardChangesButton.RegisterCallback<ClickEvent>((evt) => DiscardChanges());

      EnableEditConfirmButtons(false);
    }

    public void OnDestroy() {
      SaveChanges();
      _entryService.OnUpdated.RemoveListener(OnFavoritesUpdated);
    }

    public override void SaveChanges() {
      _entryService.ApplyChanges();
      EnableEditConfirmButtons(false);
      base.SaveChanges();
    }

    public override void DiscardChanges() {
      _entryService.DiscardChanges();
      EnableEditConfirmButtons(false);
      base.DiscardChanges();
    }

    private void OnFavoritesUpdated() {
      EnableEditConfirmButtons(true);
    }

    private void EnableEditConfirmButtons(bool enable) {
      _windowRoot.Q<Button>("ApplyChanges").SetEnabled(enable);
      _windowRoot.Q<Button>("DiscardChanges").SetEnabled(enable);
    }
  }
}
