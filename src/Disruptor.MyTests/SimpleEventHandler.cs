using System;
using System.Linq;
using Disruptor.Dsl;
using Disruptor;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

public class SimpleEventHandler<T> : IEventHandler<T>
{
    private readonly Action<T, long, bool> _action;
    public SimpleEventHandler(Action<T, long, bool> action)
    {
        _action = action;
    }

    public void OnEvent(T data, long sequence, bool endOfBatch)
    {
        throw new NotImplementedException();
    }

    public void OnNext(T @event, long sequence, bool isEndOfBatch)
    {
        _action(@event, sequence, isEndOfBatch);
    }
}
