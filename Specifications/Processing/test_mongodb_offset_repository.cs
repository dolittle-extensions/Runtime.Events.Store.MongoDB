﻿// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Logging;
using Dolittle.Runtime.Events.Specs.MongoDB;

namespace Dolittle.Runtime.Events.Processing.MongoDB.Specs
{
    public class test_mongodb_offset_repository : EventProcessorOffsetRepository
    {
        readonly a_mongo_db_connection _database_runner;

        public test_mongodb_offset_repository(a_mongo_db_connection database_runner, ILogger logger)
            : base(database_runner.Connection, logger)
        {
            _database_runner = database_runner;
        }

        ~test_mongodb_offset_repository()
        {
            _database_runner.Dispose();
        }
    }
}