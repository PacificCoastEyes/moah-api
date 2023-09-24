using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using moah_api.Utilities;
using MongoDB.Bson;
using moah_api.Models;
using MongoDB.Driver;

namespace moah_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JournalController : ControllerBase
    {
        private readonly IMongoCollection<JournalEntry> _journalEntriesCollection;
        private readonly TokenDecryptor _tokenDecryptor;
        private readonly ILogger<JournalController> _logger;
        public JournalController(IMongoCollection<JournalEntry> journalEntriesCollection, TokenDecryptor tokenDecryptor, ILogger<JournalController> logger)
        {
            _journalEntriesCollection = journalEntriesCollection;
            _tokenDecryptor = tokenDecryptor;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("initialize-entry", Name = "InitializeEntryRoute")]
        async public Task<ActionResult> InitializeEntry()
        {
            try
            {
                string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                string userId = _tokenDecryptor.DecryptToken(token).FindFirstValue("id")!;
                JournalEntry newEntry = new() { UserId = ObjectId.Parse(userId) };
                await _journalEntriesCollection.InsertOneAsync(newEntry);
                return Ok(new { id = newEntry.Id.ToString(), metadata = newEntry.Id });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Initialize journal entry error");
                return StatusCode(500, "Sorry, there was an error logging you in, please try again later.");
            }
        }
    }
}