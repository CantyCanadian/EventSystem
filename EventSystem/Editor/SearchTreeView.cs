using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Canty.Editor
{
    public abstract class SearchTreeView<T> : SearchTreeView where T : class
    {
        protected readonly Dictionary<int, T> _content = new Dictionary<int, T>();
        protected List<T> _data = new List<T>();

        private bool _canCreate = true;
        private bool _createDisabled = false;

        public T GetSelectedObject()
        {
            IList<int> list = GetSelection();
            return list == null || list.Count == 0 ? null : _content[list.First()];
        }

        public IEnumerable<T> GetSelectedObjects()
        {
            IList<int> list = GetSelection();
            return list == null || list.Count == 0 ? null : list.Select(id => _content[id]);
        }

        public void SetSelectedObject(T obj)
        {
            if (_content.ContainsValue(obj))
                SetSelection(new List<int> { _content.First(pair => pair.Value == obj).Key }, TreeViewSelectionOptions.FireSelectionChanged);
        }

        public void SetSelectedObject(IEnumerable<T> obj)
        {
            SetSelection(obj.Select(o => _content.First(c => c.Value == o).Key).ToList(), TreeViewSelectionOptions.FireSelectionChanged);
        }

        public void SetCreateAssetDisabled(bool flag)
        {
            _createDisabled = flag;
        }

        protected virtual T CreateObject() { return null; }
        protected virtual void DeleteObject(T obj) 
        {
            SetSelection(new List<int>());
        }
        protected virtual void FetchObject(T obj) { }

        protected abstract void SortObjects();

        protected virtual bool IsObjectValid(T obj)
        {
            return true;
        }

        protected abstract string GetObjectName(T obj);

        protected override void DrawToolbar(Rect rect)
        {
            Rect workRect = rect;
            if (_canCreate)
            {
                workRect.height = TOOLBAR_HEIGHT;
                workRect.width = TOOLBAR_HEIGHT;

                using (new EditorGUI.DisabledScope(_createDisabled))
                {
                    if (GUI.Button(workRect, _createNewButton.Value, _iconButton.Value))
                    {
                        T obj = CreateObject();
                        if (obj != null)
                        {
                            _data.Add(obj);

                            Reload();
                            SetSelectedObject(obj);
                        }
                    }
                }

                workRect = rect;
                workRect.x += TOOLBAR_HEIGHT;
                workRect.width -= TOOLBAR_HEIGHT;
            }

            base.DrawToolbar(workRect);
        }

        protected override TreeViewItem BuildRoot()
        {
            SortObjects();

            _content.Clear();

            int id = 0;
            TreeViewItem root = new TreeViewItem()
            {
                id = ++id,
                depth = -1,
                displayName = "<Root>"
            };

            foreach (T container in _data)
            {
                if (!IsObjectValid(container))
                    continue;

                TreeViewItem item = new TreeViewItem()
                {
                    id = ++id,
                    displayName = GetObjectName(container),
                    depth = 0
                };

                _content.Add(id, container);
                root.AddChild(item);
            }

            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds) => _selectedIdsChanged?.Invoke(selectedIds);

        protected override void DoubleClickedItem(int id) => _itemDoubleClicked?.Invoke(id);

        protected override void ContextClickedItem(int id) => _itemRightClicked?.Invoke(id);

        protected override bool CanMultiSelect(TreeViewItem item) => _canSelectMultiple;

        protected SearchTreeView(TreeViewState state, bool canCreateAndDelete, bool canFetch, bool canSelectMultiple = false, MultiColumnHeader multiColumnHeader = null)
            : base(state, canSelectMultiple, multiColumnHeader)
        {
            _canCreate = canCreateAndDelete;

            if (canFetch)
            {
                _contextMenu.AddItem(new GUIContent("Open in Project"), false, () =>
                {
                    IList<int> ids = GetSelection();
                    if (ids != null && ids.Count > 0)
                        FetchObject(_content[ids[0]]);
                });
            }

            if (canCreateAndDelete)
            {
                if (_contextMenu.GetItemCount() > 0)
                    _contextMenu.AddSeparator(string.Empty);

                _contextMenu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    IList<int> ids = GetSelection();
                    foreach(int id in ids)
                    {
                        T obj = _content[id];
                        _data.Remove(obj);
                        _content.Remove(id);
                        DeleteObject(obj);
                        Reload();
                    }
                });

                RegisterItemRightClickedCallback((id) => _contextMenu.ShowAsContext());
            }
        }
    }

    public abstract class SearchTreeView : TreeView
    {
        protected enum SearchResultMode
        {
            Flat,
            Tree,
        }

        protected const float TOOLBAR_HEIGHT = 18.0f;

        protected static readonly Lazy<GUIStyle> _richTextLabel = new Lazy<GUIStyle>(() =>
            new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                richText = true,
                alignment = TextAnchor.MiddleLeft
            });

        protected readonly Lazy<GUIContent> _createNewButton = new Lazy<GUIContent>(() => EditorGUIUtility.IconContent("CreateAddNew"));
        protected readonly Lazy<GUIStyle> _iconButton = new Lazy<GUIStyle>(() => GUI.skin.GetStyle("IconButton"));

        protected readonly SearchField _searchField;
        protected readonly GenericMenu _contextMenu;

        protected SearchResultMode _searchResultMode = SearchResultMode.Flat;

        protected Action<IList<int>> _selectedIdsChanged;
        protected Action<int> _itemDoubleClicked;
        protected Action<int> _itemRightClicked;

        protected string _label = string.Empty;
        protected bool _canSelectMultiple = false;

        public void FocusSearchField()
        {
            _searchField.SetFocus();
        }

        public void SetLabel(string label)
        {
            _label = label;
        }

        public void RegisterSelectedIdsChangedCallback(Action<IList<int>> callback)
        {
            if (_selectedIdsChanged == null)
                _selectedIdsChanged = callback;
            else
                _selectedIdsChanged += callback;
        }

        public void RegisterItemDoubleClickedCallback(Action<int> callback)
        {
            if (_itemDoubleClicked == null)
                _itemDoubleClicked = callback;
            else
                _itemDoubleClicked += callback;
        }

        public void RegisterItemRightClickedCallback(Action<int> callback)
        {
            if (_itemRightClicked == null)
                _itemRightClicked = callback;
            else
                _itemRightClicked += callback;
        }

        public override void OnGUI(Rect rect)
        {
            Rect toolbarRect = rect;
            toolbarRect.height = TOOLBAR_HEIGHT;

            DrawToolbar(toolbarRect);

            Rect mainRect = rect;
            mainRect.yMin += TOOLBAR_HEIGHT;

            base.OnGUI(mainRect);
        }

        protected virtual void DrawToolbar(Rect rect)
        {
            Rect workRect = rect;
            using (new EditorGUILayout.HorizontalScope())
            {
                float oldWidth = workRect.width;
                workRect.y -= 1.0f;
                workRect.width = 60.0f;
                if (!string.IsNullOrEmpty(_label))
                    GUI.Label(workRect, _label);

                workRect.width = oldWidth - 60.0f;
                workRect.x += 60.0f;
                workRect.y += 1.0f;
                searchString = _searchField.OnGUI(workRect, searchString);
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            const float iconHeightWidth = 16.0f;
            const float spaceBetweenIconAndText = 4.0f;

            int depth = args.item.depth;
            if (args.item.parent != null && CanChangeExpandedState(args.item.parent))
            {
                depth += 1;
            }

            float rowY = args.row * args.rowRect.height;
            float rowX = depth * depthIndentWidth;

            if (args.item.icon != null)
            {
                float verticalCenteringOffset = (args.rowRect.height - iconHeightWidth) / 2;
                rowX += spaceBetweenIconAndText;
                var imageRect = new Rect(
                    x: rowX,
                    y: rowY + verticalCenteringOffset,
                    width: iconHeightWidth,
                    height: iconHeightWidth
                );

                rowX += iconHeightWidth;
                GUI.DrawTexture(imageRect, args.item.icon);
            }

            rowX += spaceBetweenIconAndText;
            Rect labelRect = new Rect(x: rowX, y: rowY, width: args.rowRect.width - rowX, height: args.rowRect.height);

            string text = ColorText(args.label);
            GUI.Label(labelRect, text, _richTextLabel.Value);
        }

        protected string ColorText(string text)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return text;

            try
            {
                return Regex.Replace(text, searchString.Replace(oldValue: " ", newValue: "|"), "<color=teal><b>$&</b></color>", RegexOptions.IgnoreCase);
            }
            catch
            {
                return text;
            }
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (hasSearch && _searchResultMode == SearchResultMode.Tree)
            {
                var result = new List<TreeViewItem>();
                SearchChildren(rootItem, searchString, result);
                return result;
            }

            return base.BuildRows(root);
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return true;

            bool orMode = searchString.Contains("|");
            if (orMode)
                return Regex.Match(item.displayName, searchString, RegexOptions.IgnoreCase | RegexOptions.Compiled).Success;

            string regexSearchString = searchString.Replace(oldValue: " ", newValue: "|");

            var uniqueWords = new HashSet<string>(collection: search.Split(separator: ' '), StringComparer.OrdinalIgnoreCase);

            foreach (Match match in Regex.Matches(item.displayName, regexSearchString, RegexOptions.IgnoreCase | RegexOptions.Compiled))
            {
                if (!match.Success)
                    return false;

                uniqueWords.Remove(match.Value);
            }

            return !uniqueWords.Any();
        }

        private void SearchChildren(TreeViewItem item, string search, List<TreeViewItem> result)
        {
            if (item.children == null)
                return;

            foreach (TreeViewItem child in item.children)
            {
                if (child == null)
                    continue;

                if (DoesItemMatchSearch(child, search))
                {
                    result.Add(child);
                    SearchChildren(child, search: null, result);
                }
                else
                {
                    int insertIndex = result.Count;
                    SearchChildren(child, search, result);

                    if (insertIndex != result.Count)
                        result.Insert(insertIndex, child);
                }
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds) => _selectedIdsChanged?.Invoke(selectedIds);

        protected override void DoubleClickedItem(int id) => _itemDoubleClicked?.Invoke(id);

        protected override void ContextClickedItem(int id) => _itemRightClicked?.Invoke(id);

        protected override bool CanMultiSelect(TreeViewItem item) => _canSelectMultiple;

        protected SearchTreeView(TreeViewState state, bool canSelectMultiple = false, MultiColumnHeader multiColumnHeader = null)
            : base(state, multiColumnHeader)
        {
            rowHeight = 21.0f;
            useScrollView = true;
            _canSelectMultiple = canSelectMultiple;
            _searchField = new SearchField();

            _contextMenu = new GenericMenu();
        }
    }
}