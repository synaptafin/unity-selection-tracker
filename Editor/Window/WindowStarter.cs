using UnityEditor;
using UnityEngine;

namespace Synaptafin.Editor.SelectionTracker {

  public static class WindowStarter {

    [MenuItem(Constants.MENU_PATH_PREFIX + "History")]
    public static void HistoryWindow() {
      HistoryWindow wnd = EditorWindow.GetWindow<HistoryWindow>();

      GUIContent titleContent = new("History") {
        text = "History",
        tooltip = "History Window",
        image = EditorGUIUtility.IconContent(UnityBuiltInIcons.DEFAULT_ASSET_ICON_NAME).image
      };
      wnd.titleContent = titleContent;
    }

    [MenuItem(Constants.MENU_PATH_PREFIX + "Most Visited")]
    public static void MostVisitedWindow() {

      MostVisitedWindow wnd = EditorWindow.GetWindow<MostVisitedWindow>();
      GUIContent titleContent = new("Most Visited") {
        text = "Most Visited",
        tooltip = "Most Visited",
        image = EditorGUIUtility.IconContent(UnityBuiltInIcons.DEFAULT_ASSET_ICON_NAME).image
      };
      wnd.titleContent = titleContent;
    }

    [MenuItem(Constants.MENU_PATH_PREFIX + "Scene Components")]
    public static void SceneComponentsWindow() {
      SceneComponentsWindow wnd = EditorWindow.GetWindow<SceneComponentsWindow>();
      GUIContent titleContent = new("Scene Components") {
        text = "Scene Components",
        tooltip = "Scene Components Window",
        image = EditorGUIUtility.IconContent(UnityBuiltInIcons.DEFAULT_ASSET_ICON_NAME).image
      };
      wnd.titleContent = titleContent;
    }

    /* [MenuItem(Constants.MENU_PATH_PREFIX + "Favorites")] */
    public static void FavoritesWindow() {
      FavoritesWindow wnd = EditorWindow.GetWindow<FavoritesWindow>();
      GUIContent titleContent = new("Favorites") {
        text = "Favorites",
        tooltip = "Favorites Window",
        image = EditorGUIUtility.IconContent(UnityBuiltInIcons.DEFAULT_ASSET_ICON_NAME).image
      };
      wnd.titleContent = titleContent;
    }
  }
}

