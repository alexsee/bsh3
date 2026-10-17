// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using Brightbits.BSH.Engine.Runtime;

namespace BSH.Test.Fakes;

/// <summary>
/// In-process stand-in for the named mutex and activate event. Tests use it
/// where the operating system cannot create named events.
/// </summary>
public sealed class InMemoryPreviewInstanceSync : IPreviewInstanceSync
{
    private readonly object gate = new();
    private readonly HashSet<string> held = new();
    private readonly Dictionary<string, List<Action>> listeners = new();

    public bool TryAcquireMutex(string name, out IDisposable ownership)
    {
        lock (gate)
        {
            if (!held.Add(name))
            {
                ownership = null;
                return false;
            }

            ownership = new Release(this, name);
            return true;
        }
    }

    public IDisposable Listen(string name, Action onSignal)
    {
        lock (gate)
        {
            if (!listeners.TryGetValue(name, out var list))
            {
                list = new List<Action>();
                listeners[name] = list;
            }

            list.Add(onSignal);
            return new Subscription(this, name, onSignal);
        }
    }

    public void Signal(string name)
    {
        Action[] snapshot;
        lock (gate)
        {
            if (!listeners.TryGetValue(name, out var list))
            {
                return;
            }

            snapshot = list.ToArray();
        }

        foreach (var listener in snapshot)
        {
            listener();
        }
    }

    private void ReleaseMutex(string name)
    {
        lock (gate)
        {
            held.Remove(name);
        }
    }

    private void Remove(string name, Action onSignal)
    {
        lock (gate)
        {
            if (listeners.TryGetValue(name, out var list))
            {
                list.Remove(onSignal);
            }
        }
    }

    private sealed class Release : IDisposable
    {
        private readonly InMemoryPreviewInstanceSync owner;
        private readonly string name;
        private bool disposed;

        public Release(InMemoryPreviewInstanceSync owner, string name)
        {
            this.owner = owner;
            this.name = name;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            owner.ReleaseMutex(name);
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly InMemoryPreviewInstanceSync owner;
        private readonly string name;
        private readonly Action onSignal;

        public Subscription(InMemoryPreviewInstanceSync owner, string name, Action onSignal)
        {
            this.owner = owner;
            this.name = name;
            this.onSignal = onSignal;
        }

        public void Dispose()
        {
            owner.Remove(name, onSignal);
        }
    }
}
