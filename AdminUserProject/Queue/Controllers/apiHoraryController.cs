﻿using Queue.Action;
using Queue.Models;
using System.Net.Http;
using System.Web.Http;


namespace Queue.Controllers
{
    [RoutePrefix("api/apiHorary")]
    public class apiHoraryController : ApiController
    {
        aUtilities ut = new aUtilities();

        [HttpPost]
        [Route("GetHorary")]
        public HttpResponseMessage CreateTracker(HoraryModel t)
        {
            aAutomaticTakeTime s = new aAutomaticTakeTime();
            return ut.ReturnResponse(s.GetHorary(t));
        }
    }
}