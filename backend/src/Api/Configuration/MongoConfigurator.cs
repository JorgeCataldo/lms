using System.Collections.Generic;
using Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Api.Configuration
{
    public static class MongoConfigurator
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized)
                return;

            RegisterConventions();
        }

        private static void RegisterConventions()
        {
            ConventionRegistry.Register("MyConventions", new MongoConvention(), x => true);
            _initialized = true;
        }

        private class MongoConvention : IConventionPack
        {
            public IEnumerable<IConvention> Conventions => new List<IConvention>
            {
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
                new CamelCaseElementNameConvention()
            };
        }

        public static void InitializeDb(string connectionString, string database)
        {
            
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(database);
            
            if (!CollectionExists(db, DbContext.UserCollectionName))
                db.CreateCollection(DbContext.UserCollectionName);

            if (!CollectionExists(db, DbContext.EventCollectionName))
                db.CreateCollection(DbContext.EventCollectionName);

        }

        public static bool CollectionExists(IMongoDatabase db, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = db.ListCollections(new ListCollectionsOptions {Filter = filter});
            return collections.Any();
        }
    }
}