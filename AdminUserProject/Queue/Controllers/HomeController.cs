using Queue.App_Start;
using Queue.DAL;
using Queue.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace Queue.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private QueueContext db = new QueueContext();

        [SessionAuthorize]
        public ActionResult Index()
        {
            DashBoardStats dasb = new DashBoardStats();
            Guid company = Guid.Parse(Request.RequestContext.HttpContext.Session["Company"].ToString());
            ViewBag.empleados = db.Agent_Employee.Where(f => f.IdCompany == company).OrderBy(o => o.Nombre).ToList();
            OperationController opc = new OperationController();


            List<BasicStatsDashboard> data = opc.GetDataForDashBoard(company.ToString());
            dasb.resume = GetResume(data);
            return View(dasb);
        }

        private Resume GetResume(List<BasicStatsDashboard> data)
        {
            Resume rs = new Resume();

            foreach (var k in data.GroupBy(g => g.Clasification))
            {
                decimal total = decimal.Parse(data.Where(f => f.Clasification == k.Key).Sum(b => b.Time).ToString());
                switch (k.Key)
                {
                    case 0:
                        rs.UnclasifyTime = total;
                        break;
                    case 1:
                        rs.ProductiveTime = total;
                        break;
                    case 2:
                        rs.ImproductiveTime = total;
                        break;
                    case 3:
                        rs.NeutralTime = total;
                        break;
                    default:
                        break;
                }
            }

            decimal totalactivity = rs.Total;

            rs.ProductiveTime = Math.Round((rs.ProductiveTime * 100) / totalactivity, 2);
            rs.ImproductiveTime = Math.Round((rs.ImproductiveTime * 100) / totalactivity, 2);
            rs.NeutralTime = Math.Round((rs.NeutralTime * 100) / totalactivity, 2);
            rs.UnclasifyTime = Math.Round((rs.UnclasifyTime * 100) / totalactivity, 2);

            return rs;
        }
        private List<AppResume> GetAppResume(List<BasicStatsDashboard> data)
        {
            List<AppResume> lrs = new List<AppResume>();
            AppResume rs;

            foreach (var k in data.GroupBy(g => g.Application))
            {
                rs = new AppResume();
                decimal total = decimal.Parse(data.Where(f => f.Application == k.Key).Sum(b => b.Time).ToString());
                rs.appname = k.Key;
                lrs.Add(rs);
            }

            return lrs.OrderBy(o => o.Time).Take(5).ToList();
        }

        [SessionAuthorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [SessionAuthorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public JsonResult MoreUsedApp()
        {
            var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
            OperationController opc = new OperationController();
            DateTime fromdate = DateTime.Today.AddDays(-10);
            DateTime todate = DateTime.Today;
            BasicStatsModel bm = opc.MoreUsedApp(company, fromdate, todate);
            return Json(bm, JsonRequestBehavior.AllowGet);
        }
    }
}