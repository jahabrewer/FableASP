using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Fable.Models
{
    public class Absence
    {
        public int AbsenceId { get; set; }

        [DataType(DataType.Text), MaxLength(32)]
        public string Location { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Start { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime End { get; set; }
        [DataType(DataType.MultilineText), MaxLength(512)]
        public string Description { get; set; }

        // navigation properties
        public virtual ApplicationUser Absentee { get; set; }
        public virtual ApplicationUser Fulfiller { get; set; }
        public virtual School School { get; set; }
    }
}