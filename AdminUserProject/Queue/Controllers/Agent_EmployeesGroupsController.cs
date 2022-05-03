using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Queue.DAL;
using Queue.Models;

namespace Queue.Controllers
{
    [Authorize]
    public class Agent_EmployeesGroupsController : BaseController
    {
        private QueueContext db = new QueueContext();

        // GET: Agent_EmployeesGroups
        public ActionResult Index()
        {
            var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
            var guidCompany = Guid.Parse(company);

            return View(db.Agent_EmployeesGroups.Where(d => d.Agent_Empresa.IdCompany == guidCompany).ToList());
        }

        // GET: Agent_EmployeesGroups/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Agent_EmployeesGroups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Agent_EmployeesGroups agent_EmployeesGroups)
        {
            if (ModelState.IsValid)
            {
                var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
                var guidCompany = Guid.Parse(company);

                if (db.Agent_EmployeesGroups.Where(f => f.Agent_Empresa.IdCompany == guidCompany && f.Nombre == agent_EmployeesGroups.Nombre).Count() == 0)
                {
                    agent_EmployeesGroups.idemployeesGroup = Guid.NewGuid();
                    agent_EmployeesGroups.Agent_Empresa = db.Agent_Empresa.Where(d => d.IdCompany == guidCompany).SingleOrDefault();
                    db.Agent_EmployeesGroups.Add(agent_EmployeesGroups);
                    db.SaveChanges();
                    Success("Registro creado con exito");
                    return RedirectToAction("Index");
                }
                else
                    Warning("Grupo ya existe", "");
            }

            return View(agent_EmployeesGroups);
        }

        // GET: Agent_EmployeesGroups/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
            var guidCompany = Guid.Parse(company);

            Agent_EmployeesGroups agent_EmployeesGroups = db.Agent_EmployeesGroups.Find(id);

            List<Agent_Employee> asigemp = db.Agent_EmployeeGroupsEmployee.Where(b => b.Agent_EmployeesGroups.Agent_Empresa.IdCompany == guidCompany).Select(t => t.Agent_Employee).ToList();
            List<Agent_Employee> empl = db.Agent_Employee.Where(s => s.IdCompany == guidCompany).ToList();

            empl = empl.Except(asigemp).ToList();

            ViewBag.asigemp = asigemp;
            ViewBag.idemployee = new SelectList(empl, "idEmployee", "Nombre").ToList();
            agent_EmployeesGroups.Agent_EmployeeGroupsEmployee_list.AddRange(asigemp);
            if (agent_EmployeesGroups == null)
            {
                return HttpNotFound();
            }
            return View(agent_EmployeesGroups);
        }

        // POST: Agent_EmployeesGroups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Agent_EmployeesGroups agent_EmployeesGroups)
        {
            if (ModelState.IsValid)
            {
                var company = Request.RequestContext.HttpContext.Session["Company"].ToString();
                var guidCompany = Guid.Parse(company);

                if (db.Agent_EmployeesGroups.Where(f => f.Agent_Empresa.IdCompany == guidCompany && f.Nombre == agent_EmployeesGroups.Nombre && f.idemployeesGroup != agent_EmployeesGroups.idemployeesGroup).Count() == 0)
                {
                    //agent_EmployeesGroups.Agent_Empresa = db.Agent_Empresa.Where(d => d.IdCompany == guidCompany).SingleOrDefault();
                    db.Entry(agent_EmployeesGroups).State = EntityState.Modified;
                    db.SaveChanges();
                    Success("Registro editado con exito");
                    return RedirectToAction("Index");
                }
                else
                    Warning("Grupo ya existe", "");
            }
            return View(agent_EmployeesGroups);
        }
        [HttpPost]
        public ActionResult Addemployee(Guid idemployeesGroup, Guid idemployee)
        {

            Agent_EmployeeGroupsEmployee eegg = new Agent_EmployeeGroupsEmployee();
            eegg.idAgent_EmployeeGroupsEmployee = Guid.NewGuid();
            eegg.Agent_Employee = db.Agent_Employee.Where(f => f.idEmployee == idemployee).SingleOrDefault();
            eegg.Agent_EmployeesGroups = db.Agent_EmployeesGroups.Where(a => a.idemployeesGroup == idemployeesGroup).SingleOrDefault();
            db.Agent_EmployeeGroupsEmployee.Add(eegg);
            db.SaveChanges();

            Success("Registro editado con exito");
            return RedirectToAction("Edit", new { id = idemployeesGroup });
        }

        public ActionResult Deleteemployee(Guid idEmployee, Guid idAgent_EmployeeGroupsEmployee)
        {
            Agent_EmployeeGroupsEmployee eegg = db.Agent_EmployeeGroupsEmployee.Where(f => f.Agent_Employee.idEmployee == idEmployee && f.Agent_EmployeesGroups.idemployeesGroup == idAgent_EmployeeGroupsEmployee).Include(a => a.Agent_EmployeesGroups).SingleOrDefault();
            Guid idgroup = eegg.Agent_EmployeesGroups.idemployeesGroup;
            db.Agent_EmployeeGroupsEmployee.Remove(eegg);
            db.SaveChanges();

            Success("Registro eliminado con exito");
            return RedirectToAction("Edit", new { id = idgroup });
        }



        // GET: Agent_EmployeesGroups/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agent_EmployeesGroups agent_EmployeesGroups = db.Agent_EmployeesGroups.Find(id);
            if (agent_EmployeesGroups == null)
            {
                return HttpNotFound();
            }
            return View(agent_EmployeesGroups);
        }

        // POST: Agent_EmployeesGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {

            if (db.Agent_EmployeeGroupsEmployee.Where(g => g.Agent_EmployeesGroups.idemployeesGroup == id).Count() == 0)
            {

                Agent_EmployeesGroups agent_EmployeesGroups = db.Agent_EmployeesGroups.Where(a => a.idemployeesGroup == id).SingleOrDefault();
                db.Agent_EmployeesGroups.Remove(agent_EmployeesGroups);
                db.SaveChanges();
                Success("Registro eliminado con exito");
                return RedirectToAction("Index");
            }
            else
            {
                Warning("El registro contiene datos asociados, no se puede eliminar", "");
            }
            return RedirectToAction("Delete", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
