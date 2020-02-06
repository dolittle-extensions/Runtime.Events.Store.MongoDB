// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Processing;
using Dolittle.Runtime.Events.Store.MongoDB.Streams;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing
{
    /// <summary>
    /// Represents an implementation of <see cref="IWriteEventsToStreams" />.
    /// </summary>
    public class EventsToStreamsWriter : IWriteEventsToStreams
    {
        readonly EventStoreConnection _connection;
        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsToStreamsWriter"/> class.
        /// </summary>
        /// <param name="connection">An <see cref="EventStoreConnection"/> to a MongoDB EventStore.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        public EventsToStreamsWriter(EventStoreConnection connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> Write(CommittedEvent @event, StreamId streamId, PartitionId partitionId)
        {
            try
            {
                using var session = _connection.MongoClient.StartSession();
                return await session.WithTransactionAsync(async (transaction, cancel) =>
                {
                    var streamPosition = (uint)_connection.StreamEvents.CountDocuments(transaction, Builders<StreamEvent>.Filter.Eq(_ => _.StreamIdAndPosition.StreamId, streamId.Value));

                    await _connection.StreamEvents.InsertOneAsync(
                        transaction,
                        @event.ToStreamEvent(streamId, streamPosition, partitionId),
                        null,
                        cancel).ConfigureAwait(false);

                    return true;
                 }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new EventStorePersistenceError("Error persisting event to MongoDB event store", ex);
            }
        }
    }
}