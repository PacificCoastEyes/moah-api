using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BC = BCrypt.Net.BCrypt;
using moah_api.Models;
using moah_api.Utilities;

namespace moah_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly TokenSigner _tokenSigner;
    public readonly ILogger<AuthController> _logger;
    public AuthController(IMongoCollection<User> usersCollection, TokenSigner tokenSigner, ILogger<AuthController> logger)
    {
        _usersCollection = usersCollection;
        _tokenSigner = tokenSigner;
        _logger = logger;
    }

    [HttpPost("signup", Name = "SignupRoute")]
    async public Task<ActionResult> Signup(User newUser)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq("email", newUser.Email);
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            if (user is not null)
            {
                return BadRequest("Hey you! Welcome back! Please log in instead.");
            }
            else
            {
                newUser.Password = BC.HashPassword(newUser.Password);
                await _usersCollection.InsertOneAsync(newUser);
                string authToken = _tokenSigner.SignToken(newUser);
                return Ok(new { AuthToken = authToken, newUser.FirstName });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Signup error");
            return StatusCode(500, "Sorry, there was an error signing you up, please try again later.");
        }
    }

    [HttpPost("login", Name = "LoginRoute")]
    async public Task<ActionResult> Login(User user)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq("email", user.Email);
            var existingUser = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            if (existingUser is not null && BC.Verify(user.Password, existingUser.Password))
            {
                string authToken = _tokenSigner.SignToken(existingUser);
                return Ok(new { AuthToken = authToken, existingUser.FirstName });
            }
            else
            {
                return BadRequest("Oops! The email and password you entered do not match.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Login error");
            return StatusCode(500, "Sorry, there was an error logging you in, please try again later.");
        }
    }
}