using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        // Inject our data context and token service.
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        // HTTP POST
        // Register user using registerDto object as a parameter.
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){
            
            // Check if username already exists. (BadRequest returns a 400 Http Status Bad Request)
            if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");
            // Make use of using in order to let HMAC class correctly dispose itself.
            using var hmac = new HMACSHA512();

            // Create user and assign it's values.
            // Encode into a byte array the user password and proceed to compute hash using ComputeHash().
            // Save salted key generated randomly by HMACSHA512().
            var user = new AppUser(){
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
            // Add our user by tracking it in EF.
            _context.Users.Add(user);
            // Save user and return our UserDto.
            await _context.SaveChangesAsync();

            return new UserDto{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }
        // HTTP POST
        // Login user using loginDto object as a parameter.
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            // Get from our database our username with SingleOrDefaultAsync() which actually throws an exception if there is more than one username.
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username);
            // In case we didn't find it return an Unauthorized 401.
            if(user == null) return Unauthorized("Invalid username");
            // Pass as a parameter our user.PasswordSalt in order to calculate it correctly using the key.
            using var hmac = new HMACSHA512(user.PasswordSalt);
            // Get our hash.
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            // Check char by char if password is right.
            for(int i = 0; i < computedHash.Length; i++){
                if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }
            // Return our UserDto.
            return new UserDto{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }
        // Check if a username is already taken, in case it is return true;
        private async Task<bool> UserExists(string username){
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}