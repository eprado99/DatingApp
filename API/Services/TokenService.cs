using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        // Make use of SymmetricSecurityKey (Only one key to encrypt and decrypt).
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            // Get our key from our config file.
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public string CreateToken(AppUser user)
        {
            // Add our claims (attributes useful in context of authentication and authorization operations).
            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.NameId, user.UserName)
            };
            // Get our signing credentials by passing a key and using strongest encrypting algorithm.
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            // Create our description of security token, add subject, expiring time and signing credentials.
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };
            // Create our security token handler.
            var tokenHandler = new JwtSecurityTokenHandler();
            // Create our token.
            var token = tokenHandler.CreateToken(tokenDescriptor);
            // Return written token.
            return tokenHandler.WriteToken(token);
        }
    }
}