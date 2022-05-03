﻿using Newtonsoft.Json;
using Queue.DAL;
using Queue.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using System.Web;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Threading.Tasks;

namespace Queue.Controllers
{
    [Authorize]
    public class ReportGanttController : Controller
    {
        private QueueContext db = new QueueContext();
        // GET: ReportGantt
        public ActionResult Index()
        {
            try
            {
                ViewBag.DateFrom = DateTime.Now.AddDays(-2);
                ViewBag.DateTo = DateTime.Now;

                var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
                OperationController opc = new OperationController();
                ViewBag.DataUser = opc.GetActivityUser(company).ToList();

                return View();
            }
            catch (Exception err)
            {
                return RedirectToAction("Login", "Account", null);
                //throw err;
            }
        }

        private object MultiSelectList(List<SelectListItem> selectListItems)
        {
            throw new NotImplementedException();
        }

        // GET: ReportGantt/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ReportGantt/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReportGantt/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ReportGantt/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ReportGantt/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ReportGantt/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        public async Task<JsonResult> NewChart(DateTime? dateFrom, DateTime? dateTo, int periods, string[] user)
        {
            //try
            //{
            var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
            OperationController opc = new OperationController();
            var result = await opc.GetactivityData(company, dateFrom.Value, dateTo.Value, periods, user);
            return Json(result, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception err)
            //{
            //    throw err;
            //}

        }

        public async Task<JsonResult> NewChartUserSelected(DateTime? dateFrom, DateTime? dateTo, string user)
        {
            var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
            OperationController opc = new OperationController();
            var result = await opc.GetactivityDataUserSelected(company, dateFrom.Value, dateTo.Value, user);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
              
        public JsonResult UserbyDepartamentData(string dateFrom, string dateTo, string idUser, string idDeparment)
        {
            try
            {
                var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
                OperationController opc = new OperationController();
                DateTime fromdate = Convert.ToDateTime(dateFrom);
                DateTime todate = Convert.ToDateTime(dateTo);

                //BasicStatsModel bm = opc.MoreUsedApp(company, fromdate, todate);

                var UserByDeparment = db.Agent_Employee.Where(x => x.Agent_CompanyDepartment.Id.ToString() == idDeparment && x.idEmployee.ToString() == idUser)
                .Distinct().ToList();
                BasicUserModel User = opc.GetUserByName(company, fromdate, todate, UserByDeparment[0].Usuario);
                var ApplicationDist = User.Application.Distinct().ToList();

                ArrayUsersModel UserModel = new ArrayUsersModel();
                List<object> iData = new List<object>();
                Random rnd = new Random();
                string UpdateDate = "";
                double SumTime = 0;

                for (var k = 0; k < ApplicationDist.Count; k++)
                {
                    UserModel.Application.Add(ApplicationDist[k]);
                    for (var cont = 0; cont < User.User.Count; cont++)
                    {
                        if (User.Application[cont] == ApplicationDist[k])
                        {
                            SumTime = SumTime + User.Time[cont];
                        }
                    }
                    var round = Math.Round((SumTime / 3600), 2);
                    UserModel.Time.Add(round);
                    SumTime = 0;
                    UserModel.User = User.User[0];
                };

                UpdateDate = JsonConvert.SerializeObject(UserModel);

                iData.Add(UpdateDate);


                return Json(iData, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public JsonResult GetUser(string idDeparment)
        {
            try
            {
                var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
                OperationController opc = new OperationController();
                var result = db.Agent_Employee.Where(d => d.Agent_CompanyDepartment.Id.ToString() == idDeparment).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception err)
            {
                throw err;
            }

        }
        // POST: ReportGantt/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
