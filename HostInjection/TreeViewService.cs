using System.Collections.Generic;
using System.Linq;
using PowerShellToolsPro.Cmdlets.VSCode;

namespace PowerShellToolsPro.VSCode
{
    public class TreeViewService 
    {
        private static TreeViewService _instance;

        public static TreeViewService Instance {
            get {
                if (_instance == null)
                {
                    _instance = new TreeViewService();
                }
                return _instance;
            }
        }

        private Dictionary<string, TreeView> _treeViews = new Dictionary<string, TreeView>();

        public IEnumerable<TreeView> GetTreeViews()
        {
            return _treeViews.Values;
        }

        public void RegisterTreeView(TreeView treeView)
        {
            if (_treeViews.ContainsKey(treeView.Label))
            {
                _treeViews.Remove(treeView.Label);
            }
            _treeViews.Add(treeView.Label, treeView);
        }

        public void RefreshTreeView(string treeViewId)
        {
            var treeView = _treeViews[treeViewId];
            treeView._treeItemCache.Clear();
        }

        public IEnumerable<TreeItem> LoadChildren(string treeViewId, string path)
        {
            var treeView = _treeViews[treeViewId];
            if (treeView.LoadChildren == null) yield break;

            var treeItem = treeView._treeItemCache.ContainsKey(path) ? treeView._treeItemCache[path] : null;

            var treeItems = treeView.LoadChildren.Invoke(treeItem).Select(m => m.BaseObject).OfType<TreeItem>();
            foreach(var childItem in treeItems)
            {
                if (childItem != null && !string.IsNullOrEmpty(path))
                    childItem.Path = path + "\\";
                childItem.Path += childItem.Label;
                childItem.TreeViewId = treeViewId;
                childItem.CanInvoke = treeView.InvokeChild != null && !childItem.DisableInvoke;

                if (treeView._treeItemCache.ContainsKey(childItem.Path))
                {
                    treeView._treeItemCache[treeItem.Path] = childItem;
                }
                else 
                {
                    treeView._treeItemCache.Add(childItem.Path, childItem);
                }

                yield return childItem;
            }
        }

        public void InvokeChild(string treeViewId, string path)
        {
            var treeView = _treeViews[treeViewId];
            if (treeView.InvokeChild == null) return;

            var treeItem = treeView._treeItemCache[path];
            
            treeView.InvokeChild.Invoke(treeItem);
        }
    }
}