using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StoryChainAPI.Configuration;
using StoryChainAPI.Data;
using StoryChainAPI.Data.Models;
using StoryChainAPI.DTO.Requests;
using StoryChainAPI.DTO.Results;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace StoryChainAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly JwtConfig _jwtConfig;
        // Options for the website client
        private readonly WebsiteConfig _websiteConfig;

        public AccountController(UserManager<ApplicationUser> userManager, ApplicationDbContext db, TokenValidationParameters tokenValidationParameters, IOptionsMonitor<JwtConfig> jwtConfig, IOptionsMonitor<WebsiteConfig> websiteConfig)
        {
            _userManager = userManager;
            _db = db;
            _tokenValidationParameters = tokenValidationParameters;
            _jwtConfig = jwtConfig.CurrentValue;
            _websiteConfig = websiteConfig.CurrentValue;
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sign-up", Name = "SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            // Check if the incoming request is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the user with the same email exist
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email already exists");
                return BadRequest(ModelState);
            }

            // Create the user
            var user = new ApplicationUser() { Email = request.Email, UserName = request.Email };
            var isCreated = await _userManager.CreateAsync(user, request.Password);
            if (isCreated.Succeeded)
            {
                var jwtToken = await GenerateJwtToken(user);

                return Ok(jwtToken);
            }
            else
            {

                // Relay the errors to the client
                isCreated.Errors.Select(x => x.Description).ToList().ForEach(errorDesc =>
                {
                    ModelState.AddModelError("", errorDesc);

                });

                return BadRequest(ModelState);
            }

        }

        /// <summary>
        /// Signs a user in and returns a jwt
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the user with the same email exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                // Now we need to check if the user has entered the right password
                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, request.Password);
                if (isCorrect)
                {
                    var jwtToken = await GenerateJwtToken(existingUser);

                    return Ok(jwtToken);
                }
            }

            // We dont want to give to much information on why the request has failed for security reasons
            ModelState.AddModelError("", "Your credentials didn't work.");
            return BadRequest(ModelState);

        }

        /// <summary>
        /// Sends the forgot password email
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("forgot-password", Name = "ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                // Request not valid
                return BadRequest();
            }

            // Get the user from the email address
            var user = await _userManager.FindByNameAsync(request.Email);
            //if (user != null && !user.EmailConfirmed)
            //{
            //    // User is not verified, resend verification email
            //    await SendEmailConfirmationLink(user);
            //    return Ok();
            //}
            if (user == null)
            {
                // Don't send anything, the user does not exist
                return Ok();
            }

            // Send an email with this link
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"{_websiteConfig.Host}/change-password/{user.Id}/{WebUtility.UrlEncode(code)}";

            // Send the email
            await SendEmailAsync(user, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

            return Ok();

        }

        [HttpPost]
        [Route("change-password", Name = "ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                // Request not valid
                return BadRequest(ModelState);
            }

            // Get the user from the email address
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "Cannot change password.");
                // Don't send anything, the user does not exist
                return BadRequest(ModelState);
            }

            // Change the password
            var result = await _userManager.ResetPasswordAsync(user, request.Code, request.Password);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                // Relay errors to client
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return BadRequest(ModelState);
            }

        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await VerifyToken(request);
            if (res != null)
            {
                return Ok(res);
            }
            else
            {
                ModelState.AddModelError("", "The supplied tokens are invalid.");
                return BadRequest(ModelState);
            }

        }

        #region Helpers
        private async Task SendConfirmEmail(ApplicationUser user)
        {
            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = $"{_websiteConfig.Host}{Url.RouteUrl("ConfirmEmail", new { user.Id, code })}";

            // Send the email
            await SendEmailAsync(user, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

        }

        private async Task SendEmailAsync(ApplicationUser user, string subject, string body)
        {
            using (var smtp = new SmtpClient())
            {
                smtp.Host = "127.0.0.1";
                smtp.Port = 25;

                var msg = new MailMessage
                {
                    IsBodyHtml = true,
                    From = new MailAddress("support@plotmash.com", "Plot Mash Support"),
                    Subject = subject,
                    Body = body
                };

                msg.To.Add(new MailAddress(user.Email));

                await smtp.SendMailAsync(msg);
                return;
            }
        }

        private async Task<AuthResult> GenerateJwtToken(ApplicationUser user)
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
                Expires = DateTime.UtcNow.AddMinutes(5),
                // here we are adding the encryption alogorithim information which will be used to decrypt our token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            // Create a refresh token
            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                User = user,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                IsRevoked = false,
                Token = GenerateRandomString(25) + Guid.NewGuid()
            };

            using (_db)
            {
                _db.RefreshTokens.Add(refreshToken);
                await _db.SaveChangesAsync();
            }

            return new AuthResult()
            {
                Success = true,
                Token = jwtToken,
                RefreshToken = refreshToken.Token
            };
        }

        private async Task<AuthResult> VerifyToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // this will validate the 3rd part of the jwt token using the secret that we added in the appsettings and verify we have generated the jwt token
                    IssuerSigningKey = _tokenValidationParameters.IssuerSigningKey, // Add the secret key to our Jwt encryption
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = false,
                    ValidateLifetime = false
                };

                // This validation function will make sure that the token meets the validation parameters
                // and its an actual jwt token not just a random string
                var principal = jwtTokenHandler.ValidateToken(tokenRequest.Token, tokenValidationParameters, out var validatedToken);

                // Now we need to check if the token has a valid security algorithm
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    if (result == false)
                    {
                        return null;
                    }
                }

                // Will get the time stamp in unix time
                var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                // we convert the expiry date from seconds to the date
                var expDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expDate > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "We cannot refresh this since the token has not expired" },
                        Success = false
                    };
                }

                // Check the token we got if its saved in the db
                var storedRefreshToken = _db.RefreshTokens.Include(x => x.User).Where(x => x.Token == tokenRequest.RefreshToken).FirstOrDefault();
                if (storedRefreshToken == null)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "Refresh token doesnt exist" },
                        Success = false
                    };
                }

                // Check the date of the saved token if it has expired
                if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "Token has expired, user needs to relogin" },
                        Success = false
                    };
                }

                // check if the refresh token has been used
                if (storedRefreshToken.IsUsed)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "Token has been used" },
                        Success = false
                    };
                }

                // Check if the token is revoked
                if (storedRefreshToken.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "Token has been revoked" },
                        Success = false
                    };
                }

                // we are getting here the jwt token id
                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                // check the id that the recieved token has against the id saved in the db
                if (storedRefreshToken.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "the token doenst mateched the saved token" },
                        Success = false
                    };
                }

                storedRefreshToken.IsUsed = true;
                _db.RefreshTokens.Update(storedRefreshToken);
                await _db.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedRefreshToken.User.Id);
                return await GenerateJwtToken(dbUser);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        private string GenerateRandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion
    }

}