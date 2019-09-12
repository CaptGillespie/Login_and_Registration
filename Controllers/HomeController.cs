using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CBelt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CBelt.Controllers
{
    public class HomeController : Controller
    {
        private CBeltContext dbContext;
        // here we can "inject" our context service into the constructor
        public HomeController(CBeltContext context)
        {
            dbContext = context;
        }


    public IActionResult Index()
        {
            return View();
        }

    [HttpPost("/register")]
    public IActionResult register(RegisterUser userFromForm)
    {
        System.Console.WriteLine("Reached Register route!!!!!!!!! *****************************");
        // Check initial ModelState
        if(ModelState.IsValid)
        {
            System.Console.WriteLine("Model state is valid");
            if(dbContext.Logged_In_User.Any(u => u.Email == userFromForm.Email))
            {
                System.Console.WriteLine("Email is not unique");
                ModelState.AddModelError("Email", "Email already in use!");
                return View("Index");
            }
            else
            {
                System.Console.WriteLine("Everything is valid!");
                PasswordHasher<RegisterUser> Hasher = new PasswordHasher<RegisterUser>();
                userFromForm.Password = Hasher.HashPassword(userFromForm, userFromForm.Password);
                System.Console.WriteLine("Password hashed!***************************");
                dbContext.Logged_In_User.Add(userFromForm);
                System.Console.WriteLine("New User Added!**************************");
                dbContext.SaveChanges();
                System.Console.WriteLine("New User Saved!**************************");
                
                HttpContext.Session.SetInt32("LoggedID", userFromForm.UserId);
                return RedirectToAction("home");
            }
        }
        else
        {
            return View("Index");
        }
    } 

    [HttpPost("Login")]
    public IActionResult Login(LoginUser userSubmission)
    {
        System.Console.WriteLine("Reached the Login route*************");
        if(ModelState.IsValid)
        {
            System.Console.WriteLine("Model state is valid*************");
            // If inital ModelState is valid, query for a user with provided email
            var userInDb = dbContext.Logged_In_User.FirstOrDefault(u => u.Email == userSubmission.Email);
            // If no user exists with provided email
            if(userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("Email", "User error, please replace user");
                System.Console.WriteLine("user not in database... yet**************");
                return View("Index");
            }
            
            // Initialize hasher object
            var hasher = new PasswordHasher<LoginUser>();
            
            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
            System.Console.WriteLine("passwords match***************************");
            // result can be compared to 0 for failure
            if(result == 0)
            {
                ModelState.AddModelError("Password", "I even gave you a checkbox...");
                return View("Index");
            }
            HttpContext.Session.SetInt32("LoggedID", userInDb.UserId);
            HttpContext.Session.SetString("Username", userInDb.FirstName);
            int? idUser = HttpContext.Session.GetInt32("LoggedID");
            System.Console.WriteLine("Should FINALLY be in session....*************");
            return RedirectToAction("home");
        }
        return View("Index");
    }






    [HttpGet("home")]
    public IActionResult Home()
        {
            if(HttpContext.Session.GetInt32("LoggedID") != null)
                {
                List<Happening>AllHappenings = dbContext.happenings
                    .OrderByDescending(u => u.CreatedAt)
                    .Include(w => w.Attendee)
                    .ToList();
                List<Happening> MostRecent = dbContext.happenings
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToList();
                    
                    int? idUser = HttpContext.Session.GetInt32("LoggedID");
                    ViewBag.idUser = idUser;
                    ViewBag.Username = HttpContext.Session.GetString("Username");
                    System.Console.WriteLine("should be in session***********");
			        return View("home", AllHappenings);
                }
            else{
                ModelState.AddModelError("Email", "Please login to continue.");
                return Redirect("/");
                }
            
        }

    [HttpGet("create")]
    public IActionResult Create()
        {  

        if(HttpContext.Session.GetInt32("LoggedID") != null)
            {
                int? idUser = HttpContext.Session.GetInt32("LoggedID");
                ViewBag.idUser = idUser;
                return View();
            }
        else{
            ModelState.AddModelError("Email", "Please login to continue.");
            return Redirect("/");
            }
        }

    [HttpPost("new")]
    public IActionResult New(Happening newHappening)
        {
            if(newHappening.Date < DateTime.Now){
                ModelState.AddModelError("Date", "Better get a delorean!");
                return View ("create");
            }
            if (ModelState.IsValid) {
                newHappening.CreatedAt = DateTime.Now;
                newHappening.UpdatedAt = DateTime.Now;
                newHappening.RegisterUserId = (int)HttpContext.Session.GetInt32("LoggedID");
                dbContext.Add(newHappening);
                dbContext.SaveChanges();
                return RedirectToAction("home");
            }
            else
            {
                return View ("create");
            }
        }

    [HttpGet("details/{happeningsID}")]
    public IActionResult Details(int happeningsID)
    {
        if(HttpContext.Session.GetInt32("LoggedID") != null)
            {
                int? idUser = HttpContext.Session.GetInt32("LoggedID");
                ViewBag.idUser = idUser;
                ViewBag.Username = HttpContext.Session.GetString("Username");
                Happening aHappening = dbContext.happenings
                    .Where(w => w.HappeningId == happeningsID)
                    .Include(w => w.Attendee)
                    .ThenInclude(r => r.Creator)
                    .SingleOrDefault();
                return View(aHappening);
            }
            ModelState.AddModelError("Email", "Please login to continue.");
            return View("details");
    }

    [HttpGet("delete/{happeningsID}")]
        public IActionResult Delete(int happeningsID)
        {
            if(HttpContext.Session.GetInt32("LoggedID") != null)
            {
                int? idUser = HttpContext.Session.GetInt32("LoggedID");
                dbContext.Remove(dbContext.happenings
                .Where(w => w.HappeningId == happeningsID)
                .SingleOrDefault());
                dbContext.SaveChanges();
                return RedirectToAction("Home");
            }
            ModelState.AddModelError("Email", "Please login to continue.");
            return View("Index");
        }


    [HttpGet("rsvp/{happeningsID}")]
        public IActionResult AddGuest(int happeningsID)
        {
            if(HttpContext.Session.GetInt32("LoggedID") != null)
            {
                int? idUser = HttpContext.Session.GetInt32("LoggedID");
                Association newAssociation = new Association();
                newAssociation.RegisterUserId = (int)idUser;
                newAssociation.HappeningId = happeningsID;
                dbContext.Add(newAssociation);
                dbContext.SaveChanges();
                return RedirectToAction("Home");
            }
            ModelState.AddModelError("Email", "Please login to continue.");
            return View("Index");
        }

        [HttpGet("unrsvp/{happeningsID}")]
        public IActionResult RemoveAttendee(int happeningsID)
        {
            if(HttpContext.Session.GetInt32("LoggedID") != null)
            {
                int? idUser = HttpContext.Session.GetInt32("LoggedID");
                dbContext.Remove(dbContext.Attendee
                    .Where(r => r.HappeningId == happeningsID)
                    .Where(r => r.RegisterUserId == (int)idUser)
                    .SingleOrDefault());
                dbContext.SaveChanges();
                return RedirectToAction("Home");
            }
            ModelState.AddModelError("Email", "Please login to continue.");
            return View("Index");
        }




    [HttpGet("logout")]
    public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/");
        }

        
    }
}
