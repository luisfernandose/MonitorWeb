using MongoDB.Driver;
using Queue.ViewModels;
using System;

namespace Queue.Models
{
    public class MongoHelper
    {
        public static IMongoClient client { get; set; }
        public static IMongoDatabase database { get; set; }

        //public static string MongoConnection = "mongodb+srv://monitor:Abc123456+@cluster0.24sb3.mongodb.net/MonitorTracker?retryWrites=true&w=majority";
        public static string MongoConnection = "mongodb://localhost:27017";        
        public static string MongoDatabase = "MonitorTracker";

        public static IMongoCollection<TrakerBase> TrakerBase { get; set; }
        public static IMongoCollection<InstalledHardwareViewModel> HardWareList { get; set; }
        public static IMongoCollection<InstalledProgramsViewModel> SoftWareList { get; set; }
        public static IMongoCollection<CaptureBase> UserCapture { get; set; }
        public static IMongoCollection<LogsViewModel> Logs { get; set; }
        internal static void ConnectToMongoService()
        {
            try
            {
                client = new MongoClient(MongoConnection);
                database = client.GetDatabase(MongoDatabase);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}