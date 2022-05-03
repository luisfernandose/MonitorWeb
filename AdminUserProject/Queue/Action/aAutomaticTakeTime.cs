using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Queue.Controllers.Token;
using Queue.Models;
using static Queue.Common.CommonEnum;
using Queue.DAL;
using Queue.ViewModels;
using Queue.Controllers;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;

namespace Queue.Action
{
    public class aAutomaticTakeTime
    {
        aUtilities autil = new aUtilities();
        ClaimsPrincipal cp = new ClaimsPrincipal();
        TokenValidationHandler tvh = new TokenValidationHandler();
        OperationController opc = new OperationController();

        //CREA LA ACTIVIDAD QUE EL USUARIO ESTA REALIZANDO
        public object CreateAutomaticTakeTime(TrackerModel o)
        {
            Response rp = new Response();
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            try
            {
                cp = tvh.getprincipal(Convert.ToString(o.token));
                if (cp != null)
                {
                    List<TrakerBase> ltb = new List<TrakerBase>();

                    foreach (var i in o.AutomaticTakeTimeModel)
                    {
                        TrakerBase tb = new TrakerBase();
                        Copier.CopyPropertiesTo(i, tb);
                        ltb.Add(tb);
                    }

                    if (ltb.Count() > 0)
                    {
                        bool response = opc.AddTracker(ltb);
                        if (response)
                            rp.response_code = GenericErrors.SaveOk.ToString();
                        else
                            rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    //token invalido
                    rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.InvalidToken, string.Empty, null, HttpStatusCode.OK);
                    return rp;
                }

                return rp;
            }
            catch (Exception ex)
            {
                //error general
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }
        }

        //CREA CAPTURAS DE PANATALLA DE LOS USUARIOS
        public object CreateCapture(CaptureModel o)
        {
            Response rp = new Response();
            try
            {
                cp = tvh.getprincipal(Convert.ToString(o.token));
                if (cp != null)
                {
                    CaptureBase cb = new CaptureBase();
                    Copier.CopyPropertiesTo(o, cb);
                    if (cb != null)
                    {
                        bool response = opc.AddCapture(cb);
                        if (response)
                            rp.response_code = GenericErrors.SaveOk.ToString();
                        else
                            rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);

                        List<CaptureBase> getimage = opc.GeImage(o.IdCompany);
                    }

                }
            }
            catch (Exception ex)
            {
                //error general
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }
            return rp;
        }

