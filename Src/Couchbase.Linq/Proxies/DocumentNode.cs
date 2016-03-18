using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Couchbase.Linq.Metadata;

namespace Couchbase.Linq.Proxies
{
    /// <summary>
    /// Represents the status of a node in a document.  Also keeps a collection of child nodes which are monitored for changes.
    /// </summary>
    internal class DocumentNode : ITrackedDocumentNode, ITrackedDocumentNodeCallback
    {
        public bool IsDeserializing { get; set; }
        public bool IsDirty { get; set; }
        public DocumentMetadata __metadata { get; set; }

        #region Child Document Tracking

        /// <summary>
        /// Keep a list of all child documents so that we can recurse down through the tree.
        /// Value of the dictionary is a reference count, in case the same document is beneath two properties.
        /// </summary>
        private readonly Dictionary<ITrackedDocumentNode, int> _childDocuments =
            new Dictionary<ITrackedDocumentNode, int>();

        public void AddChild(ITrackedDocumentNode child)
        {
            lock (_childDocuments)
            {
                if (_childDocuments.ContainsKey(child))
                {
                    _childDocuments[child] += 1;
                }
                else
                {
                    _childDocuments.Add(child, 1);

                    child.RegisterChangeTracking(this);
                }
            }
        }

        public void RemoveChild(ITrackedDocumentNode child)
        {
            lock (_childDocuments)
            {
                int refCount;
                if (_childDocuments.TryGetValue(child, out refCount))
                {
                    refCount -= 1;

                    if (refCount > 0)
                    {
                        _childDocuments[child] = refCount;
                    }
                    else
                    {
                        _childDocuments.Remove(child);

                        child.UnregisterChangeTracking(this);
                    }
                }
            }
        }

        public void RemoveAllChildren()
        {
            lock (_childDocuments)
            {
                foreach (var child in _childDocuments)
                {
                    child.Key.UnregisterChangeTracking(this);
                }

                _childDocuments.Clear();
            }
        }

        #endregion

        #region Change Tracking Callbacks

        /// <summary>
        /// List of callbacks to bubble dirty status back up the tree
        /// </summary>
        private readonly List<WeakReference<ITrackedDocumentNodeCallback>> _callbacks =
            new List<WeakReference<ITrackedDocumentNodeCallback>>();

        /// <summary>
        /// Register a callback to be triggered when this document is modified
        /// </summary>
        /// <param name="callback">Callback to be triggered</param>
        public virtual void RegisterChangeTracking(ITrackedDocumentNodeCallback callback)
        {
            lock (_callbacks)
            {
                var isAlreadyTracked = _callbacks.Any(p =>
                {
                    ITrackedDocumentNodeCallback target;

                    if (p.TryGetTarget(out target))
                    {
                        return target == callback;
                    }
                    else
                    {
                        return false;
                    }
                });

                if (!isAlreadyTracked)
                {
                    _callbacks.Add(new WeakReference<ITrackedDocumentNodeCallback>(callback));
                }
            }
        }

        /// <summary>
        /// Unregister a callback so it will no longer be called when this document is modified
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public virtual void UnregisterChangeTracking(ITrackedDocumentNodeCallback callback)
        {
            lock (_callbacks)
            {
                _callbacks.RemoveAll(p =>
                {
                    ITrackedDocumentNodeCallback target;

                    if (p.TryGetTarget(out target))
                    {
                        return target == callback;
                    }
                    else
                    {
                        return false;
                    }
                });
            }
        }

        /// <summary>
        /// Trigger any callbacks to inform them that this document has been modified
        /// </summary>
        private void TriggerCallbacks()
        {
            lock (_callbacks)
            {
                foreach (var callback in _callbacks)
                {
                    ITrackedDocumentNodeCallback target;

                    if (callback.TryGetTarget(out target))
                    {
                        target.DocumentModified();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Clears IsDeserializing and IsDirty on this document and all child documents.
        /// Does nothing if IsDeserialization is already false to prevent accidental infinite recursion.
        /// </summary>
        public virtual void ClearStatus()
        {
            if (IsDeserializing || IsDirty)
            {
                lock (_childDocuments)
                {
                    // Clear flags first to prevent accidental infinite recursion
                    IsDirty = false;
                    IsDeserializing = false;

                    foreach (var childDocument in _childDocuments.Select(p => p.Key))
                    {
                        childDocument.ClearStatus();
                    }
                }
            }
        }

        /// <summary>
        /// Flag this node as modified, and trigger any registered <see cref="ITrackedDocumentNodeCallback" /> callbacks.
        /// </summary>
        public virtual void DocumentModified()
        {
            if (!IsDirty && !IsDeserializing)
            {
                // Don't trigger callbacks if already marked as dirty before
                // And don't mark as dirty or trigger callbacks if still deserializing

                // Set as dirty before triggering callbacks to prevent accidental infinite recursion
                IsDirty = true;

                TriggerCallbacks();
            }
        }
    }
}
