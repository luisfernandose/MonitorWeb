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
using System.Reflection;
using System.Collections;
using System.IO;
using System.Drawing;
using System.Data.Entity;
using System.Net;
using MongoDB.Bson;

namespace Queue.Controllers
{
    [Authorize]
    public class OperationController : BaseController
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
            catch (Exception ex)
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
                List<TrakerBase> ltbr = new List<TrakerBase>();
                MongoHelper.TrakerBase = MongoHelper.database.GetCollection<TrakerBase>("TrackerTime");

                //sele restan 5 horas porque no se que pasa con el servidor de mkongo que le suma 5 horas, no se como configurar una UTC acorde con mongo para que ponga
                //la hora que es.
                foreach (var j in tb)
                {
                    double upload = double.Parse(j.UploadFrecuency);

                    j.Date = new DateTime(j.Date.Year, j.Date.Month, j.Date.Day, j.Date.Hour, j.Date.Minute, j.Date.Second, j.Date.Kind).AddHours(-5);
                    j.FocusTime = new DateTime(j.FocusTime.Year, j.FocusTime.Month, j.FocusTime.Day, j.FocusTime.Hour, j.FocusTime.Minute, j.FocusTime.Second, j.FocusTime.Kind).AddHours(-5);

                    if (j.Inactivity > (upload + 5))
                        j.Inactivity = upload;

                    //aqui preguntamos si el tiempo de actividad es mayor al doble de la frecuencia de subida se elimina de la lista, 
                    //porque puede ser un bug
                    TrakerBase trb = new TrakerBase();
                    if (j.Activity >= (upload * 2))
                    {
                        ltbr.Add(trb);
                    }
                }
                //se eliminan los que son mayores al doble de actividad, por posible bug
                foreach (var l in ltbr)
                {
                    tb.Remove(l);
                }

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
                cb.Date = cb.Date.AddHours(-5);
                cb.Hour = cb.Date.Hour;
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
                //var update = Builders<InstalledProgramsViewModel>.Update.Set("Status", false).Set("uninstalldate", DateTime.Now);
                var result = collection.DeleteOne(filter);
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
            DateTime dateFrom = DateTime.Today;//.AddDays(-50);
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

        //public BasicStatsModel ImproductiveUsedApp(string idcompany, DateTime fromdate, DateTime todate)
        //{
        //    BasicStatsModel bm = new BasicStatsModel();
        //    var query = (from e in MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>()
        //                 where e.IdEmpresa == idcompany
        //                 && e.Date >= fromdate && e.Date <= todate
        //                 select new AutomaticTakeTimeModel
        //                 {
        //                     Application = e.Application,
        //                     Time = e.Activity,
        //                     Date = e.Date
        //                 }).Distinct().ToList();

        //    foreach (var grouping in query.OrderByDescending(x => x.Time).GroupBy(g => g.Application).ToList())
        //    {
        //        var item = grouping;

        //        double? time = query.Where(t => t.Application == item.Key).Select(f => f.Time).Sum();
        //        bm.labels.Add(item.Key);
        //        var date = query.Where(t => t.Application == item.Key).Select(f => f.Date).ToList();

        //        double? totalminutes = 0;
        //        for (int i = 0; i < date.Count; i++)
        //        {
        //            bm.DateTime.Add(date[i].ToString("H:mm:ss"));
        //        }
        //        if (time != null && time > 0)
        //            totalminutes = (time / 60);

        //        bm.data.Add(Math.Round(totalminutes.Value, 2));
        //    }

        //    return bm;
        //}



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

        public List<BasicStatsDashboard> GetDataForDashBoard(string idcompany, DateTime from, DateTime to, string user, Guid idgroup)
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

                List<string> users = new List<string>();

