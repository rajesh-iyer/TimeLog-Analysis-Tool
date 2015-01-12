using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ServiceCenter.Framework.Notification;
using TimeLogManager;

namespace MongoUtilities
{
    public class TimeLogMongoNotificationListener : NotificationListener
    {
        MongoDatabase database;

        public string MongoConnectionString { get; set; }
        public string RepositoryName { get; set; }

        public override void Notify(NotificationContext context)
        {
            var Context = context as TimeLogNotificationContext;
            if (Context != null && Context.Data != null && Context.Data.Workitems != null && Context.Data.Workitems.Count > 0)
            {
                MongoCollection<TimeLog> timeLogCollection = GetTimeLogCollection();

                CreateClassMap();

                Context.Data.Workitems.ForEach(record =>
                {
                    var beginDate = record.TrackingDate;        // e.g. 7/24/2013 00:00:00
                    var endDate = beginDate.AddDays(1);         // e.g. 7/25/2013 00:00:00

                    //get workitem record for given date - if exist delete and recreate
                    var query = Query.And(
                                    Query.EQ("Id", record.WorkitemId),
                                    Query.GTE("TrackingDate", beginDate),
                                    Query.LT("TrackingDate", endDate));
                    timeLogCollection.Remove(query);
                    timeLogCollection.Save<TimeLog>(record);
                });
            }
        }

        private MongoCollection<TimeLog> GetTimeLogCollection()
        {
            database = new MongoClient(new MongoUrl(MongoConnectionString)).GetServer().GetDatabase(RepositoryName);
            MongoCollection<TimeLog> timeLogCollection = database.GetCollection<TimeLog>("TimeLog");
            return timeLogCollection;
        }

        private static void CreateClassMap()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(TimeLog)))
            {
                BsonClassMap.RegisterClassMap<TimeLog>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.InternalId));
                    cm.IdMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);
                    cm.IdMemberMap.SetRepresentation(BsonType.ObjectId);

                    cm.GetMemberMap(c => c.TrackingDate).SetSerializationOptions(new DateTimeSerializationOptions { DateOnly = true });
                });
            }
        }
    }
}
