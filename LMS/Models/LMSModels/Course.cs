using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Course
    {
        public Course()
        {
            Classes = new HashSet<Class>();
        }

        public string Name { get; set; } = null!;
        public ushort Number { get; set; }
        public uint DepartId { get; set; }
        public uint CourseId { get; set; }

        public virtual Department Depart { get; set; } = null!;
        public virtual ICollection<Class> Classes { get; set; }
    }
}
