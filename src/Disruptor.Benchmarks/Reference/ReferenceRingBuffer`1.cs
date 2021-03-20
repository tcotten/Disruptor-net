using System;
using System.Runtime.CompilerServices;
using Disruptor.Dsl;

namespace Disruptor.Benchmarks.Reference
{
    public sealed class ReferenceRingBuffer<T> : ReferenceRingBuffer, IEventSequencer<T>
        where T : class
    {
        /// <summary>
        /// Construct a ReferenceRingBuffer with the full option set.
        /// </summary>
        /// <param name="eventFactory">eventFactory to create entries for filling the ReferenceRingBuffer</param>
        /// <param name="sequencer">sequencer to handle the ordering of events moving through the ReferenceRingBuffer.</param>
        /// <exception cref="ArgumentException">if bufferSize is less than 1 or not a power of 2</exception>
        public ReferenceRingBuffer(Func<T> eventFactory, ISequencer sequencer)
        : base(sequencer, typeof(T), _bufferPadRef)
        {
            Fill(eventFactory);
        }

        private void Fill(Func<T> eventFactory)
        {
            var entries = (T[])_entries;
            for (int i = 0; i < _bufferSize; i++)
            {
                entries[_bufferPadRef + i] = eventFactory();
            }
        }

        /// <summary>
        /// Construct a ReferenceRingBuffer with a <see cref="MultiProducerSequencer"/> sequencer.
        /// </summary>
        /// <param name="eventFactory"> eventFactory to create entries for filling the ReferenceRingBuffer</param>
        /// <param name="bufferSize">number of elements to create within the ring buffer.</param>
        public ReferenceRingBuffer(Func<T> eventFactory, int bufferSize)
            : this(eventFactory, new MultiProducerSequencer(bufferSize))
        {
        }

        /// <summary>
        /// Create a new multiple producer ReferenceRingBuffer with the specified wait strategy.
        /// </summary>
        /// <param name="factory">used to create the events within the ring buffer.</param>
        /// <param name="bufferSize">number of elements to create within the ring buffer.</param>
        /// <param name="waitStrategy">used to determine how to wait for new elements to become available.</param>
        /// <returns>a constructed ring buffer.</returns>
        /// <exception cref="ArgumentException">if bufferSize is less than 1 or not a power of 2</exception>
        public static ReferenceRingBuffer<T> CreateMultiProducer(Func<T> factory, int bufferSize, IWaitStrategy waitStrategy)
        {
            MultiProducerSequencer sequencer = new MultiProducerSequencer(bufferSize, waitStrategy);

            return new ReferenceRingBuffer<T>(factory, sequencer);
        }

        /// <summary>
        /// Create a new multiple producer ReferenceRingBuffer using the default wait strategy <see cref="BlockingWaitStrategy"/>.
        /// </summary>
        /// <param name="factory">used to create the events within the ring buffer.</param>
        /// <param name="bufferSize">number of elements to create within the ring buffer.</param>
        /// <returns>a constructed ring buffer.</returns>
        /// <exception cref="ArgumentException">if bufferSize is less than 1 or not a power of 2</exception>
        public static ReferenceRingBuffer<T> CreateMultiProducer(Func<T> factory, int bufferSize)
        {
            return CreateMultiProducer(factory, bufferSize, SequencerFactory.DefaultWaitStrategy());
        }

        /// <summary>
        /// Create a new single producer ReferenceRingBuffer with the specified wait strategy.
        /// </summary>
        /// <param name="factory">used to create the events within the ring buffer.</param>
        /// <param name="bufferSize">number of elements to create within the ring buffer.</param>
        /// <param name="waitStrategy">used to determine how to wait for new elements to become available.</param>
        /// <returns>a constructed ring buffer.</returns>
        /// <exception cref="ArgumentException">if bufferSize is less than 1 or not a power of 2</exception>
        public static ReferenceRingBuffer<T> CreateSingleProducer(Func<T> factory, int bufferSize, IWaitStrategy waitStrategy)
        {
            SingleProducerSequencer sequencer = new SingleProducerSequencer(bufferSize, waitStrategy);

            return new ReferenceRingBuffer<T>(factory, sequencer);
        }

        /// <summary>
        /// Create a new single producer ReferenceRingBuffer using the default wait strategy <see cref="BlockingWaitStrategy"/>.
        /// </summary>
        /// <param name="factory">used to create the events within the ring buffer.</param>
        /// <param name="bufferSize">number of elements to create within the ring buffer.</param>
        /// <returns>a constructed ring buffer.</returns>
        /// <exception cref="ArgumentException">if bufferSize is less than 1 or not a power of 2</exception>
        public static ReferenceRingBuffer<T> CreateSingleProducer(Func<T> factory, int bufferSize)
        {
            return CreateSingleProducer(factory, bufferSize, new BlockingWaitStrategy());
        }

        /// <summary>
        /// Create a new Ring Buffer with the specified producer type (SINGLE or MULTI)
        /// </summary>
        /// <param name="producerType">producer type to use <see cref="ProducerType" /></param>
        /// <param name="factory">used to create the events within the ring buffer.</param>
        /// <param name="bufferSize">number of elements to create within the ring buffer.</param>
        /// <param name="waitStrategy">used to determine how to wait for new elements to become available.</param>
        /// <returns>a constructed ring buffer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">if the producer type is invalid</exception>
        /// <exception cref="ArgumentException">if bufferSize is less than 1 or not a power of 2</exception>
        public static ReferenceRingBuffer<T> Create(ProducerType producerType, Func<T> factory, int bufferSize, IWaitStrategy waitStrategy)
        {
            switch (producerType)
            {
                case ProducerType.Single:
                    return CreateSingleProducer(factory, bufferSize, waitStrategy);
                case ProducerType.Multi:
                    return CreateMultiProducer(factory, bufferSize, waitStrategy);
                default:
                    throw new ArgumentOutOfRangeException(producerType.ToString());
            }
        }

        /// <summary>
        /// Get the event for a given sequence in the ReferenceRingBuffer.
        ///
        /// This call has 2 uses.  Firstly use this call when publishing to a ring buffer.
        /// After calling <see cref="ReferenceRingBuffer.Next()"/> use this call to get hold of the
        /// preallocated event to fill with data before calling <see cref="ReferenceRingBuffer.Publish(long)"/>.
        ///
        /// Secondly use this call when consuming data from the ring buffer.  After calling
        /// <see cref="ISequenceBarrier.WaitFor"/> call this method with any value greater than
        /// that your current consumer sequence and less than or equal to the value returned from
        /// the <see cref="ISequenceBarrier.WaitFor"/> method.
        /// </summary>
        /// <param name="sequence">sequence for the event</param>
        /// <returns>the event for the given sequence</returns>
        public T this[long sequence]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Util.Read<T>(_entries, _bufferPadRef + (int)(sequence & _indexMask));
            }
        }

        /// <summary>
        /// Sets the cursor to a specific sequence and returns the preallocated entry that is stored there.  This
        /// can cause a data race and should only be done in controlled circumstances, e.g. during initialisation.
        /// </summary>
        /// <param name="sequence">the sequence to claim.</param>
        /// <returns>the preallocated event.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ClaimAndGetPreallocated(long sequence)
        {
            _sequencerDispatcher.Sequencer.Claim(sequence);
            return this[sequence];
        }

        public override string ToString()
        {
            return $"ReferenceRingBuffer {{Type={typeof(T).Name}, BufferSize={_bufferSize}, Sequencer={_sequencerDispatcher.Sequencer.GetType().Name}}}";
        }

        /// <summary>
        /// Determines if a particular entry is available.  Note that using this when not within a context that is
        /// maintaining a sequence barrier, it is likely that using this to determine if you can read a value is likely
        /// to result in a race condition and broken code.
        /// </summary>
        /// <param name="sequence">The sequence to identify the entry.</param>
        /// <returns><c>true</c> if the value can be read, <c>false</c> otherwise.</returns>
        [Obsolete("Please don't use this method. It probably won't do what you think that it does.")]
        public bool IsPublished(long sequence)
        {
            return _sequencerDispatcher.Sequencer.IsAvailable(sequence);
        }

        /// <summary>
        /// Creates an event poller for this ring buffer gated on the supplied sequences.
        /// </summary>
        /// <param name="gatingSequences">gatingSequences to be gated on.</param>
        /// <returns>A poller that will gate on this ring buffer and the supplied sequences.</returns>
        public EventPoller<T> NewPoller(params ISequence[] gatingSequences)
        {
            return _sequencerDispatcher.Sequencer.NewPoller(this, gatingSequences);
        }

        /// <summary>
        /// Increment the ring buffer sequence and return a scope that will publish the sequence on disposing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If there is not enough space available in the ring buffer, this method will block and spin-wait using <see cref="AggressiveSpinWait"/>, which can generate high CPU usage.
        /// Consider using <see cref="TryPublishEvent()"/> with your own waiting policy if you need to change this behavior.
        /// </para>
        /// <para>
        /// Example:
        /// <code>
        /// using (var scope = _ReferenceRingBuffer.PublishEvent())
        /// {
        ///     var e = scope.Event();
        ///     // Do some work with the event.
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnpublishedEventScope PublishEvent()
        {
            var sequence = Next();
            return new UnpublishedEventScope(this, sequence);
        }

        /// <summary>
        /// Try to increment the ring buffer sequence and return a scope that will publish the sequence on disposing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will not block if there is not enough space available in the ring buffer.
        /// </para>
        /// <para>
        /// Example:
        /// <code>
        /// using (var scope = _ReferenceRingBuffer.TryPublishEvent())
        /// {
        ///     if (!scope.TryGetEvent(out var eventRef))
        ///         return;
        ///
        ///     var e = eventRef.Event();
        ///     // Do some work with the event.
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NullableUnpublishedEventScope TryPublishEvent()
        {
            var success = TryNext(out var sequence);
            return new NullableUnpublishedEventScope(success ? this : null, sequence);
        }

        /// <summary>
        /// Increment the ring buffer sequence by <paramref name="count"/> and return a scope that will publish the sequences on disposing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If there is not enough space available in the ring buffer, this method will block and spin-wait using <see cref="AggressiveSpinWait"/>, which can generate high CPU usage.
        /// Consider using <see cref="TryPublishEvents(int)"/> with your own waiting policy if you need to change this behavior.
        /// </para>
        /// <para>
        /// Example:
        /// <code>
        /// using (var scope = _ReferenceRingBuffer.PublishEvents(2))
        /// {
        ///     var e1 = scope.Event(0);
        ///     var e2 = scope.Event(1);
        ///     // Do some work with the events.
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnpublishedEventBatchScope PublishEvents(int count)
        {
            var endSequence = Next(count);
            return new UnpublishedEventBatchScope(this, endSequence + 1 - count, endSequence);
        }

        /// <summary>
        /// Try to increment the ring buffer sequence by <paramref name="count"/> and return a scope that will publish the sequences on disposing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will not block when there is not enough space available in the ring buffer.
        /// </para>
        /// <para>
        /// Example:
        /// <code>
        /// using (var scope = _ReferenceRingBuffer.TryPublishEvent(2))
        /// {
        ///     if (!scope.TryGetEvents(out var eventsRef))
        ///         return;
        ///
        ///     var e1 = eventRefs.Event(0);
        ///     var e2 = eventRefs.Event(1);
        ///     // Do some work with the events.
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NullableUnpublishedEventBatchScope TryPublishEvents(int count)
        {
            var success = TryNext(count, out var endSequence);
            return new NullableUnpublishedEventBatchScope(success ? this : null, endSequence + 1 - count, endSequence);
        }

        /// <summary>
        /// Holds an unpublished sequence number.
        /// Publishes the sequence number on disposing.
        /// </summary>
        public readonly struct UnpublishedEventScope : IDisposable
        {
            private readonly ReferenceRingBuffer<T> _ReferenceRingBuffer;
            private readonly long _sequence;

            public UnpublishedEventScope(ReferenceRingBuffer<T> ReferenceRingBuffer, long sequence)
            {
                _ReferenceRingBuffer = ReferenceRingBuffer;
                _sequence = sequence;
            }

            public long Sequence => _sequence;

            /// <summary>
            /// Gets the event at the claimed sequence number.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Event() => _ReferenceRingBuffer[_sequence];

            /// <summary>
            /// Publishes the sequence number.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _ReferenceRingBuffer.Publish(_sequence);
        }

        /// <summary>
        /// Holds an unpublished sequence number batch.
        /// Publishes the sequence numbers on disposing.
        /// </summary>
        public readonly struct UnpublishedEventBatchScope : IDisposable
        {
            private readonly ReferenceRingBuffer<T> _ReferenceRingBuffer;
            private readonly long _startSequence;
            private readonly long _endSequence;

            public UnpublishedEventBatchScope(ReferenceRingBuffer<T> ReferenceRingBuffer, long startSequence, long endSequence)
            {
                _ReferenceRingBuffer = ReferenceRingBuffer;
                _startSequence = startSequence;
                _endSequence = endSequence;
            }

            public long StartSequence => _startSequence;
            public long EndSequence => _endSequence;

            /// <summary>
            /// Gets the event at the specified index in the claimed sequence batch.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Event(int index) => _ReferenceRingBuffer[_startSequence + index];

            /// <summary>
            /// Publishes the sequence number batch.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _ReferenceRingBuffer.Publish(_startSequence, _endSequence);
        }

        /// <summary>
        /// Holds an unpublished sequence number.
        /// Publishes the sequence number on disposing.
        /// </summary>
        public readonly struct NullableUnpublishedEventScope : IDisposable
        {
            private readonly ReferenceRingBuffer<T> _ReferenceRingBuffer;
            private readonly long _sequence;

            public NullableUnpublishedEventScope(ReferenceRingBuffer<T> ReferenceRingBuffer, long sequence)
            {
                _ReferenceRingBuffer = ReferenceRingBuffer;
                _sequence = sequence;
            }

            /// <summary>
            /// Returns a value indicating whether the sequence was successfully claimed.
            /// </summary>
            public bool HasEvent => _ReferenceRingBuffer != null;

            /// <summary>
            /// Gets the event at the claimed sequence number.
            /// </summary>
            /// <returns>
            /// true if the sequence number was successfully claimed, false otherwise.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetEvent(out EventRef eventRef)
            {
                eventRef = new EventRef(_ReferenceRingBuffer, _sequence);
                return _ReferenceRingBuffer != null;
            }

            /// <summary>
            /// Publishes the sequence number if it was successfully claimed.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (_ReferenceRingBuffer != null)
                    _ReferenceRingBuffer.Publish(_sequence);
            }
        }

        /// <summary>
        /// Holds an unpublished sequence number.
        /// </summary>
        public readonly struct EventRef
        {
            private readonly ReferenceRingBuffer<T> _ReferenceRingBuffer;
            private readonly long _sequence;

            public EventRef(ReferenceRingBuffer<T> ReferenceRingBuffer, long sequence)
            {
                _ReferenceRingBuffer = ReferenceRingBuffer;
                _sequence = sequence;
            }

            public long Sequence => _sequence;

            /// <summary>
            /// Gets the event at the claimed sequence number.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Event() => _ReferenceRingBuffer[_sequence];
        }

        /// <summary>
        /// Holds an unpublished sequence number batch.
        /// Publishes the sequence numbers on disposing.
        /// </summary>
        public readonly struct NullableUnpublishedEventBatchScope : IDisposable
        {
            private readonly ReferenceRingBuffer<T> _ReferenceRingBuffer;
            private readonly long _startSequence;
            private readonly long _endSequence;

            public NullableUnpublishedEventBatchScope(ReferenceRingBuffer<T> ReferenceRingBuffer, long startSequence, long endSequence)
            {
                _ReferenceRingBuffer = ReferenceRingBuffer;
                _startSequence = startSequence;
                _endSequence = endSequence;
            }

            /// <summary>
            /// Returns a value indicating whether the sequence batch was successfully claimed.
            /// </summary>
            public bool HasEvents => _ReferenceRingBuffer != null;

            /// <summary>
            /// Gets the events for the associated sequence number batch.
            /// </summary>
            /// <returns>
            /// true if the sequence batch was successfully claimed, false otherwise.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetEvents(out EventBatchRef eventRef)
            {
                eventRef = new EventBatchRef(_ReferenceRingBuffer, _startSequence, _endSequence);
                return _ReferenceRingBuffer != null;
            }

            /// <summary>
            /// Publishes the sequence batch if it was successfully claimed.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (_ReferenceRingBuffer != null)
                    _ReferenceRingBuffer.Publish(_startSequence, _endSequence);
            }
        }

        /// <summary>
        /// Holds an unpublished sequence number batch.
        /// </summary>
        public readonly struct EventBatchRef
        {
            private readonly ReferenceRingBuffer<T> _ReferenceRingBuffer;
            private readonly long _startSequence;
            private readonly long _endSequence;

            public EventBatchRef(ReferenceRingBuffer<T> ReferenceRingBuffer, long startSequence, long endSequence)
            {
                _ReferenceRingBuffer = ReferenceRingBuffer;
                _startSequence = startSequence;
                _endSequence = endSequence;
            }

            public long StartSequence => _startSequence;
            public long EndSequence => _endSequence;

            /// <summary>
            /// Gets the event at the specified index in the claimed sequence batch.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Event(int index) => _ReferenceRingBuffer[_startSequence + index];
        }
    }
}