                if (idgroup != null && idgroup != Guid.Empty)
                {
                    users = new List<string>();
                    users = db.Agent_EmployeeGroupsEmployee.Where(f => f.Agent_EmployeesGroups.idemployeesGroup == idgroup).Select(g => g.Agent_Employee.Usuario.ToLower()).ToList();
                }
                if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString() && user != "Todos")
                    users.Add(user.ToLower());


                queryPrincipal = MongoHelper.database.GetCollection<AutomaticTakeTimeModel>("TrackerTime").AsQueryable<AutomaticTakeTimeModel>().
                    Where(e => e.IdEmpresa == idcompany && (e.FocusTime >= _startTest && e.FocusTime <= _endTest))
                    .Select(e =>
                               new BasicStatsDashboard
                               {
                                   User = e.UserName,
                                   Application = e.Application,
                                   Time = e.Activity,
                                   Date_ = e.Date
                               }).ToList();


                //foltra por usuario o usuarios
                if (users.Count() > 0)
                    queryPrincipal = queryPrincipal.Where(v => users.Contains(v.User.ToLower())).ToList();


                foreach (var j in queryPrincipal)
                {
                    j.Clasification = clasifications.Where(t => t.name == j.Application).Select(s => s.clasification).SingleOrDefault();
                }

                queryPrincipal = queryPrincipal.OrderBy(o => o.Date_).ToList();

                List<BasicStatsDashboard> queryPrincipaltest = new List<BasicStatsDashboard>();

                queryPrincipaltest = queryPrincipal.OrderByDescending(o => o.Time).ToList();

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
                                   FocusTime = x.FocusTime,
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
                        FocusTime = Item2.FocusTime,
                        Activity = Item2.Activity,
                        InActivity = Item2.InActivity,
                        AppsImproClassify = IdClassication,
                        AppImproName = NameClassication
                    };

                    _ReportSytemsParallel.Enqueue(NewReportTimes);


                }));

                InsertArray.ReportSytems = _ReportSytemsParallel.OrderBy(x => x.FocusTime).ToList();

                queryAuxParallel.Enqueue(InsertArray);

            }));


            List<UsersReportGanttModel> queryAux = new List<UsersReportGanttModel>();

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
                            FocusTime = itemReportTimes.FocusTime,
                            Activity = itemReportTimes.Activity,
                            InActivity = itemReportTimes.InActivity,
                            AppsImproClassify = itemReportTimes.AppsImproClassify,
                            AppImproName = itemReportTimes.AppImproName

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
                            FocusTime = itemReportTimes.FocusTime,
                            Activity = 0, //se sumará mas adelante
                            InActivity = 0, //se sumará mas adelante
                            AppsImproClassify = itemReportTimes.AppsImproClassify,
                            AppImproName = itemReportTimes.AppImproName
                        };

                        insertTimesfirst = true;

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
                                InActivity = foundRecordTimes.InActivity
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
                                    FocusTime = _FocusTime,
                                    Activity = auxActivity,
                                    InActivity = auxInActivity,
                                    AppsImproClassify = itemReportTimes.AppsImproClassify,
                                    AppImproName = itemReportTimes.AppImproName
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
                                   InActivity = x.Inactivity ?? 0
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
                        AppImproName = NameClassication
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
                            AppImproName = itemReportTimes.AppImproName
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
                        AppImproName = itemReportTimes.AppImproName
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



        #region Reports      
        public ActionResult SoftwareReport(string user, Guid? idgroup)
        {
            List<SoftwareReport> srlist = new List<SoftwareReport>();
            Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (IdCompany != Guid.Empty)
            {
                if (!string.IsNullOrEmpty(user) || idgroup != null)
                {
                    MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                    var builder = Builders<InstalledProgramsViewModel>.Filter;
                    var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("Status", true);

                    List<InstalledProgramsViewModel> results = MongoHelper.SoftWareList.Find(filter).ToList();

                    if (idgroup != null && idgroup != Guid.Empty)
                    {
                        List<Agent_Employee> users_ = db.Agent_EmployeeGroupsEmployee.Where(f => f.Agent_Employee.IdCompany == IdCompany).Select(t => t.Agent_Employee).ToList();
                        List<string> _users = users_.Select(s => s.Usuario).ToList();

                        results = results.Where(u => _users.Contains(u.User)).ToList();
                    }

                    if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString())
                    {
                        results = results.Where(u => u.User == user).ToList();
                    }

                    SoftwareReport sr;
                    foreach (var i in results.GroupBy(g => g.Name))
                    {
                        sr = new SoftwareReport();
                        sr.program = i.Key;
                        sr.quantity = i.Count();
                        srlist.Add(sr);
                    }
                }
            }

            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == IdCompany).ToList(), "idemployeesGroup", "Nombre", idgroup);
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgroup = sli;


            List<SelectListItem> sle = CreateList(db.Agent_Employee.Where(c => c.IdCompany == IdCompany).ToList(), "Usuario", "Usuario", user);
            sle.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.user = sle;


            return View(srlist);
        }

        public ActionResult SoftwareReportDetails(string name, Guid? idgroup, string user = "00000000-0000-0000-0000-000000000000")
        {
            List<SoftwareReport> srlist = new List<SoftwareReport>();
            Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (IdCompany != Guid.Empty)
            {
                if (!string.IsNullOrEmpty(user) || idgroup != null)
                {
                    MongoHelper.SoftWareList = MongoHelper.database.GetCollection<InstalledProgramsViewModel>("Software");
                    var builder = Builders<InstalledProgramsViewModel>.Filter;
                    var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("Status", true) & builder.Eq("Name", name);

                    List<InstalledProgramsViewModel> results = MongoHelper.SoftWareList.Find(filter).ToList();

                    if (idgroup != null && idgroup != Guid.Empty)
                    {
                        List<Agent_Employee> users_ = db.Agent_EmployeeGroupsEmployee.Where(f => f.Agent_Employee.IdCompany == IdCompany).Select(t => t.Agent_Employee).ToList();
                        List<string> _users = users_.Select(s => s.Usuario).ToList();

                        results = results.Where(u => _users.Contains(u.User)).ToList();
                    }

                    if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString())
                    {
                        results = results.Where(u => u.User == user).ToList();
                    }

                    SoftwareReport sr;
                    foreach (var i in results.Where(g => g.Name == name))
                    {
                        sr = new SoftwareReport();
                        sr.program = i.Name;
                        sr.agrupation = i.Pc;
                        srlist.Add(sr);
                    }
                }
            }

            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == IdCompany).ToList(), "idemployeesGroup", "Nombre", idgroup);
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgroup = sli;


            List<SelectListItem> sle = CreateList(db.Agent_Employee.Where(c => c.IdCompany == IdCompany).ToList(), "Usuario", "Usuario", user);
            sle.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.user = sle;


            return View(srlist.Distinct());
        }
        public ActionResult HardwareReport(string user, Guid? idgroup)
        {
            List<HardwareReport> srlist = new List<HardwareReport>();
            Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            if (IdCompany != Guid.Empty)
            {
                if (!string.IsNullOrEmpty(user) || idgroup != null)
                {
                    MongoHelper.HardWareList = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");
                    var builder = Builders<InstalledHardwareViewModel>.Filter;
                    var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("status", true);

                    List<InstalledHardwareViewModel> results = MongoHelper.HardWareList.Find(filter).ToList();

                    if (idgroup != null && idgroup != Guid.Empty)
                    {
                        List<Agent_Employee> users_ = db.Agent_EmployeeGroupsEmployee.Where(f => f.Agent_Employee.IdCompany == IdCompany).Select(t => t.Agent_Employee).ToList();
                        List<string> _users = users_.Select(s => s.Usuario).ToList();

                        results = results.Where(u => _users.Contains(u.User)).ToList();
                    }

                    if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString())
                    {
                        results = results.Where(u => u.User == user).ToList();
                    }

                    HardwareReport sr;
                    foreach (var i in results.GroupBy(g => new { g.Type, g.Hardware }))
                    {
                        sr = new HardwareReport();
                        sr.type = i.Key.Type;
                        sr.hardware = i.Key.Hardware;
                        sr.quantity = i.Count();
                        srlist.Add(sr);
                    }
                }
            }

            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == IdCompany).ToList(), "idemployeesGroup", "Nombre", idgroup);
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgroup = sli;


            List<SelectListItem> sle = CreateList(db.Agent_Employee.Where(c => c.IdCompany == IdCompany).ToList(), "Usuario", "Usuario", user);
            sle.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.user = sle;


            return View(srlist);
        }

        public ActionResult HardwareReportDetails(string hardware, Guid? idgroup, string user = "00000000-0000-0000-0000-000000000000")
        {
            List<HardwareReport> srlist = new List<HardwareReport>();
            Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            if (IdCompany != Guid.Empty)
            {
                if (!string.IsNullOrEmpty(user) || idgroup != null)
                {
                    MongoHelper.HardWareList = MongoHelper.database.GetCollection<InstalledHardwareViewModel>("Hardware");
                    var builder = Builders<InstalledHardwareViewModel>.Filter;
                    var filter = builder.Eq("IdCompany", IdCompany) & builder.Eq("status", true) & builder.Eq("Hardware", hardware);

                    List<InstalledHardwareViewModel> results = MongoHelper.HardWareList.Find(filter).ToList();

                    if (idgroup != null && idgroup != Guid.Empty)
                    {
                        List<Agent_Employee> users_ = db.Agent_EmployeeGroupsEmployee.Where(f => f.Agent_Employee.IdCompany == IdCompany).Select(t => t.Agent_Employee).ToList();
                        List<string> _users = users_.Select(s => s.Usuario).ToList();

                        results = results.Where(u => _users.Contains(u.User)).ToList();
                    }

                    if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString())
                    {
                        results = results.Where(u => u.User == user).ToList();
                    }

                    HardwareReport sr;
                    foreach (var i in results.Where(g => g.Hardware == hardware))
                    {
                        sr = new HardwareReport();
                        sr.agrupation = i.Pc;
                        sr.type = i.Type;
                        sr.hardware = i.Hardware;
                        srlist.Add(sr);
                    }
                }
            }

            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == IdCompany).ToList(), "idemployeesGroup", "Nombre", idgroup);
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgroup = sli;


            List<SelectListItem> sle = CreateList(db.Agent_Employee.Where(c => c.IdCompany == IdCompany).ToList(), "Usuario", "Usuario", user);
            sle.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.user = sle;


            return View(srlist);
        }

        public ActionResult CapturesReport(string user, Guid? idgroup, DateTime? Datefrom, DateTime? Dateto, int? hour)
        {
            List<CapturesViewModel> result = new List<CapturesViewModel>();
            Guid IdCompany = Guid.Empty;
            string IdCompany_ = IdCompany.ToString();
            try
            {
                IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
                IdCompany_ = Request.RequestContext.HttpContext.Session["Company"].ToString();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }

            if (Datefrom == null)
            {
                Datefrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 01, 01, 00);
            }

            if (Dateto == null)
            {
                Dateto = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 00);
            }
            else
                Dateto = Dateto.Value.AddHours(23).AddMinutes(59);


            if (Datefrom != null && Dateto != null)
            {
                List<string> users = new List<string>();

                if (idgroup != Guid.Empty && idgroup != null)
                {
                    users = new List<string>();
                    users = db.Agent_EmployeeGroupsEmployee.Where(f => f.Agent_EmployeesGroups.idemployeesGroup == idgroup).Select(g => g.Agent_Employee.Usuario).ToList();
                }
                else
                {
                    if (!string.IsNullOrEmpty(user) && user != Guid.Empty.ToString())
                    {
                        users.Add(user);
                    }
                }


                IQueryable<CaptureBase> ListCapturesViewModel = (from e in MongoHelper.database.GetCollection<CaptureBase>("WindowsCapture").AsQueryable<CaptureBase>()
                                                                 where e.IdCompany == IdCompany_
                                                                 && (e.Date >= Datefrom.Value && e.Date <= Dateto.Value)
                                                                 select new CaptureBase
                                                                 {
                                                                     idrecord = e.idrecord,
                                                                     UserName = e.UserName,
                                                                     Image = e.Image,
                                                                     Date = e.Date,
                                                                     Hour = e.Hour
                                                                 });

                if (hour != null && hour != 0)
                    ListCapturesViewModel = ListCapturesViewModel.Where(t => t.Hour == hour);

                if (users.Count() > 0)
                    ListCapturesViewModel = ListCapturesViewModel.Where(p => users.Contains(p.UserName));

                foreach (var i in ListCapturesViewModel.OrderByDescending(o => o.Date).Take(100).ToList())
                {
                    CapturesViewModel cvm = new CapturesViewModel();
                    cvm.idrecord = i.idrecord;
                    cvm.UserName = i.UserName;
                    cvm.image = Convert.ToBase64String(i.Image);
                    cvm.Date = i.Date;
                    result.Add(cvm);
                }

                ViewBag.Datefrom = Datefrom.Value.ToString("yyyy-MM-dd");
                ViewBag.Dateto = Dateto.Value.ToString("yyyy-MM-dd");
            }
            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == IdCompany).ToList(), "idemployeesGroup", "Nombre", idgroup);
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgroup = sli;

            List<SelectListItem> sle = CreateList(db.Agent_Employee.Where(c => c.IdCompany == IdCompany).ToList(), "Usuario", "Usuario", user);
            sle.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.user = sle;


            return View(result.OrderByDescending(o => o.Date));
        }
        [HttpGet]
        public JsonResult GetCapturesReport(string id_)
        {
            try
            {
                Guid IdCompany = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
                MongoHelper.UserCapture = MongoHelper.database.GetCollection<CaptureBase>("WindowsCapture");
                var builder = Builders<CaptureBase>.Filter;
                var filter = builder.Eq("idrecord", id_);

                CaptureBase Capturelist = new CaptureBase();
                Capturelist = MongoHelper.UserCapture.Find(filter).SingleOrDefault();

                CapturesViewModel cvm = new CapturesViewModel();
                if (Capturelist != null)
                {
                    cvm._id = Capturelist._id.ToString();
                    cvm.UserName = Capturelist.UserName;
                    cvm.idrecord = Capturelist.idrecord;
                    cvm.image = Convert.ToBase64String(Capturelist.Image);
                    cvm.Date = Capturelist.Date;
                }
                return Json(cvm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<SelectListItem> CreateList(IEnumerable list, string dataValueField, string dataTextField, object selectedValue = null)
        {
            List<SelectListItem> sli = new List<SelectListItem>();
            SelectListItem sl;
            foreach (var i in list)
            {
                sl = new SelectListItem();
                foreach (PropertyInfo p in i.GetType().GetProperties())
                {
                    if (p.Name == dataTextField)
                        sl.Text = p.GetValue(i).ToString();

                    if (p.Name == dataValueField)
                    {
                        sl.Value = p.GetValue(i).ToString();
                        if (selectedValue != null)
                            if (sl.Value == selectedValue.ToString())
                                sl.Selected = true;
                    }
                }
                sli.Add(sl);
            }
            return sli;
        }


        [HttpGet]
        public ActionResult TimePerActivity()
        {
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            List<SelectListItem> sliu = CreateList(db.Agent_Employee.Where(u => u.IdCompany == company), "Usuario", "Usuario").ToList();
            sliu.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.user = sliu;

            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == company).ToList(), "idemployeesGroup", "Nombre");
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgruoup = sli;

            TimePerActivityViewModel datos = new TimePerActivityViewModel();

            return View(datos);
        }

        public ActionResult TimePerActivity(TimePerActivityViewModel activity)
        {
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            if (activity.from.Year <= 1900)
                activity.from = DateTime.Today;

            if (activity.to.Year <= 1900)
                activity.to = DateTime.Today;


            List<BasicStatsDashboard> data = GetDataForDashBoard(company.ToString(), activity.from, activity.to, activity.user, activity.idgruoup);
            TimePerActivityViewModel datos = new TimePerActivityViewModel();
            if (data.Count() > 0)
            {
                foreach (var i in data.GroupBy(g => g.Application))
                {
                    ActivitySumViewModel activities = new ActivitySumViewModel();

                    activities.program = i.Key;
                    double times = data.Where(b => b.Application == i.Key).Sum(k => k.Time).Value;

                    //sacamos minutos
                    times = times / 60;

                    //sacamos hotas
                    times = times / 60;

                    activities.time = times;

                    if (times > 0.01)
                        datos.activities.Add(activities);
                }
            }
            datos.activities = datos.activities.OrderByDescending(o => o.time).ToList();

            List<SelectListItem> sliu = CreateList(db.Agent_Employee.Where(u => u.IdCompany == company), "Usuario", "Usuario", activity.user).ToList();
            sliu.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.user = sliu;

            List<SelectListItem> sli = CreateList(db.Agent_EmployeesGroups.Where(c => c.Agent_Empresa.IdCompany == company).ToList(), "idemployeesGroup", "Nombre", activity.idgruoup);
            sli.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.idgruoup = sli;

            return View(datos);
        }


        #endregion


        #region Alerts

        public void alert()
        {
            //primero borramos las alertas del dia anterior
            db.Alerts.RemoveRange(db.Alerts.Where(g => g.date < DateTime.Today).ToList());
            db.SaveChanges();

            MongoHelper.TrakerBase = MongoHelper.database.GetCollection<TrakerBase>("TrackerTime");
            var builder = Builders<TrakerBase>.Filter;

            List<Alertas> _alertas = db.Alertas.ToList();
            //var filter = builder.Gte("Date", DateTime.Today.AddDays(-5));
            var filter = builder.Gte("Date", DateTime.Today);

            var results = MongoHelper.TrakerBase.Find(filter).ToList();

            List<Agent_ProgramClasification> clasif = db.Agent_ProgramClasification.ToList();
            List<AlertsDataViewModel> lavm = new List<AlertsDataViewModel>();

            foreach (var i in results.GroupBy(g => g.IdEmpresa))
            {
                foreach (var j in results.Where(t => t.IdEmpresa == i.Key).GroupBy(g => g.UserName))
                {
                    foreach (var l in results.Where(t => t.IdEmpresa == i.Key && t.UserName == j.Key).GroupBy(a => a.Application))
                    {
                        AlertsDataViewModel avm = new AlertsDataViewModel();
                        Guid idempresa = Guid.Parse(i.Key);
                        avm.idempresa = i.Key;
                        avm.username = j.Key;
                        avm.application = l.Key;
                        avm.activity = results.Where(t => t.IdEmpresa == i.Key && t.UserName == j.Key && t.Application == l.Key).Sum(r => r.Activity);
                        avm.inactivity = results.Where(t => t.IdEmpresa == i.Key && t.UserName == j.Key && t.Application == l.Key).Sum(r => r.Inactivity).Value;
                        avm.clasification = clasif.Where(c => c.Agent_Empresa.IdCompany == idempresa && c.name == l.Key).Select(v => v.clasification).FirstOrDefault();
                        lavm.Add(avm);
                    }
                }
            }

            if (lavm.Count() > 0)
            {
                //clasificaciones
                // 1 prodictivas
                // 2 improductivas
                // 3 neutrales
                // 

                // TIPOS DE ALERTAS
                // 2: APLIACIONES IMPRODUCTIVAS CON MAS DE 30 MIN DE USO
                // 3: MAS DE 30 MIN DE INACTIVIDAD ACUMULADA
                // 4: NO REPORTA DESDE HACE MAS DE 30 MIN

                foreach (var n in lavm.GroupBy(f => f.idempresa))
                {
                    foreach (var m in lavm.Where(t => t.idempresa == n.Key).GroupBy(u => u.username))
                    {
                        //sacamos la cantidad de tiempo de aplicaciones impoductivas de cada usuario
                        double improductivity = lavm.Where(p => p.username == m.Key && p.clasification == 2).Sum(s => s.activity);
                        Guid _idempresa = Guid.Parse(n.Key);

                        //alerta de usuario con mas de 30 min de uso en aplicaciones improductivas
                        if (improductivity >= 30 && db.Alerts.Where(r => r.tipo == 2 && r.Agent_Employee.Usuario == m.Key && r.Agent_Employee.IdCompany == _idempresa).Count() == 0)
                        {
                            Alerts a = new Alerts();
                            a.IdAlerts = Guid.NewGuid();
                            a.Agent_Employee = db.Agent_Employee.Where(d => d.IdCompany == _idempresa && d.Usuario == m.Key).SingleOrDefault();
                            a.tipo = 2;
                            a.status = false;
                            a.date = DateTime.Now;
                            db.Alerts.Add(a);
                        }

                        double inactivity = lavm.Where(p => p.username == m.Key).Sum(s => s.inactivity);

                        //Alerts alla = db.Alerts.Where(r => r.tipo == 3 && r.Agent_Employee.Usuario == m.Key && r.Agent_Employee.IdCompany == _idempresa).SingleOrDefault();

                        //alerta de usuario con mas de 30 min de inactividad acumulada
                        if (inactivity >= 30 && db.Alerts.Where(r => r.tipo == 3 && r.Agent_Employee.Usuario == m.Key && r.Agent_Employee.IdCompany == _idempresa).Count() == 0)
                        {
                            Alerts a = new Alerts();
                            a.IdAlerts = Guid.NewGuid();
                            a.Agent_Employee = db.Agent_Employee.Where(d => d.IdCompany == _idempresa && d.Usuario == m.Key).SingleOrDefault();
                            a.tipo = 3;
                            a.status = false;
                            a.date = DateTime.Now;
                            db.Alerts.Add(a);
                        }


                        // no reporta desde hace mas de 30 minE

                        //nos traemos la ultima vez que reporto
                        DateTime lastreport = results.Where(p => p.UserName == m.Key && p.IdEmpresa == n.Key).OrderByDescending(o => o.Date).Select(f => f.Date).FirstOrDefault();

                        //sacamos la cuenta de cuantos minutos han pasado desde su ultimo reporte
                        var lastreportmin_ = (DateTime.Now - lastreport).TotalMinutes;

                        //si mi ultimo reporte fue hace mas de 30 min, entonces alerto
                        if (lastreportmin_ >= 30 && db.Alerts.Where(r => r.tipo == 4 && r.Agent_Employee.Usuario == m.Key && r.Agent_Employee.IdCompany == _idempresa).Count() == 0)
                        {
                            Alerts a = new Alerts();
                            a.IdAlerts = Guid.NewGuid();
                            a.Agent_Employee = db.Agent_Employee.Where(d => d.IdCompany == _idempresa && d.Usuario == m.Key).SingleOrDefault();
                            a.tipo = 4;
                            a.status = false;
                            a.date = DateTime.Now;
                            db.Alerts.Add(a);
                        }
                        else
                        {
                            //si no consigue nada tratamos de borrar ya que ahora si esta reportando
                            db.Alerts.RemoveRange(db.Alerts.Where(r => r.tipo == 4 && r.Agent_Employee.Usuario == m.Key && r.Agent_Employee.IdCompany == _idempresa).ToList());
                        }
                    }

                    db.SaveChanges();

                    ///DESPUES QUE HACEMOS LAS VALIDACIONES, BUSCAMOS LO QUE HAY QUE REPORTAR PARA ENVIAR LOS CORREOS

                    List<Alerts> Alerts_ = db.Alerts.Where(t => t.status == false).Include(a => a.Agent_Employee).ToList();
                    foreach (var c in Alerts_.Where(t => t.Agent_Employee != null).GroupBy(g => g.Agent_Employee.IdCompany))
                    {
                        Agent_Empresa empr = db.Agent_Empresa.Where(e => e.IdCompany == c.Key).SingleOrDefault();
                        List<string> users_ = new List<string>();
                        foreach (var d in Alerts_.Where(t => t.Agent_Employee != null && t.Agent_Employee.IdCompany == c.Key).GroupBy(l => l.tipo))
                        {
                            users_ = new List<string>();
                            foreach (var e in Alerts_.Where(t => t.Agent_Employee != null && t.Agent_Employee.IdCompany == c.Key && t.tipo == d.Key))
                            {
                                users_.Add(e.Agent_Employee.Usuario);
                            }
                            List<string> _AlertAsociados = new List<string>();
                            if (users_.Count() > 0)
                            {
                                //traemos la listade correos por el tipo de alerta
                                Guid alertid = _alertas.Where(g => g.type == d.Key).Select(j => j.Id).SingleOrDefault();
                                _AlertAsociados = db.AlertAsociados.Where(t => t.Alertas.Id == alertid && t.Agent_Empresa.IdCompany == c.Key).Select(p => p.Email).ToList();

                                if (_AlertAsociados.Count() > 0)
                                {
                                    EmailController ec = new EmailController();
                                    //ec.SendAlert(_AlertAsociados, d.Key, users_);
                                }
                            }

                            if (_AlertAsociados.Count() > 0)
                            {
                                List<Alerts> laedit = db.Alerts.Where(h => h.Agent_Employee.IdCompany == c.Key && h.tipo == d.Key).ToList();

                                foreach (var o in laedit)
                                {
                                    o.status = true;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Alertas(Guid? Id)
        {
            AlertModuleViewModel avm = new AlertModuleViewModel();
            var company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

            List<AlertAsociados> oAlerta = new List<AlertAsociados>();
            if (Id != null && Id != Guid.Empty)
            {
                oAlerta = db.AlertAsociados.Where(p => p.Alertas.Id == Id && p.IdCompany == company).Include(a => a.Alertas).ToList();
                foreach (var j in oAlerta)
                {
                    AlertListViewModel alaso = new AlertListViewModel();
                    alaso.id = j.Id;
                    alaso.alert = j.Alertas.Alerta;
                    alaso.mail = j.Email;
                    avm.AlertListViewModel.Add(alaso);
                }
            }

            List<SelectListItem> alert = CreateList(db.Alertas.ToList(), "Id", "Alerta");
            alert.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.Id = alert;

            return View(avm);

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Alertas(AlertModuleViewModel alert)
        {
            if (ModelState.IsValid)
            {
                var company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());

                if (alert.id != null && !string.IsNullOrEmpty(alert.AlertAsociados?.Email))
                {
                    Guid idalet = Guid.Parse(alert.id);

                    if (db.AlertAsociados.Where(t => t.IdCompany == company && t.Email == alert.AlertAsociados.Email && t.Alertas.Id == idalet).Count() == 0)
                    {
                        alert.AlertAsociados.Id = Guid.NewGuid();
                        alert.AlertAsociados.IdCompany = company;
                        alert.AlertAsociados.Alertas = db.Alertas.Where(x => x.Id == idalet).SingleOrDefault();
                        db.AlertAsociados.Add(alert.AlertAsociados);
                        db.SaveChanges();
                        Success("Registro exitoso");
                    }
                    else
                        Warning("Email ya se encuentra reacionado con esta alerta", string.Empty);
                }
            }

            List<SelectListItem> lalert = CreateList(db.Alertas.ToList(), "Id", "Alerta", alert.AlertAsociados?.Id);
            lalert.Insert(0, (new SelectListItem { Text = "Seleccione", Value = Guid.Empty.ToString() }));
            ViewBag.Id = lalert;

            return RedirectToAction("Alertas", new { Id = alert.id });
        }

        [HttpPost]
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AlertAsociados alertAsociados = db.AlertAsociados.Find(id);
            if (alertAsociados == null)
            {
                return HttpNotFound();
            }
            else
            {
                db.AlertAsociados.Remove(alertAsociados);
                db.SaveChanges();
                Success("Elimado exitosamente");
                return RedirectToAction("Alertas");
            }

        }

        #endregion
    }
}