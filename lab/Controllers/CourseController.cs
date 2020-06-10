using lab.Models;
using lab.ViewModel;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.Owin.Security.Provider;

namespace lab.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        // GET: BigSchool
        public CourseController()
        {
            _dbContext = new ApplicationDbContext();
        }
        public ActionResult Home()
        {
            var upcomming = _dbContext.Courses
                .Include(c => c.Lecture)
                .Include(c=> c.Category)
                
                .Where(c => c.DateTime > DateTime.Now);
          
            var viewModel = new CoursesViewMode
            {
                UpcommingCourses = upcomming,
                ShowAction = User.Identity.IsAuthenticated,
              

            };
            return View(viewModel);
        }
        [Authorize]
        public ActionResult Create()
        {

            var viewModel = new CourseViewModel
            {
                Categories = _dbContext.Categories.ToList()
            };
            return View(viewModel);


        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CourseViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Categories = _dbContext.Categories.ToList();
                return View(viewModel);
            }
            var course = new Course
            {
                CategoryId = viewModel.Category,
                DateTime = viewModel.GetDateTime(),
                LectureId = User.Identity.GetUserId(),
                Place = viewModel.Place
            };
            _dbContext.Courses.Add(course);
            _dbContext.SaveChanges();
            return RedirectToAction("Home", "Course");
        }
        [Authorize]
        public ActionResult Attending()
        {
            var userId = User.Identity.GetUserId();

            var course = _dbContext.Attendances
                .Where(a => a.AttendeeId == userId)
                .Select(a => a.Course)
                .Include(l => l.Lecture)
                .Include(l => l.Category)
                .ToList();
            var viewModel = new CoursesViewMode
            {
                UpcommingCourses = course,
                ShowAction = User.Identity.IsAuthenticated
            };
            return View(viewModel);
        }
        [Authorize]
        public ActionResult Following()
        {
            var userId = User.Identity.GetUserId();
            var query = from a in _dbContext.Users
                        join b in _dbContext.Followings on a.Id equals b.FollowerId
                        where b.FolloweeId == userId
                        select a;
            return View(query);
                
        }
        [Authorize]
        public ActionResult MineCourse()
        {
            var userId = User.Identity.GetUserId();
            var course = _dbContext.Courses
                .Where(c => c.LectureId == userId && c.DateTime > DateTime.Now)
                .Include(l => l.Lecture)
                .Include(c => c.Category)
                .ToList();

            return View(course);
        }
        [Authorize]
        public ActionResult EditCourse (int? id)
        {
            var userId = User.Identity.GetUserId();
            var course = _dbContext.Courses.FirstOrDefault(p => p.Id == id && p.LectureId == userId);

            var viewModel = new CourseViewModel
            {
                Categories = _dbContext.Categories.ToList(),
                Date = course.DateTime.ToString("dd/MM/yyyy"),
                Place = course.Place,
                Time = course.DateTime.ToString("HH:mm"),
                Category = course.CategoryId
            };
            return View(viewModel);
        }
        
    }
}