using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query = from d in db.Departments
                        select new
                        {
                            name = d.Name,
                            subject = d.Subject
                        };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query = from d in db.Departments
                        select new
                        {
                            subject = d.Subject,
                            dname = d.Name,
                            courses = (from c in db.Courses
                                       where c.DepartId == d.DepartId
                                       select new
                                       {
                                           number = c.Number,
                                           cname = c.Name
                                       }).ToArray()
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            uint departID;
            if (!AdministratorController.getDepartID(subject, db, out departID))
                return Json(new { success = false });

            var classes =
                (from a in db.Classes
                 join b in db.Professors
                 on a.UId equals b.UId
                 join c in db.Courses
                 on a.CourseId equals c.CourseId
                 where c.DepartId == departID
                 && c.Number == number
                 select new {season = a.Season, year = a.Year, location = a.Location, start = a.Start, end = a.End, fname = b.FName, lname = b.LName}
                ).ToList();
            return Json(classes);
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            // find the match assignment with all these parameters
            var findAssignment = db.Assignments.FirstOrDefault(a =>
                a.Category.Class.Course.Depart.Subject == subject &&
                a.Category.Class.Course.Number == num &&
                a.Category.Class.Season == season &&
                a.Category.Class.Year == year &&
                a.Category.Name == category &&
                a.Name == asgname);

            if (findAssignment != null)
            {
                return Content(findAssignment.Contents);
            }
            else
            {
                return Content("This assignment is not exist.");
            }
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            // find the match submission with all these parameters
            var findSubmission = db.Submissions.FirstOrDefault(s =>
                s.Assignment.Category.Class.Course.Depart.Subject == subject &&
                s.Assignment.Category.Class.Course.Number == num &&
                s.Assignment.Category.Class.Season == season &&
                s.Assignment.Category.Class.Year == year &&
                s.Assignment.Category.Name == category &&
                s.Assignment.Name == asgname &&
                s.UId == uid);

            // if there is a submission found, Returns the contents of an assignment submission.
            if (findSubmission != null)
            {
                return Content(findSubmission.Contents);
            }
            else
            {
                //Returns the empty string ("") if there is no submission.
                return Content("");
            }
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var studentQuery = from s in db.Students
                               where s.UId == uid
                               select new
                               {
                                   fname = s.FName,
                                   lname = s.LName,
                                   uid = s.UId,
                                   department = s.Depart.Name
                               };

            var professorQuery = from s in db.Professors
                                 where s.UId == uid
                                 select new
                                 {
                                     fname = s.FName,
                                     lname = s.LName,
                                     uid = s.UId,
                                     department = s.Depart.Name
                                 };

            var adminQuery = from s in db.Administrators
                             where s.UId == uid
                             select new
                             {
                                 fname = s.FName,
                                 lname = s.LName,
                                 uid = s.UId
                             };


            if (studentQuery.Count() != 0)
            {
                return Json(studentQuery.ToArray()[0]);
            }

            if (professorQuery.Count() != 0)
            {
                return Json(professorQuery.ToArray()[0]);
            }

            if (adminQuery.Count() != 0)
            {
                return Json(adminQuery.ToArray()[0]);
            }

            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

