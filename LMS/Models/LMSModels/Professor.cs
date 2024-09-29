using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Professor
    {
        public Professor()
        {
            Classes = new HashSet<Class>();
        }

        public string UId { get; set; } = null!;
        public string FName { get; set; } = null!;
        public string LName { get; set; } = null!;
        public DateOnly Dob { get; set; }
        public uint DepartId { get; set; }

        public virtual Department Depart { get; set; } = null!;
        public virtual ICollection<Class> Classes { get; set; }
    }
}
