using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fable.Models
{
    public class AbsenceDetailsViewModel
    {
        public Absence Absence { get; set; }
        /// <summary>
        /// If the user is a substitute and has an application for this
        /// absence, that application, otherwise null.
        /// </summary>
        public Application MyApplication { get; set; }
        /// <summary>
        /// Whether to display the applications for the absence.
        /// </summary>
        public bool ShowApplications { get; set; }
    }
}