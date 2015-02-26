using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Fable.Models
{
    public class Roles
    {
        public static string CanCreateAbsence { get { return "canCreateAbsence"; } }
        public static string CanCreateApplication { get { return "canCreateApplication"; } }
        public static string CanViewOwnApplications { get { return "canViewOwnApplications"; } }

        public static ICollection<string> AdminRoles { get { return new Collection<string>(); } }

        public static ICollection<string> TeacherRoles
        {
            get
            {
                return new Collection<string>
                {
                    CanCreateAbsence,
                    CanViewOwnApplications,
                };
            }
        }

        public static ICollection<string> SubstituteRoles
        {
            get
            {
                return new Collection<string>
                {
                    CanCreateApplication,
                };
            }
        } 

        public static ICollection<string> AllRoles
        {
            get
            {
                return new Collection<string>
                {
                    CanCreateAbsence,
                    CanCreateApplication,
                    CanViewOwnApplications,
                };
            }
        }
    }
}