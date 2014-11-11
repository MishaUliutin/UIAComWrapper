// (c) Copyright Microsoft, 2012.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
// All other rights reserved.



using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using UIAComWrapper;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public sealed class CacheRequest : DisposableBaseWithoutFinalizer
    {
        private UIAutomationClient.IUIAutomationCacheRequest _obj;
        private object _lock;
        private int _cRef;
        [ThreadStatic]
        private static Stack _cacheStack;
        internal static readonly CacheRequest DefaultCacheRequest = new CacheRequest();

        
        internal CacheRequest(UIAutomationClient.IUIAutomationCacheRequest obj)
        {
            Debug.Assert(obj != null);
            this._obj = obj;
            this._lock = new object();
        }

        public CacheRequest()
        {
            this._obj = Automation.Factory.CreateCacheRequest();
            this._lock = new object();
        }

        public IDisposable Activate()
        {
            CheckDisposed();

            this.Push();
            return new CacheRequestActivation(this);
        }

        public void Add(AutomationPattern pattern)
        {
            CheckDisposed();
            Utility.ValidateArgumentNonNull(pattern, "pattern");

            lock (this._lock)
            {
                this.CheckAccess();
                this._obj.AddPattern(pattern.Id);
            }
        }

        public void Add(AutomationProperty property)
        {
            CheckDisposed();
            Utility.ValidateArgumentNonNull(property, "property");

            lock (this._lock)
            {
                this.CheckAccess();
                this._obj.AddProperty(property.Id);
            }
        }

        private void CheckAccess()
        {
            if ((this._cRef != 0) || (this == DefaultCacheRequest))
            {
                throw new InvalidOperationException("Can't modify an active cache request");
            }
        }

        public CacheRequest Clone()
        {
            CheckDisposed();

            return new CacheRequest(this._obj.Clone());
        }

        public void Pop()
        {
            CheckDisposed();

            if (((_cacheStack == null) || (_cacheStack.Count == 0)) || (_cacheStack.Peek() != this))
            {
                throw new InvalidOperationException("Only the top cache request can be popped");
            }
            _cacheStack.Pop();
            lock (this._lock)
            {
                this._cRef--;
            }
        }

        public void Push()
        {
            CheckDisposed();

            if (_cacheStack == null)
            {
                _cacheStack = new Stack();
            }
            _cacheStack.Push(this);
            lock (this._lock)
            {
                this._cRef++;
            }
        }

        
        public AutomationElementMode AutomationElementMode
        {
            get
            {
                CheckDisposed();

                return (AutomationElementMode)this._obj.AutomationElementMode;
            }
            set
            {
                CheckDisposed();

                lock (this._lock)
                {
                    this.CheckAccess();
                    this._obj.AutomationElementMode = (UIAutomationClient.AutomationElementMode)value;
                }
            }
        }

        public static CacheRequest Current
        {
            get
            {
                if ((_cacheStack != null) && (_cacheStack.Count != 0))
                {
                    return (CacheRequest)_cacheStack.Peek();
                }
                return DefaultCacheRequest;
            }
        }

        internal static UIAutomationClient.IUIAutomationCacheRequest CurrentNativeCacheRequest
        {
            get
            {
                return CacheRequest.Current.NativeCacheRequest;
            }
        }

        public Condition TreeFilter
        {
            get
            {
                CheckDisposed();

                return Condition.Wrap(this._obj.TreeFilter);
            }
            set
            {
                CheckDisposed();
                Utility.ValidateArgumentNonNull(value, "TreeFilter");

                lock (this._lock)
                {
                    this.CheckAccess();
                    this._obj.TreeFilter = value.NativeCondition;
                }
            }
        }

        public TreeScope TreeScope
        {
            get
            {
                CheckDisposed();

                return (TreeScope)this._obj.TreeScope;
            }
            set
            {
                CheckDisposed();

                lock (this._lock)
                {
                    this.CheckAccess();
                    this._obj.TreeScope = (UIAutomationClient.TreeScope)value;
                }
            }
        }

        internal UIAutomationClient.IUIAutomationCacheRequest NativeCacheRequest
        {
            get
            {
                CheckDisposed();

                return this._obj;
            }
        }

        protected override void DisposeManagedResource()
        {
            Marshal.ReleaseComObject(_obj);
            base.DisposeManagedResource();
        }
    }

    internal class CacheRequestActivation : IDisposable
    {
        
        private CacheRequest _request;

        
        internal CacheRequestActivation(CacheRequest request)
        {
            this._request = request;
        }

        public void Dispose()
        {
            if (this._request != null)
            {
                this._request.Pop();
                this._request = null;
            }
        }
    }
}
