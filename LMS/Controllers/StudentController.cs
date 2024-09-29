using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from e in db.Enrolleds
                        where e.UId == uid
                        select new
                        {
                            subject = e.Class.Course.Depart.Subject,
                            number = e.Class.Course.Number,
                            name = e.Class.Course.Name,
                            season = e.Class.Season,
                            year = e.Class.Year,
                            grade = e.Grade != null ? e.Grade : "--"
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query = from c in db.Categories
                        join cl in db.Classes on c.ClassId equals cl.ClassId
                        join a in db.Assignments on c.CategoryId equals a.CategoryId
                        join e in db.Enrolleds.Where(e => e.UId == uid) on cl.ClassId equals e.ClassId into s
                        from e in s.DefaultIfEmpty()
                        join u in db.Submissions.Where(s => s.UId == uid) on a.AssignmentId equals u.AssignmentId into sub
                        from u in sub.DefaultIfEmpty()
                        where cl.Course.Depart.Subject == subject
                              && cl.Course.Number == num
                              && cl.Season == season
                              && cl.Year == year
                        select new
                        {
                            aname = a.Name,
                            cname = c.Name,
                            due = a.Due,
                            score = u == null ? "--" : u.Score.ToString()
                        };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            try
            {
                // try to find exist assignment 
                var findAssignment = db.Assignments.FirstOrDefault(a => a.Category.Class.Course.Depart.Subject == subject &&
                                                                        a.Category.Class.Course.Number == num &&
                                                                        a.Category.Class.Season == season &&
                                                                        a.Category.Class.Year == year &&
                                                                        a.Category.Name == category &&
                                                                        a.Name == asgname);
                if (findAssignment == null)
                {
                    return Content("this assignment doesn't exist!");
                }

                // try to find exist submission
                var findSubmission = db.Submissions.FirstOrDefault(s => s.UId == uid && s.AssignmentId == findAssignment.AssignmentId);
                if (findSubmission == null)
                {
                    // if there is no submisson found, then create a new one
                    var newSubmission = new Submission
                    {
                        Time = DateTime.Now,
                        Score = 0,
                        Contents = contents,
                        UId = uid,
                        AssignmentId = findAssignment.AssignmentId
                    };
                    db.Submissions.Add(newSubmission);
                }
                else
                {
                    // if there is a submission, then change the contents and time
                    findSubmission.Contents = contents;
                    findSubmission.Time = DateTime.Now;
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            try
            {
                // try to find if a student already enrolled in a class 
                var findEnrollment = db.Enrolleds.FirstOrDefault(e => e.UId == uid &&
                                                                      e.Class.Course.Depart.Subject == subject &&
                                                                      e.Class.Course.Number == num &&
                                                                      e.Class.Season == season &&
                                                                      e.Class.Year == year);
                if (findEnrollment != null)
                {
                    return Content("the student is already enrolled in this class!");
                }

                // try to find exist course 
                var findCourse = db.Courses.FirstOrDefault(c => c.Depart.Subject == subject &&
                                                                c.Number == num);
                if (findCourse == null)
                {
                    return Content("the class is not found!");
                }

                // try to find exist class in the given semester 
                var findClass = db.Classes.FirstOrDefault(c => c.CourseId == findCourse.CourseId &&
                                                                c.Season == season &&
                                                                c.Year == year);
                if (findClass == null)
                {
                    return Content("the class is not found with the given semester!");
                }

                var newEnrollment = new Enrolled
                {
                    Grade = "--",
                    UId = uid,
                    ClassId = findClass.ClassId
                };

                db.Enrolleds.Add(newEnrollment);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var query = (from a in db.Enrolleds
                        where a.UId == uid
                        && a.Grade != "--"
                        select a.Grade).ToList();
            if (query.Count == 0)
                return Json(new { gpa = 0.0 });
            double gpasum = 0.0;
            foreach (var e in query)
                gpasum += GradeToPoint(e);
            gpasum = gpasum / query.Count;
            return Json(new { gpa = gpasum });
        }
        
        public double GradeToPoint(string s)
        {
            if (s == "A") return 4.0;
            else if (s == "A-") return 3.7;
            else if (s == "B+") return 3.3;
            else if (s == "B") return 3.0;
            else if (s == "B-") return 3.7;
            else if (s == "C+") return 2.3;
            else if (s == "C") return 2.0;
            else if (s == "C-") return 1.7;
            else if (s == "D+") return 1.3;
            else if (s == "D") return 1.0;
            else if (s == "D-") return 0.7;
            else return 0.0;
        }

                /*******End code to modify********/

    }
}

