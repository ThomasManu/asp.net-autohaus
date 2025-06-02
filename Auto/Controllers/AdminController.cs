using Auto.Data;
using Auto.Models;
using Auto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auto.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserService _userService;

        private readonly ApplicationDbContext _context;
        public AdminController(UserService userservice, ApplicationDbContext context)
        {
            _context = context;
            _userService = userservice;
        }
        #region Startseite mit allen usern
        public IActionResult AdminIndex()
        {
            var usersWithRoles = from user in _context.Users
                                 join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                 join role in _context.Roles on userRole.RoleId equals role.Id
                                 select new UserRoleViewModel
                                 {
                                     UserId = user.Id,                 // UserId abfragen
                                     UserName = user.UserName,
                                     Email = user.Email,
                                     RoleName = role.Name,
                                     EmailConfirmed = user.EmailConfirmed  // EmailConfirmed abfragen
                                 };

            return View(usersWithRoles.ToList());
        }
        #endregion

        #region neuenUserErstellen/updaten

        public IActionResult neuenUserErstellen(string id)
        {
            ViewBag.neuerstellung = "neuen User erstellen";
            ViewBag.senden = "User hochladen";
            var model = new UserRoleViewModel();
            if (id != null)
            {
                var usersWithRoles = (from user in _context.Users
                                      join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                      join role in _context.Roles on userRole.RoleId equals role.Id
                                      where user.Id == id.ToString() // Filtern nach der Id
                                      select new UserRoleViewModel
                                      {
                                          UserId = user.Id,
                                          UserName = user.UserName,
                                          Email = user.Email,
                                          RoleName = role.Name,
                                          EmailConfirmed = user.EmailConfirmed
                                      }).SingleOrDefault(); // SingleOrDefault, um nur einen Benutzer zu erhalten

                if (usersWithRoles != null)
                {
                    ViewBag.neuerstellung = "User bearbeiten";
                    ViewBag.senden = "User updaten";
                    model = usersWithRoles;
                    model.passwort = "noChangePossible";
                }
                else
                {
                    // Benutzer mit dieser ID wurde nicht gefunden
                    return NotFound("User with the specified ID was not found.");
                }
            }
            ViewBag.Roles = _context.Roles.ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> neuenUserErstellen(UserRoleViewModel Uvm)
        {
            if (Uvm == null)


            {
                return NotFound("Es wurde kein Model übergeben");
            }
            if (Uvm.UserId == null)
            {
                await _userService.CreateUser(Uvm.RoleName, Uvm.Email, Uvm.passwort, Uvm.EmailConfirmed);

            }
            else
            {
                await _userService.UpdateUser(Uvm.AlteEmail, Uvm.Email, Uvm.Email, Uvm.RoleName, Uvm.EmailConfirmed);
            }

            return RedirectToAction("AdminIndex");
        }
        #endregion

        #region user löschen
        public async Task<IActionResult> del(string email)
        {
            await _userService.DeleteUser(email);

            return RedirectToAction("AdminIndex");
        }

        #endregion
    }
}

