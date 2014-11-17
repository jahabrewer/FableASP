using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fable.Models
{
    public class Application
    {
        public int ApplicationId { get; set; }
        public ApplicationState ApplicationState { get; set; }
        public DateTime ApplicationStateModified { get; set; }
        
        // navigation properties
        public virtual ApplicationUser Applicant { get; set; }
        public virtual Absence Absence { get; set; }
    }

    public enum ApplicationState
    {
        WaitingForDecision,
        Rejected,
        Accepted,
        Retracted,
    }
}