        //CREA LOS PROGRAMAS QUE TIENN LOS USUARIOS INSTALADOS EN SUS MAQUINAS, SI YA NO LOS TIENE LOS MARCA COMO DESACTIVADOS
        public object SaveIntalledPrograms(InstalledModel o)
        {
            Response rp = new Response();
            try
            {
                cp = tvh.getprincipal(Convert.ToString(o.token));
                if (cp != null)
                {
                    string empresa = string.Empty;

                    List<InstalledProgramsViewModel> livm = new List<InstalledProgramsViewModel>();
                    foreach (var i in o.InstalledProgramsViewModel)
                    {
                        InstalledProgramsViewModel ivm = new InstalledProgramsViewModel();
                        Copier.CopyPropertiesTo(i, ivm);
                        empresa = i.IdCompany;
                        livm.Add(ivm);
                    };

                    if (livm.Count() > 0)
                    {
                        List<InstalledProgramsViewModel> installed = opc.GetSoftWare(empresa.ToString(), livm[0].Pc);
                        foreach (var i in installed)
                        {
                            //paso 1, validar las que ya estan en base de datos, para agregar las que no
                            InstalledProgramsViewModel ipvm = livm.Where(l => l.Name == i.Name && l.Vertion == i.Vertion).FirstOrDefault();
                            //si no esta en la lista que viene del monitor, es porque se desinstalo
                            if (ipvm == null)
                            {
                                opc.UpdateSotfware(i);
                            }
                            else
                            {
                                livm.Remove(ipvm);
                            }
                        }

                        if (livm.Count() > 0)
                        {
                            opc.AddSoftware(livm);
                        }
                        rp.response_code = GenericErrors.SaveOk.ToString();
                    }
                    else
                    {
                        //token invalido
                        rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.InvalidToken, string.Empty, null, HttpStatusCode.OK);
                        return rp;
                    }
                    return rp;
                }
            }
            catch (Exception ex)
            {
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }
            return rp;
        }

        //GUARDA EL HARDWARE QUE TIENEN LOS USUARIOS, REVISA SI HAY NUEVOS Y LOS QUE YA NO, LOS MARCA COMO DESINSTALADOS
        public object SaveHardware(HardwareModel model)
        {
            Response rp = new Response();
            try
            {
                cp = tvh.getprincipal(Convert.ToString(model.token));
                if (cp != null)
                {
                    string empresa = string.Empty;

                    List<InstalledHardwareViewModel> livm = new List<InstalledHardwareViewModel>();

                    foreach (var i in model.InstalledHardwareViewModel)
                    {
                        InstalledHardwareViewModel ivm = new InstalledHardwareViewModel();
                        empresa = i.IdCompany;
                        Copier.CopyPropertiesTo(i, ivm);
                        livm.Add(ivm);
                    };
                    if (livm.Count() > 0)
                    {
                        List<InstalledHardwareViewModel> installed = opc.GetHardware(empresa, livm[0].Pc);

                        foreach (var i in installed)
                        {
                            //paso 1, validar las que ya estan en base de datos, para agregar las que no
                            InstalledHardwareViewModel ipvm = livm.Where(l => l.Hardware == i.Hardware && l.Type == i.Type).SingleOrDefault();
                            //si no esta en la lista que viene del monitor, es porque se desinstalo
                            if (ipvm == null)
                            {
                                opc.UpdateHardware(i);
                            }
                            else
                            {
                                //si si esta, entonces la quito de la lista que viene del monitor para quedarme solo con las que no estan en BD que son las nuevas
                                livm.Remove(ipvm);
                            }
                        }

                        //si qedo algo lo agrego
                        if (livm.Count() > 0)
                        {
                            opc.AddHardware(livm);
                        }
                    }
                    rp.response_code = GenericErrors.SaveOk.ToString();
                }
                else
                {
                    //token invalido
                    rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.InvalidToken, string.Empty, null, HttpStatusCode.OK);
                    return rp;
                }

                return rp;
            }
            catch (Exception ex)
            {
                //error general
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }
            return rp;
        }


        //GUARDA LOGS, PARA HACER DEBUG D EERRORES O PARA GUARDAR LOGS DE LO QUE SE NECESITE
        public object Createlog(LogsViewModel o)
        {
            Response rp = new Response();
            try
            {
                bool response = opc.AddLog(o);
                if (response)
                    rp.response_code = GenericErrors.SaveOk.ToString();
                else
                    rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);

                return rp;
            }
            catch (Exception ex)
            {
                //error general
                rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, ex.Message + " " + ex.InnerException, null, HttpStatusCode.InternalServerError);
                return rp;
            }
        }

        public object GetHorary(HoraryModel hm)
        {
            Response rp = new Response();
            try
            {
                cp = tvh.getprincipal(Convert.ToString(hm.token));
                if (cp != null)
                {
                    using (QueueContext ent = new QueueContext())
                    {
                        Guid IdEmpresa = Guid.Parse(hm.idempresa);

                        Agent_Employee ege = ent.Agent_Employee.Where(r => r.IdCompany == IdEmpresa && r.Usuario.ToLower() == hm.user.ToLower()).Include(g => g.Agent_GroupHorary).SingleOrDefault();
                        if (ege != null)
                        {
                            if (ege.Agent_GroupHorary != null)
                            {
                                rp.HoraryModel.Agent_GroupHoraryDetail = ent.Agent_GroupHoraryDetail
                                                                        .Where(d => d.Agent_GroupHorary.Id_GroupHorary == ege.Agent_GroupHorary.Id_GroupHorary)
                                                                        .Select(p => new HoraryDetail
                                                                        {
                                                                            Day = p.Day,
                                                                            HourFrom = p.HourFrom,
                                                                            HourUntil = p.HourUntil,
                                                                            Type = p.Type
                                                                        }).ToList();
                            }
                        }
                    }
                    //retorna un response, con el campo data lleno con la respuesta.               
                    return autil.ReturnMesagge(ref rp, (int)GenericErrors.OK, null, null, HttpStatusCode.OK);
                }
                else
                {
                    //token invalido
                    rp = autil.ReturnMesagge(ref rp, (int)GenericErrors.InvalidToken, string.Empty, null, HttpStatusCode.OK);
                    return rp;
                }
            }
            catch (Exception ex)
            {
                //error general               
                return autil.ReturnMesagge(ref rp, (int)GenericErrors.GeneralError, string.Empty, null, HttpStatusCode.InternalServerError);
            }
        }
    }
}