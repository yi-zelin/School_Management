using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            var query =
                from t in db.Departments
                where t.Subject == subject
                select t.Subject;
            var temp = query.ToList();
            if (temp.Count != 0)
                return Json(new { success = false });

            // create department
            var dep = new Department
            {
                Name = name,
                Subject = subject
            };
            db.Departments.Add(dep);
            db.SaveChanges();
            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var courses = (from a in db.Courses
                           join b in db.Departments
                           on a.DepartId equals b.DepartId
                           where b.Subject == subject
                           select new {number = a.Number, name = a.Name}).ToList();
            return Json(courses);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var professors = (from a in db.Professors
                              join b in db.Departments
                              on a.DepartId equals b.DepartId
                              where b.Subject == subject
                              select new {lname = a.LName, fname = a.FName, uid = a.UId}).ToList();
            return Json(professors);
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            uint departID;
            if (!getDepartID(subject, db, out departID))
                return Json(new { success = false });

            // cerate Course
            var course = new Course
            {
                Name = name,
                Number = (ushort) number,
                DepartId = departID
            };

            db.Courses.Add(course);
            db.SaveChanges();
            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            /*
            Test:
                * create Course success
                * create class success
                # Case 1: another class occupies the same location within time range in same semester
                    * diff course same semester, location with/without time contract
                    * diff course, success when diff location or semester or time occupy
                # Case 2: exist same Course in same Semester
                    * diff year
                    * diff Course
                    * diff season
            */
            uint departID;
            if (!getDepartID(subject, db, out departID))
                return Json(new { success = false });

            var course1 =
                (from a in db.Classes
                join b in db.Courses
                on a.CourseId equals b.CourseId
                where a.Year == year
                && a.Season == season
                && b.DepartId == departID
                && b.Number == number
                 select a).ToList().Count;
            var course2 =
                (from a in db.Classes
                 where a.Location == location
                 && a.Season == season
                 && a.Year == year
                 && (
                    (a.Start <= TimeOnly.FromDateTime(end)
                    && TimeOnly.FromDateTime(end) <= a.End)
                 || (a.Start <= TimeOnly.FromDateTime(start)
                    && TimeOnly.FromDateTime(start) <= a.End)
                 )
                 select a).ToList().Count;
            if ( course1 != 0 || course2 != 0)
            {
                Debug.WriteLine("@CreateClass\n\tcase one get: " + course1 + "\n\tcase two get: " +  course2);
                return Json(new { success = false });
            }

            // get course id
            var courseID =
                (from a in db.Courses
                 where a.DepartId == departID
                 && a.Number == number
                 select a.CourseId).ToList();

            // create class
            var c = new Class
            {
                Year = (ushort) year,
                Season = season,
                Location = location,
                Start = TimeOnly.FromDateTime(start),
                End = TimeOnly.FromDateTime(end),
                CourseId = courseID[0],
                UId = instructor
            };

            db.Classes.Add(c);
            db.SaveChanges();
            return Json(new { success = true });
        }
        
        /// <summary>
        /// get id of given subject
        /// </summary>
        /// <param name="subject">department subject we are searching for</param>
        /// <param name="x">write value to x</param>
        /// <returns>true if only one found, else return false</returns>
        public static bool getDepartID (string subject, LMSContext db, out uint x)
        {
            var depart =
                (from t in db.Departments
                where t.Subject == subject
                select t.DepartId).ToList();
            if (depart.Count != 1)
            {
                x = 0; 
                return false;
            }
            x = depart[0];
            return true;
        }

        /*******End code to modify********/
    }
}

