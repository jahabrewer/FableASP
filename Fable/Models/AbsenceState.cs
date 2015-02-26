using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fable.Models
{
    public enum AbsenceState : byte
    {
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        New = 0,
        /// <summary>
        /// Absence can be applied to and has no Fulfiller yet.
        /// </summary>
        Open,
        /// <summary>
        /// Absence has a Fulfiller and has not yet happened.
        /// </summary>
        Assigned,
        /// <summary>
        /// Absence is occurring.
        /// </summary>
        InProgress,
        /// <summary>
        /// Absence is over.
        /// </summary>
        Closed,
    }
}