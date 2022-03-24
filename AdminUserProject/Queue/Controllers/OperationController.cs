using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Queue.DAL;
using Queue.Models;
using Queue.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Queue.Controllers;

namespace Queue.Controllers
{
    public class OperationController : Controller
    {
        private QueueContext db = new QueueContext();

        public OperationController()
        {
            MongoHelper.ConnectToMongoService();
        }

        #region AddMethods
        public bool AddHardware(List<InstalledHardwareViewModel> o)
        {
            try
            {
                MongoHelper.HardWareList = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");
                MongoHelper.HardWareList.InsertMany(o);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddSoftware(List<InstalledProgramsViewModel> o)
        {
            try
            {
                MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                MongoHelper.SoftWareList.InsertMany(o);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool AddTracker(List<TrakerBase> tb)
        {
            try
            {
                MongoHelper.TrakerBase = MongoHelper.database.GetCollection<TrakerBase>("TrackerTime");
                MongoHelper.TrakerBase.InsertMany(tb);

                return true;
            }
            catch (Exception ex)
            {
                LogsViewModel lvm = new LogsViewModel();
                lvm.datelog = DateTime.Now;
                lvm.LogRegister = ex.Message + "-" + ex.InnerException.Message;
                lvm.Module = "AddTracker";
                AddLog(lvm);

                return false;
            }
        }

        public bool AddCapture(CaptureBase cb)
        {
            try
            {
                MongoHelper.UserCapture = MongoHelper.database.GetCollection<CaptureBase>("WindowsCapture");
                MongoHelper.UserCapture.InsertOne(cb);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddLog(LogsViewModel lg)
        {
            try
            {
                MongoHelper.Logs = MongoHelper.database.GetCollection<LogsViewModel>("Log");
                MongoHelper.Logs.InsertOne(lg);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region UpdateMethods
        public void UpdateHardware(InstalledHardwareViewModel o)
        {
            try
            {
                var collection = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");
                var builder = Builders<InstalledHardwareViewModel>.Filter;
                var filter = builder.Eq("_id", o._id);
                var update = Builders<InstalledHardwareViewModel>.Update.Set("status", false).Set("uninstalldate", DateTime.Now);
                var result = collection.UpdateOne(filter, update);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateSotfware(InstalledProgramsViewModel o)
        {
            try
            {
                var collection = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                var builder = Builders<InstalledProgramsViewModel>.Filter;
                var filter = builder.Eq("_id", o._id);
                var update = Builders<InstalledProgramsViewModel>.Update.Set("Status", false).Set("uninstalldate", DateTime.Now);
                var result = collection.UpdateOne(filter, update);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region GetMethods
        public List<InstalledProgramsViewModel> GetSoftWare(string IdCompany, string Pc)
        {
            try
            {
                MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                var builder = Builders<InstalledProgramsViewModel>.Filter;
                var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("Pc", Pc) & builder.Eq("Status", true);

                var results = MongoHelper.SoftWareList.Find(filter).ToList();
                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<InstalledHardwareViewModel> GetHardware(string IdCompany, string Pc)
        {
            try
            {
                MongoHelper.HardWareList = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");
                var builder = Builders<InstalledHardwareViewModel>.Filter;
                var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("Pc", Pc) & builder.Eq("status", true);

                var results = MongoHelper.HardWareList.Find(filter).ToList();
                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<CaptureBase> GeImage(string IdCompany)
        {
            try
            {
                MongoHelper.UserCapture = MongoHelper.database.GetCollection<CaptureBase>("WindowsCapture");
                var builder = Builders<CaptureBase>.Filter;
                var filter = builder.Eq("IdCompany", IdCompany);

                var results = MongoHelper.UserCapture.Find(filter).ToList();
                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<AutomaticTakeTimeModel> GetSoftWareClasification(string IdCompany)
        {
            try
            {
                //MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                //var builder = Builders<InstalledProgramsViewModel>.Filter;

                //var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("Status", true);

                //var results = MongoHelper.SoftWareList.Find(filter).ToList();


                var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                             where e.IdEmpresa == IdCompany
                             select new AutomaticTakeTimeModel
                             {
                                 Application = e.Application

                             }).Distinct().ToList();

                return query;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region Stadisticas

        public BasicStatsModel MoreUsedApp(string idcompany, DateTime fromdate, DateTime todate)
        {
            BasicStatsModel bm = new BasicStatsModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany
                         && e.Date >= fromdate && e.Date <= todate
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date
                         }).Distinct().ToList();

            foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Application).ToList())
            {
                var item = grouping;

                double? time = query.Where(t => t.Application == item.Key).Select(f => f.Time).Sum();
                bm.labels.Add(item.Key);
                double? totalminutes = 0;

                if (time != null && time > 0)
                    totalminutes = (time / 60);

                bm.data.Add(Math.Round(totalminutes.Value, 2));
            }

            return bm;
        }

        public BasicUserModel GetUsers(string idcompany, DateTime fromdate, DateTime todate)
        {
            BasicUserModel bm = new BasicUserModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany
                         && e.Date >= fromdate && e.Date <= todate
                         select new AutomaticTakeTimeModel
                         {
                             UserName = e.UserName,
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date,
                         }).Distinct().ToList();

            for (var i = 0; i < query.Count; i++)
            {
                bm.User.Add(query[i].UserName);
                bm.Application.Add(query[i].Application);
                bm.Time.Add((double)query[i].Time);
                bm.DateTime.Add((DateTime)query[i].Date);

            }
            return bm;
        }
        public BasicUserModel GetUserByName(string idcompany, DateTime fromdate, DateTime todate, string Name)
        {
            //string fromDate = fromdate.ToString("yyyy-MM-dd");
            //string toDate = todate.ToString("yyyy-MM-dd");
            BasicUserModel bm = new BasicUserModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany
                         && e.Date >= fromdate && e.Date <= todate
                         && e.UserName == Name
                         select new AutomaticTakeTimeModel
                         {
                             UserName = e.UserName,
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date
                         }).Distinct().ToList();

            for (var i = 0; i < query.Count; i++)
            {
                bm.User.Add(query[i].UserName);
                bm.Application.Add(query[i].Application);
                bm.Time.Add((double)query[i].Time);
            }
            return bm;
        }
        public BasicStatsModel TypeApp(string idcompany)
        {
            DateTime dateFrom = DateTime.Today.AddDays(-50);
            DateTime dateTo = dateFrom.AddHours(23);
            BasicStatsModel bm = new BasicStatsModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany && e.Date >= dateFrom && e.Date <= dateTo
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date
                         }).Distinct().ToList();

            foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Application).ToList())
            {
                var item = grouping;

                double? time = query.Where(t => t.Application == item.Key).Select(f => f.Time).Sum();
                bm.labels.Add(item.Key);
                double? totalminutes = 0;

                if (time != null && time > 0)
                    totalminutes = (time / 60);

                bm.data.Add(Math.Round(totalminutes.Value, 2));
            }

            return bm;
        }

        public BasicStatsModel WebUsedApp(string idcompany, DateTime fromdate, DateTime todate)
        {
            BasicStatsModel bm = new BasicStatsModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany && e.Application == "chrome"
                         && e.Date >= fromdate && e.Date <= todate
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date,
                             Title = e.Title
                         }).Distinct().ToList();

            foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Title).ToList())
            {
                var item = grouping;

                double? time = query.Where(t => t.Title == item.Key).Select(f => f.Time).Sum();
                bm.labels.Add(item.Key);
                double? totalminutes = 0;

                if (time != null && time > 0)
                    totalminutes = (time / 60);

                bm.data.Add(Math.Round(totalminutes.Value, 2));
            }

            return bm;
        }


        public List<SelectListItem> GetActivityUser(string idcompany)
        {
            try
            {
                var _queryFiltre = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                    Where(e => e.IdEmpresa == idcompany);

                return _queryFiltre.Select(x => x.UserName).Distinct().Select(x => new SelectListItem() { Text = x, Value = x }).ToList();

            }
            catch (Exception ex)
            {
                return new List<SelectListItem>();
            }
        }

        public List<BasicStatsDashboard> GetDataForDashBoard(string idcompany, DateTime from, DateTime to, string user)
        {
            try
            {
                Guid idempresa = Guid.Parse(idcompany);
                List<Agent_ProgramClasification> clasifications = db.Agent_ProgramClasification.Where(t => t.Agent_Empresa.IdCompany == idempresa).ToList();

                var _startTest = new DateTime(from.Year, from.Month, from.Day);
                var _endTest = new DateTime(to.Year, to.Month, to.Day);
                _endTest = _endTest.Add(new TimeSpan(23, 59, 59));

                //var _startTest = new DateTime(2022, 02, 25);
                //var _endTest = new DateTime(2022, 02, 25);
                //_endTest = _endTest.Add(new TimeSpan(23, 59, 59));
                List<BasicStatsDashboard> queryPrincipal = new List<BasicStatsDashboard>();

                if (user == "Todos")
                {
                    queryPrincipal = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                    Where(e => e.IdEmpresa == idcompany
                    && (e.FocusTime >= _startTest
                    && e.FocusTime <= _endTest)
                    )
                    .Select(e =>
                               new BasicStatsDashboard
                               {
                                   User = e.UserName,
                                   Application = e.Application,
                                   Time = e.Activity,
                                   Date_ = e.Date
                               }).ToList();
                }
                else
                {
                    queryPrincipal = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                  Where(e => e.IdEmpresa == idcompany && (e.FocusTime >= _startTest && e.FocusTime <= _endTest) && e.UserName == user)
                  .Select(e =>
                             new BasicStatsDashboard
                             {
                                 User = e.UserName,
                                 Application = e.Application,
                                 Time = e.Activity,
                                 Date_ = e.Date
                             }).ToList();
                }

                foreach (var j in queryPrincipal)
                {
                    j.Clasification = clasifications.Where(t => t.name == j.Application).Select(s => s.clasification).SingleOrDefault();
                }

                return queryPrincipal;

            }
            catch (Exception ex)
            {
                return new List<BasicStatsDashboard>();
            }
        }

        private int GetClasification(List<Agent_ProgramClasification> lc, string applicationname)
        {
            int clasification = 0;
            clasification = lc.Where(t => t.name.ToLower() == applicationname).Select(d => d.clasification).SingleOrDefault();
            return clasification;
        }

        public async Task<List<UsersReportGanttModel>> GetactivityData(string idcompany, DateTime fromdate, DateTime todate, int periods, string[] user)
        {
            string message = "";
            if (periods == 0) { periods = 5 * 60; }

            var _queryFiltre = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                Where(e => e.IdEmpresa == idcompany);

            List<string> filterusers = new List<string>();
            filterusers = user.ToList();

            //con esto filtramos por multiusuario, excepto si viene la palabra, todos
            if (!filterusers.Contains("Todos"))
                _queryFiltre = _queryFiltre.Where(x => filterusers.Contains(x.UserName));

            //if (!string.IsNullOrEmpty(user))
            //{
            //    if (user.ToLower() != "todos")
            //    {
            //        _queryFiltre = _queryFiltre.Where(x => x.UserName == user);
            //    }
            //}

            var _startTest = new DateTime(fromdate.Year, fromdate.Month, fromdate.Day);
            var _endTest = new DateTime(todate.Year, todate.Month, todate.Day);
            _endTest = _endTest.Add(new TimeSpan(23, 59, 59));

            _queryFiltre = _queryFiltre.Where(s => s.FocusTime >= _startTest && s.FocusTime <= _endTest);

            var queryPrincipal = _queryFiltre.
                GroupBy(e => e.UserName)
                .Select(e =>
                           new UsersReportGanttModel
                           {
                               UserName = e.Key,
                               ReportSytems = e.Select(x => new UsersReportGanttModel.ReportTime()
                               {
                                   Application = x.Application,
                                   StringFocusTime = x.StringFocusTime,
                                   StringDate = x.StringDate,
                                   //FocusTime = x.FocusTime,
                                   Activity = x.Activity,
                                   InActivity = x.Inactivity ?? 0 //Se agrega acá el parametro adicional
                               }).ToList()
                           }).ToList();

            List<Agent_ProgramClasification> agentclasification = db.Agent_ProgramClasification.Where(t => t.Agent_Empresa.IdCompany.ToString() == idcompany).ToList();

            ConcurrentQueue<UsersReportGanttModel> queryAuxParallel = new ConcurrentQueue<UsersReportGanttModel>();

            await Task.FromResult(Parallel.ForEach(queryPrincipal, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (Item) =>
            {
                UsersReportGanttModel InsertArray = new UsersReportGanttModel() { UserName = Item.UserName };

                ConcurrentQueue<UsersReportGanttModel.ReportTime> _ReportSytemsParallel = new ConcurrentQueue<UsersReportGanttModel.ReportTime>();

                await Task.FromResult(Parallel.ForEach(Item.ReportSytems, new ParallelOptions { MaxDegreeOfParallelism = 6 }, (Item2) =>
                {
                    string _NameApps = Item2.Application.Trim().ToUpper();
                    int IdClassication = 0;
                    string NameClassication = "Sin clasificar";

                    using (DAL.QueueContext db = new DAL.QueueContext())
                    {

                        var programsFound = agentclasification.FirstOrDefault(t => t.name.Trim().ToUpper() == _NameApps);

                        if (programsFound != null)
                        {
                            switch (programsFound.clasification)
                            {
                                case 1:
                                    IdClassication = 1;
                                    NameClassication = "Productivas";
                                    break;
                                case 2:
                                    IdClassication = 2;
                                    NameClassication = "Improductiva";
                                    break;
                                case 3:
                                    IdClassication = 3;
                                    NameClassication = "Neutrales";
                                    break;
                            }
                        }
                    }

                    var NewReportTimes = new UsersReportGanttModel.ReportTime
                    {
                        Application = _NameApps,
                        //FocusTime = Item2.FocusTime,
                        Activity = Item2.Activity,
                        InActivity = Item2.InActivity,
                        AppsImproClassify = IdClassication,
                        AppImproName = NameClassication,
                        StringFocusTime = Item2.StringFocusTime,
                        StringDate = Item2.StringDate
                    };

                    _ReportSytemsParallel.Enqueue(NewReportTimes);


                }));

                InsertArray.ReportSytems = _ReportSytemsParallel.OrderBy(x => x.FocusTime).ToList();

                queryAuxParallel.Enqueue(InsertArray);

            }));


            List<UsersReportGanttModel> queryAux = new List<UsersReportGanttModel>();

            try
            {
                foreach (var item in queryAuxParallel)
                {
                    var NewRecord = new UsersReportGanttModel() { UserName = item.UserName };
                    var listAppsGroupClasification = item.ReportSytems.Select(x => x.AppsImproClassify).Distinct().ToList();


                    List<UsersReportGanttModel.ReportTime> _reportTimestmp = new List<UsersReportGanttModel.ReportTime>();

                    var RecordTimes = item.ReportSytems.OrderBy(c => c.FocusTime);


                    string _IdAppsPrevious = string.Empty;

                    foreach (var itemReportTimes in RecordTimes)
                    {
                        var insertData = true;
                        if (_IdAppsPrevious == itemReportTimes.Application)
                        {
                            var infoTimesApps = _reportTimestmp.Where(x => x.Application == itemReportTimes.Application).OrderBy(x => x.FocusTimeEnd);
                            if (infoTimesApps.Any())
                            {
                                var timesApps = infoTimesApps.Last();

                                TimeSpan diff = timesApps.FocusTimeEnd - itemReportTimes.FocusTime;
                                double _Seconds = Math.Abs(Math.Truncate(diff.TotalSeconds));
                                if (!(_Seconds > 0))
                                {
                                    timesApps.Activity += itemReportTimes.Activity;
                                    timesApps.InActivity += itemReportTimes.InActivity;
                                    insertData = false;
                                }
                            }
                        }

                        if (insertData)
                        {
                            var NewReportTimes = new UsersReportGanttModel.ReportTime
                            {
                                Application = itemReportTimes.Application,
                                //FocusTime = itemReportTimes.FocusTime,
                                Activity = itemReportTimes.Activity,
                                InActivity = itemReportTimes.InActivity,
                                AppsImproClassify = itemReportTimes.AppsImproClassify,
                                AppImproName = itemReportTimes.AppImproName,
                                StringFocusTime = itemReportTimes.StringFocusTime,
                                StringDate = itemReportTimes.StringDate

                            };

                            _reportTimestmp.Add(NewReportTimes);
                        }

                        _IdAppsPrevious = itemReportTimes.Application;
                    }

                    double _periods = periods * 60;
                    _reportTimestmp = _reportTimestmp.OrderBy(x => x.FocusTime).ToList();
                    var firtTime = true;
                    var insertTimesfirst = false;

                    foreach (var itemReportTimes in _reportTimestmp)
                    {

                        bool newGroup = false;

                        if (firtTime)
                        {
                            firtTime = false;
                            var NewReportTimes = new UsersReportGanttModel.ReportTime
                            {
                                Application = itemReportTimes.Application,
                                //GroupApplication = new List<string>() { ShowInfo },
                                //FocusTime = itemReportTimes.FocusTime,
                                Activity = 0, //se sumará mas adelante
                                InActivity = 0, //se sumará mas adelante
                                AppsImproClassify = itemReportTimes.AppsImproClassify,
                                AppImproName = itemReportTimes.AppImproName,
                                StringFocusTime = itemReportTimes.StringFocusTime,
                                StringDate = itemReportTimes.StringDate
                            };

                            insertTimesfirst = true;

                            if (NewReportTimes.FocusTimeEnd == null)
                                NewReportTimes.StringFocusTime = string.Empty;

                            NewRecord.ReportSytems.Add(NewReportTimes);
                        }
                        else
                        {
                            insertTimesfirst = false;
                        }


                        var foundRecordTimes = NewRecord.ReportSytems.Last();

                        if (_periods > 0)
                        {
                            //if ((foundRecordTimes.Activity + itemReportTimes.Activity) > _periods)
                            //{
                            //    newGroup = true;
                            //}

                            if ((foundRecordTimes.TotalActivity + itemReportTimes.TotalActivity) > _periods)
                            {
                                newGroup = true;
                            }
                        }

                        if (newGroup)
                        {
                            double _activitys = itemReportTimes.Activity;
                            double _inactivitys = itemReportTimes.InActivity;
                            DateTime _FocusTime = itemReportTimes.FocusTime;

                            if (insertTimesfirst)
                            {
                                insertTimesfirst = false;
                                foundRecordTimes.Activity = _activitys;
                                foundRecordTimes.InActivity = _inactivitys;
                            }

                            if (foundRecordTimes.TotalActivity > _periods)
                            {
                                if (foundRecordTimes.Activity < _periods)
                                {
                                    //sacamos la diferencia con el perior
                                    double _dife = _periods - foundRecordTimes.Activity;
                                    double _restante = (foundRecordTimes.InActivity > 0 ? foundRecordTimes.InActivity - _dife : 0);

                                    //--------------------------------------------
                                    _activitys = 0;
                                    _inactivitys = _restante;
                                    //--------------------------------------------
                                    //foundRecordTimes.Activity = _periods;
                                    foundRecordTimes.InActivity = (foundRecordTimes.InActivity > 0 ? _dife : 0);
                                }
                                else
                                {
                                    double _dif = foundRecordTimes.Activity - _periods;
                                    //--------------------------------------------
                                    _activitys = _dif;
                                    _inactivitys = foundRecordTimes.InActivity;
                                    //--------------------------------------------
                                    foundRecordTimes.Activity = _periods;
                                    foundRecordTimes.InActivity = 0;
                                }

                                foundRecordTimes.GroupApplication.Add(new UsersReportGanttModel.ReportTimeGroupApps()
                                {
                                    Application = foundRecordTimes.Application,
                                    AppImproName = foundRecordTimes.AppImproName,
                                    AppsImproClassify = foundRecordTimes.AppsImproClassify,
                                    Activity = foundRecordTimes.Activity,
                                    InActivity = foundRecordTimes.InActivity,

                                });


                            }

                            _FocusTime = foundRecordTimes.FocusTimeEnd;

                            double _aux = 0;
                            bool contineCorrection = true;
                            while (contineCorrection)
                            {
                                var _sumActivitys = _aux + (_activitys + _inactivitys);
                                if (_sumActivitys > _periods)
                                {
                                    double auxActivity = 0;
                                    double auxInActivity = 0;

                                    if (_activitys < _periods)
                                    {
                                        //sacamos la diferencia con el perior
                                        double _dife = _periods - _activitys;
                                        double _restante = (_inactivitys > 0 ? _inactivitys - _dife : 0);
                                        //--------------------------------------------
                                        auxActivity = _activitys;
                                        auxInActivity = (_inactivitys > 0 ? _dife : 0);
                                        //--------------------------------------------
                                        _activitys = 0;
                                        _inactivitys = _restante;

                                    }
                                    else
                                    {
                                        double _dif = _activitys - _periods;
                                        //--------------------------------------------
                                        _activitys = _dif;
                                        //--------------------------------------------
                                        auxActivity = _periods;
                                        auxInActivity = 0;
                                    }

                                    var timeSpan = new TimeSpan(_FocusTime.TimeOfDay.Hours, _FocusTime.TimeOfDay.Minutes, _FocusTime.TimeOfDay.Seconds);
                                    var infoFound = NewRecord.ReportSytems.Where(x => (new TimeSpan(x.FocusTime.TimeOfDay.Hours, x.FocusTime.TimeOfDay.Minutes, x.FocusTime.TimeOfDay.Seconds)).CompareTo(timeSpan) == 0).ToList();
                                    if (infoFound.Any())
                                    {
                                        //_FocusTime = infoFound.First().FocusTimeEnd;
                                    }

                                    //_aux = +_periods;
                                    //crear grupos
                                    var _NewReportTimes = new UsersReportGanttModel.ReportTime
                                    {
                                        Application = foundRecordTimes.Application,
                                        GroupApplication = new List<UsersReportGanttModel.ReportTimeGroupApps>() {
                                        new UsersReportGanttModel.ReportTimeGroupApps()
                                        {
                                            Application = itemReportTimes.Application,
                                            AppImproName = itemReportTimes.AppImproName,
                                            AppsImproClassify = itemReportTimes.AppsImproClassify,
                                            Activity = auxActivity,
                                            InActivity = auxInActivity
                                        }
                                    },
                                        //FocusTime = _FocusTime,
                                        Activity = auxActivity,
                                        InActivity = auxInActivity,
                                        AppsImproClassify = itemReportTimes.AppsImproClassify,
                                        AppImproName = itemReportTimes.AppImproName,
                                        StringFocusTime = itemReportTimes.StringFocusTime,
                                        StringDate = itemReportTimes.StringDate
                                    };

                                    NewRecord.ReportSytems.Add(_NewReportTimes);
                                    //_activitys -= _periods;
                                    _FocusTime = _NewReportTimes.FocusTimeEnd;
                                }
                                else
                                {
                                    contineCorrection = false;
                                    _activitys = Math.Abs(_activitys);
                                    _inactivitys = Math.Abs(_inactivitys);
                                    //_FocusTime = _FocusTime.AddSeconds(_activitys+ _inactivitys);
                                }


                            }

                            var timeSpan2 = new TimeSpan(_FocusTime.TimeOfDay.Hours, _FocusTime.TimeOfDay.Minutes, _FocusTime.TimeOfDay.Seconds);
                            var infoFounds = NewRecord.ReportSytems.Where(x => (new TimeSpan(x.FocusTime.TimeOfDay.Hours, x.FocusTime.TimeOfDay.Minutes, x.FocusTime.TimeOfDay.Seconds)).CompareTo(timeSpan2) == 0).ToList();
                            if (infoFounds.Any())
                            {
                                //_FocusTime = infoFounds.First().FocusTimeEnd;
                            }

                            //Cierre grupo
                            var NewReportTimes = new UsersReportGanttModel.ReportTime
                            {
                                Application = itemReportTimes.Application,
                                GroupApplication = new List<UsersReportGanttModel.ReportTimeGroupApps>() {
                                new UsersReportGanttModel.ReportTimeGroupApps()
                                {
                                    Application = itemReportTimes.Application,
                                    AppImproName = itemReportTimes.AppImproName,
                                    AppsImproClassify = itemReportTimes.AppsImproClassify,
                                    Activity = _activitys,
                                    InActivity = _inactivitys
                                }
                            },
                                FocusTime = _FocusTime,
                                StringFocusTime = _FocusTime.ToString(),
                                StringDate = itemReportTimes.StringDate,
                                Activity = _activitys,
                                InActivity = _inactivitys,
                                AppsImproClassify = itemReportTimes.AppsImproClassify,
                                AppImproName = itemReportTimes.AppImproName
                            };

                            NewRecord.ReportSytems.Add(NewReportTimes);
                        }
                        else
                        {
                            foundRecordTimes.GroupApplication.Add(new UsersReportGanttModel.ReportTimeGroupApps() { Application = itemReportTimes.Application, AppImproName = itemReportTimes.AppImproName, AppsImproClassify = itemReportTimes.AppsImproClassify, Activity = itemReportTimes.Activity, InActivity = itemReportTimes.InActivity });
                            foundRecordTimes.Activity += itemReportTimes.Activity;
                            foundRecordTimes.InActivity += itemReportTimes.InActivity;
                        }
                    }

                    if (NewRecord.ReportSytems.Any())
                    {
                        queryAux.Add(NewRecord);
                    }

                }
            }
            catch (Exception ex)
            {
                if (ex.StackTrace.Substring(ex.StackTrace.LastIndexOf(' ')).Length > 0)
                {
                    var linenum = Convert.ToInt32(ex.StackTrace.Substring(ex.StackTrace.LastIndexOf(' ')));
                    message = String.Join(" ", new String[] { ex.Message.ToString(), " - Line:", linenum.ToString() });
                }
                else
                {
                    message = String.Join(" ", new String[] { ex.Message.ToString(), " - Line:", string.Empty });
                }
                var d = message;
            }
            return queryAux;
        }


        public async Task<List<UsersReportGanttModel>> GetactivityDataUserSelected(string idcompany, DateTime fromdate, DateTime todate, string user)
        {

            var _queryFiltre = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                Where(e => e.IdEmpresa == idcompany);

            _queryFiltre = _queryFiltre.Where(x => x.UserName == user);

            var _startTest = new DateTime(fromdate.Year, fromdate.Month, fromdate.Day);
            var _endTest = new DateTime(todate.Year, todate.Month, todate.Day);
            _endTest = _endTest.Add(new TimeSpan(23, 59, 59));

            _queryFiltre = _queryFiltre.Where(s => s.FocusTime >= _startTest && s.FocusTime <= _endTest);

            var queryPrincipal = _queryFiltre.
                GroupBy(e => e.UserName)
                .Select(e =>
                           new UsersReportGanttModel
                           {
                               UserName = e.Key,
                               ReportSytems = e.Select(x => new UsersReportGanttModel.ReportTime()
                               {
                                   Application = x.Application,
                                   FocusTime = x.FocusTime,
                                   Activity = x.Activity,
                                   InActivity = x.Inactivity ?? 0,
                                   StringFocusTime = x.StringFocusTime,
                                   StringDate = x.StringDate
                               }).ToList()
                           }).ToList();



            ConcurrentQueue<UsersReportGanttModel> queryAuxParallel = new ConcurrentQueue<UsersReportGanttModel>();

            var listProgramClasification = db.Agent_ProgramClasification.Where(t => t.Agent_Empresa.IdCompany.ToString() == idcompany).ToList();

            await Task.FromResult(Parallel.ForEach(queryPrincipal, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (Item) =>
            {
                UsersReportGanttModel InsertArray = new UsersReportGanttModel() { UserName = Item.UserName };

                ConcurrentQueue<UsersReportGanttModel.ReportTime> _ReportSytemsParallel = new ConcurrentQueue<UsersReportGanttModel.ReportTime>();

                await Task.FromResult(Parallel.ForEach(Item.ReportSytems, new ParallelOptions { MaxDegreeOfParallelism = 6 }, (Item2) =>
                {
                    string _NameApps = Item2.Application.Trim().ToUpper();
                    int IdClassication = 0;
                    string NameClassication = "Sin clasificar";

                    using (DAL.QueueContext db = new DAL.QueueContext())
                    {

                        var programsFound = listProgramClasification.FirstOrDefault(t => t.name.Trim().ToUpper() == _NameApps);

                        if (programsFound != null)
                        {
                            switch (programsFound.clasification)
                            {
                                case 1:
                                    IdClassication = 1;
                                    NameClassication = "Productivas";
                                    break;
                                case 2:
                                    IdClassication = 2;
                                    NameClassication = "Improductiva";
                                    break;
                                case 3:
                                    IdClassication = 3;
                                    NameClassication = "Neutrales";
                                    break;
                            }
                        }
                    }

                    var NewReportTimes = new UsersReportGanttModel.ReportTime
                    {
                        Application = _NameApps,
                        FocusTime = Item2.FocusTime,
                        Activity = Item2.Activity,
                        InActivity = Item2.InActivity,
                        AppsImproClassify = IdClassication,
                        AppImproName = NameClassication,
                        StringFocusTime = Item2.StringFocusTime,
                        StringDate = Item2.StringDate
                    };

                    _ReportSytemsParallel.Enqueue(NewReportTimes);


                }));

                InsertArray.ReportSytems = _ReportSytemsParallel.OrderBy(x => x.FocusTime).ToList();

                queryAuxParallel.Enqueue(InsertArray);

            }));


            List<UsersReportGanttModel> queryAux = new List<UsersReportGanttModel>();

            foreach (var item in queryAuxParallel)
            {


                List<UsersReportGanttModel.ReportTime> _reportTimestmp = new List<UsersReportGanttModel.ReportTime>();

                var RecordTimes = item.ReportSytems.OrderBy(c => c.FocusTime);
                string _IdAppsPrevious = string.Empty;

                foreach (var itemReportTimes in RecordTimes)
                {
                    var insertData = true;
                    if (_IdAppsPrevious == itemReportTimes.Application)
                    {
                        var sumaActivityRecordAppsLocal = itemReportTimes.Activity;
                        var infoTimesApps = _reportTimestmp.Where(x => x.Application == itemReportTimes.Application).OrderBy(x => x.FocusTimeEnd);
                        if (infoTimesApps.Any())
                        {
                            var timesApps = infoTimesApps.Last();

                            TimeSpan diff = timesApps.FocusTimeEnd - itemReportTimes.FocusTime;
                            double _Seconds = Math.Abs(Math.Truncate(diff.TotalSeconds));
                            if (!(_Seconds > 0))
                            {
                                timesApps.Activity += itemReportTimes.Activity;
                                timesApps.InActivity += itemReportTimes.InActivity;
                                insertData = false;
                            }
                        }
                    }

                    if (insertData)
                    {
                        var NewReportTimes = new UsersReportGanttModel.ReportTime
                        {
                            Application = itemReportTimes.Application,
                            FocusTime = itemReportTimes.FocusTime,
                            Activity = itemReportTimes.Activity,
                            InActivity = itemReportTimes.InActivity,
                            AppsImproClassify = itemReportTimes.AppsImproClassify,
                            AppImproName = itemReportTimes.AppImproName,
                            StringFocusTime = itemReportTimes.StringFocusTime,
                            StringDate = itemReportTimes.StringDate
                        };

                        _reportTimestmp.Add(NewReportTimes);
                    }

                    _IdAppsPrevious = itemReportTimes.Application;
                }

                var NewRecord = new UsersReportGanttModel() { UserName = item.UserName };
                _reportTimestmp = _reportTimestmp.OrderBy(x => x.FocusTime).ToList();
                var firtTime = true;


                var _FocusTime = _reportTimestmp.FirstOrDefault().FocusTime;

                foreach (var itemReportTimes in _reportTimestmp)
                {

                    var NewReportTimes = new UsersReportGanttModel.ReportTime
                    {
                        Application = itemReportTimes.Application,
                        GroupApplication = new List<UsersReportGanttModel.ReportTimeGroupApps>() { new UsersReportGanttModel.ReportTimeGroupApps() { Application = itemReportTimes.Application, AppImproName = itemReportTimes.AppImproName, AppsImproClassify = itemReportTimes.AppsImproClassify, Activity = itemReportTimes.Activity, InActivity = itemReportTimes.InActivity } },
                        FocusTime = _FocusTime,
                        Activity = itemReportTimes.Activity,
                        InActivity = itemReportTimes.InActivity,
                        AppsImproClassify = itemReportTimes.AppsImproClassify,
                        AppImproName = itemReportTimes.AppImproName,
                        StringFocusTime = itemReportTimes.StringFocusTime,
                        StringDate = itemReportTimes.StringDate
                    };

                    NewRecord.ReportSytems.Add(NewReportTimes);

                    _FocusTime = NewReportTimes.FocusTimeEnd;
                }

                var list_apps = NewRecord.ReportSytems.Select(x => x.Application).Distinct();
                foreach (var itemNameApps in list_apps)
                {
                    var _NewRecord = new UsersReportGanttModel() { UserName = itemNameApps };

                    foreach (var itemTimeTrack in NewRecord.ReportSytems.Where(x => x.Application == itemNameApps).OrderBy(x => x.FocusTime))
                    {
                        _NewRecord.ReportSytems.Add(itemTimeTrack);
                    }

                    queryAux.Add(_NewRecord);
                }
            }

            return queryAux;
        }

        public BasicStatsModel ImproductiveUsedApp(string idcompany, DateTime fromdate, DateTime todate)
        {
            BasicStatsModel bm = new BasicStatsModel();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.IdEmpresa == idcompany
                         && e.Date >= fromdate && e.Date <= todate
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date,
                         }).Distinct().ToList();

            foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Application).ToList())
            {
                var item = grouping;

                double? time = query.Where(t => t.Application == item.Key).Select(f => f.Time).Sum();
                bm.labels.Add(item.Key);
                var date = query.Where(t => t.Application == item.Key).Select(f => f.Date).ToList();

                double? totalminutes = 0;
                for (int i = 0; i < date.Count; i++)
                {
                    bm.DateTime.Add(date[i].ToString("H:mm:ss"));
                }
                if (time != null && time > 0)
                    totalminutes = (time / 60);

                bm.data.Add(Math.Round(totalminutes.Value, 2));
            }

            return bm;
        }


        public BasicStatsDate DateUsedApp(string app, DateTime fromdate, DateTime todate)
        {
            BasicStatsDate bm = new BasicStatsDate();
            var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
                         where e.Application == app
                         && e.Date >= fromdate && e.Date <= todate
                         select new AutomaticTakeTimeModel
                         {
                             Application = e.Application,
                             Time = e.Activity,
                             Date = e.Date
                         }).Distinct().ToList();

            foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Application).ToList())
            {
                var item = grouping;

                double? time = query.Where(t => t.Application == item.Key).Select(f => f.Time).Sum();

                var date = query.Where(t => t.Application == item.Key).Select(f => f.Date).ToList();


                for (int i = 0; i < date.Count; i++)
                {
                    bm.DateTime.Add(date[i].ToString("H:mm:ss"));
                }

            }

            return bm;
        }
        #endregion



        #region Dasboard Stats

        #endregion
    }
}