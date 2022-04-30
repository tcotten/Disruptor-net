using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.Models;
/// <summary>
/// Only use IValueProducer if T (struck) is 16 bytes or less or basically just a single value such as a primitive type
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IValueProducer<T>
{
    /// <summary>
    /// Send a <typeparamref name="T"/> to the disruptor to process.
    /// </summary>
    /// <param name="createProducerAction"></param>
    void ProduceEvents(Action<T> createDisruptorAction);
    /// <summary>
    /// Send a list of <typeparamref name="T"/> to the disruptor to process.
    /// </summary>
    /// <param name="createProducerListActions"></param>
    void ProduceEvents(Action<List<T>> createDisruptorListActions);
}
