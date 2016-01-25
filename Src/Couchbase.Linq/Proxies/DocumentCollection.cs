using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.Proxies
{
    /// <summary>
    /// Represents a collection that is also an <see cref="ITrackedDocumentNode"/> node.  It keeps a dirty status, and monitors
    /// all child nodes which also implement ITrackedDocumentNode for modifications.
    /// </summary>
    /// <typeparam name="T">Type of object in the collection</typeparam>
    class DocumentCollection<T> : Collection<T>, ITrackedDocumentNode
    {
        private readonly DocumentNode _documentNode = new DocumentNode();

        #region ITrackedDocumentNode

        // Redirect all ITrackedDocumentNode calls to the DocumentNode

        public bool IsDeserializing
        {
            get { return _documentNode.IsDeserializing; }
            set { _documentNode.IsDeserializing = value; }
        }

        public bool IsDirty
        {
            get { return _documentNode.IsDirty; }
            set { _documentNode.IsDirty = value; }
        }

        public string __id
        {
            get { return _documentNode.__id; }
            set { _documentNode.__id = value; }
        }

        public void RegisterChangeTracking(ITrackedDocumentNodeCallback callback)
        {
            _documentNode.RegisterChangeTracking(callback);
        }

        public void UnregisterChangeTracking(ITrackedDocumentNodeCallback callback)
        {
            _documentNode.UnregisterChangeTracking(callback);
        }

        public void ClearStatus()
        {
            _documentNode.ClearStatus();
        }

        #endregion

        protected override void ClearItems()
        {
            if (Count > 0)
            {
                base.ClearItems();

                _documentNode.RemoveAllChildren();
                _documentNode.DocumentModified();
            }
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            var status = item as ITrackedDocumentNode;
            if (status != null)
            {
                _documentNode.AddChild(status);
            }

            _documentNode.DocumentModified();
        }

        protected override void RemoveItem(int index)
        {
            var status = this[index] as ITrackedDocumentNode;
            if (status != null)
            {
                _documentNode.RemoveChild(status);
            }

            base.RemoveItem(index);

            _documentNode.DocumentModified();
        }

        protected override void SetItem(int index, T item)
        {
            var status = this[index] as ITrackedDocumentNode;
            if (status != null)
            {
                // Remove the old value from the child collection
                _documentNode.RemoveChild(status);
            }

            base.SetItem(index, item);

            status = item as ITrackedDocumentNode;
            if (status != null)
            {
                // Add the new value to the child collection
                _documentNode.AddChild(status);
            }

            _documentNode.DocumentModified();
        }
    }
}
