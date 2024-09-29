using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            Categories = new HashSet<Category>();
            Enrolleds = new HashSet<Enrolled>();
        }

        public ushort Year { get; set; }
        public string Season { get; set; } = null!;
        public string Location { get; set; } = null!;
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public uint ClassId { get; set; }
        public uint CourseId { get; set; }
        public string UId { get; set; } = null!;

        public virtual Course Course { get; set; } = null!;
        public virtual Professor UIdNavigation { get; set; } = null!;
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
    }
}
