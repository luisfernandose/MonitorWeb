﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Queue.Utils
{
    public class Enums
    {
        public enum EnumDays
        {
            [Display(Name = "Seleccione")]
            Seleccione,
            Lunes,
            Martes,
            Miércoles,
            Jueves,
            Viernes,
            Sábado,
            Domingo
        }

        public enum EnumType
        {
            [Display(Name = "Seleccione")]
            Seleccione,
            Laboral,
            [Display(Name = "No Laboral")]
            NoLaboral
        }
    }
}