﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using System.Threading;

public class SpinLock : ILock
{
    private System.Threading.SpinLock _lock;

    public SpinLock()
    {
        _lock = new System.Threading.SpinLock();
    }

    public void WithLock(Action action)
    {
        bool lockAcquired = false;
        try
        {
            _lock.Enter(ref lockAcquired);
            action();
        }
        finally
        {
            if (lockAcquired) _lock.Exit(true);
        }
    }
}
