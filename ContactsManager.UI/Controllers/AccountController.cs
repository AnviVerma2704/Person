using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.DTO;
using ContactsManager.Core.Enums;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            //Check for validation errors
            if (ModelState.IsValid == false)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
                return View(registerDTO);
            }

            ApplicationUser user = new ApplicationUser() { Email = registerDTO.Email, PhoneNumber = registerDTO.Phone, UserName = registerDTO.Email, PersonName = registerDTO.PersonName, password = registerDTO.Password, confirmPassword = registerDTO.ConfirmPassword};

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (result.Succeeded)
            {
                //ApplicationUser user = await _userManager.FindByEmailAsync(registerDTO.Email);
                //Check status of radio button
                if (registerDTO.userType == Core.Enums.UserTypeOptions.Admin)
                {
                    //Create 'Admin' role
                    if (await _roleManager.FindByNameAsync(UserTypeOptions.Admin.ToString()) is null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole() { Name = UserTypeOptions.Admin.ToString() };
                        await _roleManager.CreateAsync(applicationRole);
                    }

                    //Add the new user into 'Admin' role
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.Admin.ToString());
                }
                else
                {
                    //Add the new user into 'User' role
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());
                }
                //Sign in
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }

                return View(registerDTO);
            }

        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginDTO loginDTO, string ReturnUrl = "persons/index")
        {
            if (ModelState.IsValid == false)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
                return View(loginDTO);
            }
            var result = _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Result.Succeeded)
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(loginDTO.Email);
                if (user != null) 
                { 
                    if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Admin.ToString()))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin"});
                    }
                }
                return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }
            else
            {
                ModelState.AddModelError("Login", "Invalid login attempt");
                return View(loginDTO);
            }
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(PersonsController.Index), "Persons");
        }
    }
}