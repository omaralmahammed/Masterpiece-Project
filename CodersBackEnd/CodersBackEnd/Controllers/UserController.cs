using Coders.DTO;
using CodersBackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CodersBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _db;

        public UserController(MyDbContext db)
        {
            _db = db;
        }

        [HttpGet("AllUsers")]
        public IActionResult AllUsers()
        {
            var users = _db.Users.ToList();

            return Ok(users);
        }

        [HttpGet("UserDetails/{userId}")]
        public IActionResult UserDetails(int userId)
        { 
            var user = _db.Users.Find(userId);

            return Ok(user);
        }

        [HttpPut("UpdateUserDetails/{userId}")]
        public IActionResult UpdateUserDetails([FromForm] UserInformationRequestDTO editUser, int userId)
        {
            var user = _db.Users.Find(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            if (editUser.Image != null && editUser.Image.Length > 0)
            {
                var myFolder = "C:\\Users\\Orange\\Desktop\\Coders\\Images";

                var imagePath = Path.Combine(myFolder, editUser.Image.FileName);

                if (!System.IO.File.Exists(imagePath))
                {
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        editUser.Image.CopyTo(stream);
                    }
                }

                user.Image = editUser.Image.FileName ?? user.Image;
            }



            user.FirstName = editUser.FirstName ?? user.FirstName;
            user.LastName = editUser.LastName ?? user.LastName;
            user.Email = editUser.Email ?? user.Email;
            user.Gender = editUser.Gender ?? user.Gender;
            user.DateOfBirth = editUser.DateOfBirth ?? user.DateOfBirth;
            user.Country = editUser.Country ?? user.Country;
            user.City = editUser.City ?? user.City;
            user.Postcode = editUser.Postcode ?? user.Postcode;
            user.PhoneNumber = editUser.PhoneNumber ?? user.PhoneNumber;


            if (!string.IsNullOrEmpty(editUser.Password))
            {

                byte[] passwordHash, passwordSalt;
                PasswordHashDTO.CreatePasswordHash(editUser.Password, out passwordHash, out passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.Password = editUser.Password;
            }

            _db.Users.Update(user);
            _db.SaveChanges();

            return Ok(user);
        }


        [HttpPost("Register")]
        public IActionResult Register([FromForm] UserInformationRequestDTO userInfo)
        {
            var checkEmail = _db.Users.Where(e => e.Email == userInfo.Email).FirstOrDefault();

            if (checkEmail != null)
            {
                return BadRequest("Email is elready excest.");
            }


            byte[] passwordHash, passwordSalt;
            PasswordHashDTO.CreatePasswordHash(userInfo.Password, out passwordHash, out passwordSalt);

            User addUser = new User
            {
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,

                Email = userInfo.Email,

                Password = userInfo.Password,

                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,

                Image = "default.jpg"
            };

            _db.Users.Add(addUser);
            _db.SaveChanges();

            return Ok();
        }


        [HttpPost("Login")]

        public IActionResult Login([FromForm] UserInformationRequestDTO userInfo)
        {
            var user = _db.Users.FirstOrDefault(e => e.Email == userInfo.Email);

            if (user == null || !PasswordHashDTO.VerifyPasswordHash(userInfo.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = GenerateJwtToken(user);

            return Ok(new { Token = token, UserId = user.UserId });
        }




        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecureLongKeyForJWT12345"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "yourapp.com",
                audience: "yourapp.com",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);


            _db.SaveChanges();
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
        
                Random rand = new Random();
                string otp = rand.Next(100000, 1000000).ToString();

                string fromEmail = "techlearnhub.contact@gmail.com";
                string fromName = "Support Team";
                string subjectText = "Your OTP Code";
                string messageText = $@"
                                        <html>
                                        <body dir='rtl'>
                                            <h2>Hello {user.FirstName} {user.LastName}</h2>
                                            <p><strong>Your OTP code is {otp}. This code is valid for a short period of time.</strong></p>
                                            <p>If you have any questions or need additional assistance, please feel free to contact our support team.</p>
                                            <p>Best wishes,<br>Support Team</p>
                                        </body>
                                        </html>";

                try
                {
                    // Send email using MailKit
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(fromName, fromEmail));
                    message.To.Add(new MailboxAddress("", user.Email));
                    message.Subject = subjectText;
                    message.Body = new TextPart("html") { Text = messageText };

                    using (var client = new MailKit.Net.Smtp.SmtpClient())
                    {
                        await client.ConnectAsync("smtp.gmail.com", 465, true);
                        await client.AuthenticateAsync("techlearnhub.contact@gmail.com", "lyrlogeztsxclank");
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);
                    }   
                    user.Otp = otp;

                    _db.Update(user);
                    _db.SaveChanges();
                    return Ok(new { otp, user.UserId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Message = "Failed to send email. Please try again later.", Error = ex.Message });
                }
            }
            else
            {
                return NotFound(new { Message = "Email not found." });
            }

        }


        [HttpPut("ChangePassowrd")]
        public IActionResult ChangePassowrd([FromForm] UserInformationRequestDTO newPassowrd)
        {
            var user = _db.Users.FirstOrDefault(e => e.Email == newPassowrd.Email);

            if (user.Otp != newPassowrd.Otp)
            {
                return BadRequest("Invalid OTP");
            }

            byte[] passwordHash, passwordSalt;
            PasswordHashDTO.CreatePasswordHash(newPassowrd.Password, out passwordHash, out passwordSalt);

            user.Password = newPassowrd.Password;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _db.Users.Update(user);
            _db.SaveChanges();

            return Ok();
        }


        [HttpPost("AddBllingDetails/{userId}")]
        public IActionResult AddBllingDetails(int userId)
        {
            var check = _db.BillingDetails.Where(u => u.UserId == userId).FirstOrDefault();

            if (check != null) {

                return BadRequest("user has billilng details before");
            }

            BillingDetail addBillingDetails = new BillingDetail()
            {
                UserId = userId,
            };


            _db.BillingDetails.Add(addBillingDetails);
            _db.SaveChanges();

            return Ok(addBillingDetails);

        }
    }
}
