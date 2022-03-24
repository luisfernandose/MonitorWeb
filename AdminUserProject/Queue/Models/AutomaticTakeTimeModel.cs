using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Queue.Models
{
    public class AutomaticTakeTimeModel
    {
        public System.Guid Id { get; set; }
        public string Application { get; set; }
        public string Title { get; set; }
        public Nullable<double> Time { get; set; }
        public Nullable<double> Inactivity { get; set; }

        public string Ip { get; set; }
        public string Pc { get; set; }
        public string UserName { get; set; }
        public string User { get; set; }
        public string IdEmpresa { get; set; }

        public double Activity { get; set; }

        //public DateTime Date { get; set; }
        //public DateTime FocusTime { get; set; }

        public DateTime Date
        {
            get { return setdate; }
            set { setdate = value; }
        }
        public DateTime FocusTime
        {
            get { return setfocusdate; }
            set { setfocusdate = value; }
        }

        [NotMapped]
        public DateTime setdate
        {
            get { return DateTime.Parse(StringDate.Replace("/","-").Trim()); }
            set { }
        }

        [NotMapped]
        public DateTime setfocusdate
        {
            get { return DateTime.Parse(StringFocusTime.Replace("/", "-").Trim()); }
            set { }
        }

        public string StringDate { get; set; }
        public string StringFocusTime { get; set; }

        [Display(Name = "token")]
        [JsonProperty("token")]
        public string token { get; set; }
    }
}