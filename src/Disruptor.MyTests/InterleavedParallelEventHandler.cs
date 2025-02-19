﻿using Disruptor;
using System;

public class InterleavedParallelEventHandler<T> : IEventHandler<T>
{
    private readonly int _partitionId;
    private readonly int _partitionCount;
    private readonly Action<T, long, bool> _action;

    public static InterleavedParallelEventHandler<T>[] Group(int parallelism, Action<T, long, bool> action)
    {
        var toReturn = new InterleavedParallelEventHandler<T>[parallelism];
        for(var i = 0; i < parallelism; i++)
        {
            toReturn[i] = new InterleavedParallelEventHandler<T>(i, parallelism, action);
        }

        return toReturn;
    }

    protected InterleavedParallelEventHandler(int partitionId, int partitionCount, Action<T, long, bool> action)
    {
        _partitionId = partitionId;
        _partitionCount = partitionCount;
        _action = action;
    }

    public void OnNext(T @event, long sequence, bool isEndOfBatch)
    {
        if (sequence % _partitionCount == _partitionId)
        {
            _action(@event, sequence, isEndOfBatch);
        }
    }

    public void OnEvent(T data, long sequence, bool endOfBatch)
    {
        throw new NotImplementedException();
    }
}
