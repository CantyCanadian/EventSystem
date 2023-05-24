using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using static Canty.Editor.EventEditor;

namespace Canty.Editor
{
    public class EventListBox : SearchTreeView<EventDataContainer>
    {
        public List<EventDataContainer> Data => _data;

        private Action<EventDataContainer> _onDelete;

        private string _targetFilter = string.Empty;

        public EventListBox(TreeViewState state) 
            : base(state, true, true, true)
        {
            Reload();
        }

        public void AddUnique(EventDataContainer container)
        {
            if (!_data.Any(c => c.EventName == container.EventName))
                _data.Add(container);
            _data = _data.GroupBy(data => data.EventName).Select(group => group.First()).ToList();
        }

        public void SetSelection(string name)
        {
            SetSelection(new List<int> { _content.First(pair => pair.Value.EventName == name).Key });
        }

        public void SetData(List<EventDataContainer> data)
        {
            _data = data;
            Reload();
        }

        public void RegisterOnDelete(Action<EventDataContainer> onDelete) => _onDelete = onDelete;

        public string GetTargetFilter() => _targetFilter;
        public void SetTargetFilter(string target) => _targetFilter = target;

        protected override EventDataContainer CreateObject()
        {
            string baseName = "NewBehaviourName";
            string name = baseName;

            int index = 1;
            while (_data.Any(container => container.EventName == name))
            {
                name = baseName + index.ToString();
                index++;
            }

            _targetFilter = string.Empty;
            return new EventDataContainer(DEFAULT_NAMESPACE, new List<string>(), "", name, null, CustomCodeLocation.OutsideClass, new List<VariableContainer>(), false, "", false);
        }

        protected override void DeleteObject(EventDataContainer obj)
        {
            _onDelete?.Invoke(obj);
            base.DeleteObject(obj);
        }

        protected override void FetchObject(EventDataContainer obj)
        {
            string path = GetFinalPath(obj);
            path = path.Substring(0, path.Length - 1);
            path = path.Substring(path.IndexOf("Assets"));
            UnityUtils.ShowFolderContentsFromPath(path);
        }

        protected override void SortObjects()
        {
            _data = _data.OrderBy(pair => pair.Target).ThenBy(pair => pair.EventName).ToList();
        }

        protected override bool IsObjectValid(EventDataContainer obj)
        {
            return string.IsNullOrEmpty(_targetFilter) || obj.Target == _targetFilter;
        }

        protected override string GetObjectName(EventDataContainer obj)
        {
            return obj.EventName;
        }

        protected override bool CanChangeExpandedState(TreeViewItem item) => false;
    }
}