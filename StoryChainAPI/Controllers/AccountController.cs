﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StoryChainAPI.Configuration;
using StoryChainAPI.DTO.Requests;
using StoryChainAPI.DTO.Results;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StoryChainAPI.Controllers
{
    [Route("api/v1/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public AccountController(UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest user)
        {
            // Check if the incoming request is valid
            if (ModelState.IsValid)
            {
                // Check if the user with the same email exist
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    return BadRequest(new RegistrationResult()
                    {
                        Result = false,
                        Errors = new List<string>(){
                                            "Email already exists"
                                        }
                    });
                }

                var newUser = new IdentityUser() { Email = user.Email, UserName = user.Email };
                var isCreated = await _userManager.CreateAsync(newUser, user.Password);
                if (isCreated.Succeeded)
                {
                    var jwtToken = GenerateJwtToken(newUser);

                    return Ok(new RegistrationResult()
                    {
                        Result = true,
                        Token = jwtToken
                    });
                }

                return new JsonResult(new RegistrationResult()
                {
                    Result = false,
                    Errors = isCreated.Errors.Select(x => x.Description).ToList()
                }
                        )
                { StatusCode = 500 };
            }

            return BadRequest(new RegistrationResult()
            {
                Result = false,
                Errors = new List<string>(){
                                            "Invalid payload"
                                        }
            });
        }

        /// <summary>
        /// Signs a user in and returns a jwt
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest user)
        {
            if (ModelState.IsValid)
            {
                // check if the user with the same email exist
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if (existingUser == null)
                {
                    // We dont want to give to much information on why the request has failed for security reasons
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>(){
                                        "Uh oh... those credentials didn't work! Please try again or reset your password."
                                    }
                    });
                }

                // Now we need to check if the user has inputed the right password
                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);

                if (isCorrect)
                {
                    var jwtToken = GenerateJwtToken(existingUser);

                    return Ok(new AuthResult()
                    {
                        Result = true,
                        Token = jwtToken
                    });
                }
                else
                {
                    // We dont want to give to much information on why the request has failed for security reasons
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>(){
                                         "Invalid authentication request"
                                    }
                    });
                }
            }

            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = new List<string>(){
                                        "Invalid payload"
                                    }
            });
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            // Now its ime to define the jwt token which will be responsible of creating our tokens
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            // We get our secret from the appsettings
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            // we define our token descriptor
            // We need to utilise claims which are properties in our token which gives information about the token
            // which belong to the specific user who it belongs to
            // so it could contain their id, name, email the good part is that these information
            // are generated by our server and identity framework which is valid and trusted
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    // the JTI is used for our refresh token which we will be convering in the next video
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                // the life span of the token needs to be shorter and utilise refresh token to keep the user signedin
                // but since this is a demo app we can extend it to fit our current need
                Expires = DateTime.UtcNow.AddHours(6),
                // here we are adding the encryption alogorithim information which will be used to decrypt our token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}