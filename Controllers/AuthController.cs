using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BC = BCrypt.Net.BCrypt;

using moah_api.Models;

namespace moah_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMongoCollection<User> _usersCollection;
    public AuthController(IMongoCollection<User> usersCollection)
    {
        _usersCollection = usersCollection;
    }

    [HttpPost("signup", Name = "SignupRoute")]
    public ActionResult Signup(User newUser)
    {
        newUser.Password = BC.HashPassword(newUser.Password);
        _usersCollection.InsertOne(newUser);
        Console.WriteLine(newUser.Id);
        return Ok($"User's ID is {newUser.Id}");
    }
}