using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Synaptafin.Editor.SelectionTracker {

  public interface IEntryService {
    List<Entry> Entries { get; }
    UnityEvent OnUpdated { get; }
    int CurrentSelectionIndex { get; set; }
    void RecordEntry(Entry selection);
    void RemoveEntry(Entry selection);
  }

  [Serializable]
  public class HistoryService : IEntryService {

    [SerializeReference]
    private List<Entry> _entryList = new();
    public List<Entry> Entries => _entryList.AsEnumerable().Reverse().ToList();

    [SerializeField]
    private UnityEvent _onUpdated = new();
    public UnityEvent OnUpdated => _onUpdated;

    private int _currentSelectionIndex = -1;
    private readonly int _sizeLimit = 100;

    public int CurrentSelectionIndex {
      get => _currentSelectionIndex;
      set => _currentSelectionIndex = _entryList.Count - value - 1;
    }

    public HistoryService() { }

    public void RecordEntry(Entry entry) {
      if (entry == null) {
        return;
      }

      if (_currentSelectionIndex > 0 && _entryList.Count > _currentSelectionIndex && entry.Equals(_entryList[_currentSelectionIndex])) {
        return;
      }

      int existIndex = _entryList.FindIndex(entry0 => entry0.Equals(entry));

      if (existIndex != -1) {
        Entry existedEntry = _entryList[existIndex];
        _entryList.RemoveAt(existIndex);
      }

      // if exist then update
      _entryList.Add(entry);

      while (_entryList.Count > _sizeLimit) {
        _entryList.RemoveAt(0);
      }

      ResetCurrentSelection();
      _onUpdated?.Invoke();
    }

    public void RemoveEntry(Entry entry) {

      _entryList.Remove(entry);
      _onUpdated?.Invoke();
    }

    public void RemoveAll() {
      _entryList.Clear();
      _onUpdated?.Invoke();
    }

    public void RemoveAll(Predicate<Entry> predicate) {
      _entryList.RemoveAll(predicate);
      _onUpdated?.Invoke();
    }

    public Entry PreviousSelection() {
      if (_entryList.Count == 0) {
        return null;
      }

      _currentSelectionIndex--;
      if (_currentSelectionIndex < 0) {
        _currentSelectionIndex = 0;
      }
      return _entryList[_currentSelectionIndex];
    }

    public Entry NextSelection() {
      if (_entryList.Count == 0) {
        return null;
      }

      _currentSelectionIndex++;
      if (_currentSelectionIndex >= _entryList.Count) {
        _currentSelectionIndex = _entryList.Count - 1;
      }
      return _entryList[_currentSelectionIndex];
    }

    public void ResetCurrentSelection() {
      _currentSelectionIndex = -1;
    }

  }

  [Serializable]
  public class MostVisitedService : IEntryService {

    [SerializeReference]
    private List<Entry> _slidingWindow = new();

    public List<Entry> Entries => _slidingWindow
      .GroupBy(static entry => entry)
      .OrderByDescending(static group => group.Count())
      .Select(static group => group.Key)
      .ToList();

    public UnityEvent OnUpdated { get; } = new();

    private readonly int _sizeLimit = 200;
    public int CurrentSelectionIndex { get; set; }

    public MostVisitedService() { }

    public void RecordEntry(Entry entry) {
      if (entry == null) {
        return;
      }

      _slidingWindow.Add(entry);

      if (_slidingWindow.Count > _sizeLimit) {
        _slidingWindow.RemoveAt(0);
      }

      OnUpdated?.Invoke();
    }

    public void RemoveEntry(Entry entry) {
      int count = _slidingWindow.RemoveAll(entry0 => entry0.Equals(entry));
      OnUpdated?.Invoke();
    }

    public void RemoveAll(Predicate<Entry> predicate) { }

    public void ResetCurrentSelection() { }
  }

  [Serializable]
  public class FavoritesService : IEntryService {

    private enum SizeOverFlowChoice {
      Ignore,
      RemoveOldest,
    }

    [SerializeReference]
    private List<Entry> _entries = new();
    private List<Entry> _entries0;

    public List<Entry> Entries => _entries.AsEnumerable().Reverse().ToList();
    public UnityEvent OnUpdated { get; } = new();
    public int CurrentSelectionIndex { get; set; }

    public FavoritesService() { }

    public void RecordEntry(Entry entry, bool isFavorite) {
      if (entry == null) {
        return;
      }

      entry.IsFavorite = isFavorite;
      RecordEntry(entry);
    }

    public void RecordEntry(Entry entry) {
      if (entry == null) {
        return;
      }

      int existIndex = _entries.FindIndex(entry0 => entry0.Equals(entry));

      if (EditorWindow.HasOpenInstances<FavoritesWindow>()) {
        if (existIndex != -1) {
          _entries[existIndex] = entry;
        } else {
          _entries.Add(entry);
        }
      } else if (entry.IsFavorite) {
        if (existIndex != -1) {
          _entries.RemoveAt(existIndex);
          _entries.Add(entry);
        } else {
          _entries.Add(entry);
        }
      }
      OnUpdated?.Invoke();
    }

    public void RemoveEntry(Entry entry) {

      if (entry == null) {
        return;
      }

      _entries.RemoveAll(entry0 => entry0 == entry);
      entry.IsFavorite = false;
      OnUpdated?.Invoke();
    }

    public void RemoveAll(Predicate<Entry> predicate) { }

    public void ApplyChanges() {
      _entries.RemoveAll(static entry => !entry.IsFavorite);
      _entries0 = new List<Entry>(_entries);
      OnUpdated?.Invoke();
    }

    public void DiscardChanges() {
      _entries = new List<Entry>(_entries0);
      _entries.ForEach(static entry => entry.IsFavorite = true);
      OnUpdated?.Invoke();
    }

    public void ResetCurrentSelection() { }

    public void StoreOriginalFavorites() {
      _entries0 = new List<Entry>(_entries);
    }

  }

  [Serializable]
  public class SceneComponentsService : IEntryService {

    [SerializeReference]
    private List<Entry> _entries = new();

    public List<Entry> Entries => _entries;
    public UnityEvent OnUpdated { get; } = new();
    public int CurrentSelectionIndex { get; set; }

    public void RecordEntry(Entry selection) {
      if (selection == null) {
        return;
      }

      if (_entries.Contains(selection)) {
        return;
      }

      _entries.Add(selection);
    }

    public void RemoveEntry(Entry selection) { }

    public void ResetCurrentSelection() { }

    public void Refresh() {
      OnUpdated?.Invoke();
    }
  }

  // Temporary service for component list in main window
  public class ComponentListSupportService : IEntryService {

    public List<Entry> Entries { get; } = new();
    public UnityEvent OnUpdated { get; } = new();

    public int CurrentSelectionIndex { get; set; }

    public void RecordEntry(Entry selection) {
      Entries.Add(selection);
    }

    public void RemoveEntry(Entry selection) {
      throw new Exception("Should not remove entry manually");
    }
  }
}
