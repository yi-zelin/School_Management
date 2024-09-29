using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from e in db.Enrolleds
                        where e.Class.Course.Depart.Subject == subject &&
                            e.Class.Course.Number == num &&
                            e.Class.Season == season &&
                            e.Class.Year == year
                        select new
                        {
                            fname = e.UIdNavigation.FName,
                            lname = e.UIdNavigation.LName,
                            uid = e.UIdNavigation.UId,
                            dob = e.UIdNavigation.Dob,
                            grade = e.Grade
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var query = from a in db.Assignments
                        where a.Category.Class.Course.Depart.Subject == subject &&
                              a.Category.Class.Course.Number == num &&
                              a.Category.Class.Season == season &&
                              a.Category.Class.Year == year &&
                              (category == null || a.Category.Name == category)
                        select new
                        {
                            aname = a.Name,
                            cname = a.Category.Name,
                            due = a.Due,
                            submissions = a.Submissions.Count()
                        };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from c in db.Categories
                        where c.Class.Course.Depart.Subject == subject &&
                              c.Class.Course.Number == num &&
                              c.Class.Season == season &&
                              c.Class.Year == year
                        select new
                        {
                            name = c.Name,
                            weight = c.Weight
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            try
            {
                // check if there is already exist a assignment category
                var findCategory = db.Categories.FirstOrDefault(c => c.Class.Course.Depart.Subject == subject &&
                                                                     c.Class.Course.Number == num &&
                                                                     c.Class.Season == season &&
                                                                     c.Class.Year == year &&
                                                                     c.Name == category &&
                                                                     c.Weight == catweight);

                // new class
                var newClass = db.Classes.FirstOrDefault(cla => cla.Course.Depart.Subject == subject &&
                                                                cla.Course.Number == num &&
                                                                cla.Season == season &&
                                                                cla.Year == year);

                if (findCategory != null)
                {
                    // there is a exist category
                    return Content("there is a exist category!");
                    //return Json(new { success = false });
                }
                else
                {
                    if (newClass == null)
                    {
                        return Content("all field can not be empty!");
                    }
                    var newCategory = new Category
                    {
                        Name = category,
                        Weight = (byte)catweight,
                        ClassId = newClass.ClassId
                    };

                    db.Categories.Add(newCategory);
                    db.SaveChanges();

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            try
            {
                // check if there is already exist a class
                var findClass = db.Classes.FirstOrDefault(c => c.Course.Depart.Subject == subject &&
                                                                c.Course.Number == num &&
                                                                c.Season == season &&
                                                                c.Year == year);
                if (findClass == null)
                {
                    return Content("the class that you enter is not exist!");
                }

                // check if there is already exist a category
                var findCategory = db.Categories.FirstOrDefault(C => C.Name == category && C.ClassId == findClass.ClassId);
                if (findCategory == null)
                {
                    return Content("the category that you enter is not exist!");
                }

                // create new assignment
                var newAssignment = new Assignment
                {
                    Name = asgname,
                    Points = (uint)asgpoints,
                    Contents = asgcontents,
                    Due = asgdue,
                    CategoryId = findCategory.CategoryId
                };

                db.Assignments.Add(newAssignment);

                // change grade
                var query = (from a in db.Assignments
                             where a.Category.Class.Year == year
                             && a.Category.Class.Season == season
                             && a.Category.Class.Course.Number == num
                             && a.Category.Class.Course.Depart.Subject == subject
                             select a.Points * a.Category.Weight).ToList();

                uint fullPoint = 0;
                foreach (var p in query)
                    fullPoint += p;

                var allID = (from e in db.Enrolleds
                            where e.Class.Course.Depart.Subject == subject &&
                                e.Class.Course.Number == num &&
                                e.Class.Season == season &&
                                e.Class.Year == year
                            select e.UId).ToList();

                foreach (string uid1 in allID)
                {
                    Debug.WriteLine(uid1);
                // get student point
                uint studentPoint = 0;
                Debug.WriteLine(uid1);
                var scoreList = from a in db.Submissions
                                where a.UId == uid1
                                && a.Score != 0
                                && a.Assignment.Category.Class.Year == year
                               && a.Assignment.Category.Class.Season == season
                               && a.Assignment.Category.Class.Course.Number == num
                               && a.Assignment.Category.Class.Course.Depart.Subject == subject
                                select new { score = a.Score, weight = a.Assignment.Category.Weight };

                Debug.WriteLine("!!!!1");

                foreach (var p in scoreList)
                {
                    Debug.WriteLine(p);
                    studentPoint += p.score * p.weight;
                }
                Debug.WriteLine("!!!!2");
                double studentGpa = 0;
                if (fullPoint != 0)
                     studentGpa = studentPoint * 4 / fullPoint;
                var user = from s in db.Enrolleds
                           where s.UId == uid1
                            && s.Class.Year == year
                            && s.Class.Season == season
                            && s.Class.Course.Number == num
                            && s.Class.Course.Depart.Subject == subject
                           select s;

                //Debug.WriteLine("!!!!3");
                //if (user.ToList().Count != 1)
                //    throw new Exception("Grade submission error");

                Debug.WriteLine("!!!!4");
                foreach (Enrolled p in user)
                    p.Grade = PointToGrade(studentGpa);

                Debug.WriteLine("changed assignment grade, current gpa is: " + studentGpa);
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
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from s in db.Submissions
                        where s.UIdNavigation.Depart.Subject == subject &&
                              s.Assignment.Category.Class.Course.Number == num &&
                              s.Assignment.Category.Class.Season == season &&
                              s.Assignment.Category.Class.Year == year &&
                              s.Assignment.Category.Name == category &&
                              s.Assignment.Name == asgname
                        select new
                        {
                            fname = s.UIdNavigation.FName,
                            lname = s.UIdNavigation.LName,
                            uid = s.UId,
                            time = s.Time,
                            score = s.Score
                        };
            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            try
            {
                // the score can not be negative
                if (score < 0)
                {
                    return Content("the score can not be negative!");
                }

                // find the submission
                var findSubmission = db.Submissions.FirstOrDefault(s => s.UIdNavigation.Depart.Subject == subject &&
                                                                   s.Assignment.Category.Class.Course.Number == num &&
                                                                   s.Assignment.Category.Class.Season == season &&
                                                                   s.Assignment.Category.Class.Year == year &&
                                                                   s.Assignment.Category.Name == category &&
                                                                   s.Assignment.Name == asgname &&
                                                                   s.UId == uid);
                if (findSubmission != null)
                {
                    findSubmission.Score = (uint)score;
                    db.SaveChanges();

                    // change grade
                    var query = (from a in db.Assignments
                                 where a.Category.Class.Year == year
                                 && a.Category.Class.Season == season
                                 && a.Category.Class.Course.Number == num
                                 && a.Category.Class.Course.Depart.Subject == subject
                                 select a.Points * a.Category.Weight).ToList();

                    uint fullPoint = 0;
                    foreach (var p in query)
                        fullPoint += p;

                    // get student point
                    uint studentPoint = 0;
                    var query1 = (from a in db.Submissions
                                  where a.UId == uid
                                  && a.Assignment.Category.Class.Year == year
                                 && a.Assignment.Category.Class.Season == season
                                 && a.Assignment.Category.Class.Course.Number == num
                                 && a.Assignment.Category.Class.Course.Depart.Subject == subject
                                  select a.Score * a.Assignment.Category.Weight).ToList();
                    foreach (var p in query1)
                        studentPoint += p;

                    double studentGpa = studentPoint * 4 / fullPoint;
                    var user = from s in db.Enrolleds
                               where s.UId == uid
                                && s.Class.Year == year
                                && s.Class.Season == season
                                && s.Class.Course.Number == num
                                && s.Class.Course.Depart.Subject == subject
                               select s;
                    if (user.ToList().Count != 1)
                        throw new Exception("Grade submission error");
                    foreach (Enrolled p in user)
                        p.Grade = PointToGrade(studentGpa);

                    Debug.WriteLine("changed assignment grade, current gpa is: " + studentGpa);
                    db.SaveChanges();


                    return Json(new { success = true });
                }
                else
                {
                    return Content("No submission founded.");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            // find the class
            var query = from c in db.Classes
                        where c.UId == uid
                        select new
                        {
                            subject = c.Course.Depart.Subject,
                            number = c.Course.Number,
                            name = c.Course.Name,
                            season = c.Season,
                            year = c.Year
                        };
            return Json(query.ToArray());
        }

        public string PointToGrade(double gradePoints)
        {
            if (gradePoints > 3.7) return "A";
            else if (gradePoints > 3.3) return "A-";
            else if (gradePoints > 3.0) return "B+";
            else if (gradePoints > 2.7) return "B";
            else if (gradePoints > 2.3) return "B-";
            else if (gradePoints > 2.0) return "C+";
            else if (gradePoints > 1.7) return "C";
            else if (gradePoints > 1.3) return "C-";
            else if (gradePoints > 1.0) return "D+";
            else if (gradePoints > 0.7) return "D";
            else if (gradePoints > 0.0) return "D-";
            else return "E";
        }


        /*******End code to modify********/
    }
}

