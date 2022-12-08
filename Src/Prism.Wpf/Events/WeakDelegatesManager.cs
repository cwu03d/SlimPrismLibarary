
using System;
using System.Collections.Generic;
using System.Linq;

namespace Prism.Events;

internal class WeakDelegatesManager
{
    private readonly List<DelegateReference> _listeners = new List<DelegateReference>();

    public void AddListener(Delegate listener)
    {
        this._listeners.Add(new DelegateReference(listener, false));
    }

    public void RemoveListener(Delegate listener)
    {
        this._listeners.RemoveAll(reference =>
        {
            //Remove the listener, and prune collected listeners
            Delegate target = reference.Target;
            return listener.Equals(target) || target == null;
        });
    }

    public void Raise(params object[] args)
    {
        this._listeners.RemoveAll(listener => listener.Target == null);

        foreach (Delegate handler in this._listeners.ToList().Select(listener => listener.Target).Where(listener => listener != null))
        {
            handler.DynamicInvoke(args);
        }
    }
}
