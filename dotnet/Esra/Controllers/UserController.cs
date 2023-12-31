﻿using Microsoft.AspNetCore.Mvc;
using EsraApi.DAO;
using EsraApi.Models;
using EsraApi.Exceptions;
using EsraApi.Security;
using Microsoft.AspNetCore.Identity;

namespace EsraApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ITokenGenerator tokenGenerator;
        private readonly IPasswordHasher passwordHasher;
        private readonly IUserDao userDao;

        public UserController(ITokenGenerator tokenGenerator, IPasswordHasher passwordHasher, IUserDao userDao)
        {
            this.tokenGenerator = tokenGenerator;
            this.passwordHasher = passwordHasher;
            this.userDao = userDao;
        }

        [HttpGet("/")]
        public ActionResult<string> Ready()
        {
            int userCount = userDao.GetUsers().Count;
            return Ok($"Server is ready with {userCount} user(s).");
        }

        [HttpGet("/whoami")]
        public ActionResult<string> WhoAmI()
        {
            string result = User.Identity.Name;

            if (result == null)
            {
                return "No token provided.";
            }
            else
            {
                return result;
            }
        }

        [HttpPost("/login")]
        public IActionResult Authenticate(LoginUser userParam)
        {
            // Default to bad username/password message
            IActionResult result = Unauthorized(new { message = "Username or password is incorrect." });

            User user;
            // Get the user by username
            try
            {
                user = userDao.GetUserByUsername(userParam.Username);
            }
            catch (DaoException)
            {
                // return default Unauthorized message instead of indicating a specific error
                return result;
            }

            // If we found a user and the password hash matches
            if (user != null && passwordHasher.VerifyHashMatch(user.PasswordHash, userParam.Password, user.Salt))
            {
                // Create an authentication token
                string token = tokenGenerator.GenerateToken(user.UserId, user.Username, user.Role);

                // Create a ReturnUser object to return to the client
                LoginResponse retUser = new LoginResponse() { User = new ReturnUser() { UserId = user.UserId, Username = user.Username, Role = user.Role }, Token = token };

                // Switch to 200 OK
                result = Ok(retUser);
            }

            return result;
        }

        [HttpPost("/register")]
        public IActionResult Register(RegisterUser userParam)
        {
            // Default generic error message
            const string ErrorMessage = "An error occurred and user was not created.";

            IActionResult result = BadRequest(new { message = ErrorMessage });

            // is username already taken?
            try
            {
                User existingUser = userDao.GetUserByUsername(userParam.Username);
                if (existingUser != null)
                {
                    return Conflict(new { message = "Username already taken. Please choose a different username." });
                }
            }
            catch (DaoException)
            {
                return StatusCode(500, ErrorMessage);
            }

            // create new user
            User newUser;
            try
            {
                newUser = userDao.CreateUser(userParam.Username, userParam.Password, userParam.Role);
            }
            catch (DaoException)
            {
                return StatusCode(500, ErrorMessage);
            }

            if (newUser != null)
            {
                // Create a ReturnUser object to return to the client
                ReturnUser returnUser = new ReturnUser() { UserId = newUser.UserId, Username = newUser.Username, Role = newUser.Role };

                result = Created("/login", returnUser);
            }

            return result;
        }
    }

}
