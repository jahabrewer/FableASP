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

        public AbsenceState State { get; set; }
        [DataType(DataType.Text), MaxLength(32)]
        public string Location { get; set; }
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime Start { get; set; }
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime End { get; set; }
        [DataType(DataType.MultilineText), MaxLength(512)]
        public string Description { get; set; }

        // navigation properties
        [Required]
        public virtual ApplicationUser Absentee { get; set; }
        public virtual ApplicationUser Fulfiller { get; set; }
        [Required]
        public virtual School School { get; set; }
        public virtual IList<Application> Applications { get; set; }
    }
}