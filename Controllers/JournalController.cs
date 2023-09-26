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
        [HttpPost("initialize-entry", Name = "InitializeEntryRoute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                return StatusCode(500, "Oops! there was an error getting your journal entry started, please try again later.");
            }
        }

        [Authorize]
        [HttpGet("get-all-entries", Name = "GetAllEntriesRoute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        async public Task<ActionResult> GetAlllEntries()
        {
            try
            {
                string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                string userId = _tokenDecryptor.DecryptToken(token).FindFirstValue("id")!;

                FilterDefinitionBuilder<JournalEntry> filterBuilder = Builders<JournalEntry>.Filter;
                FilterDefinition<JournalEntry> filter = filterBuilder.Eq(document => document.UserId, ObjectId.Parse(userId));

                SortDefinitionBuilder<JournalEntry> sortBuilder = Builders<JournalEntry>.Sort;
                SortDefinition<JournalEntry> sortByNewest = sortBuilder.Descending(entry => entry.Id);

                ProjectionDefinitionBuilder<JournalEntry> projectionBuilder = Builders<JournalEntry>.Projection;
                ProjectionDefinition<JournalEntry> projection = projectionBuilder.Exclude(entry => entry.Content).Exclude(entry => entry.UserId); // Prevents full content of large journal entries from being sent to the client

                List<JournalEntry> rawEntries = await _journalEntriesCollection.Find(filter).Project<JournalEntry>(projection).Sort(sortByNewest).ToListAsync();
                if (rawEntries.Count == 0) throw new NullReferenceException();

                List<OutgoingArchiveEntry> formattedEntries = new() { };
                foreach (JournalEntry entry in rawEntries)
                {
                    formattedEntries.Add(new OutgoingArchiveEntry() { Id = entry.Id.ToString(), Created_at = entry.Id.CreationTime, Snippet = entry.Snippet });
                }

                return Ok(formattedEntries);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get all journal entries error");
                return e is NullReferenceException ? StatusCode(400, "Looks like you haven't written a journal entry yet!") : StatusCode(500, "Oops! there was an error loading your journal entries, please try again later.");
            }
        }

        [Authorize]
        [HttpGet("get-entry-by-id", Name = "GetEntryByIdRoute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        async public Task<ActionResult> GetEntryById(string id)
        {
            try
            {
                string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                string userId = _tokenDecryptor.DecryptToken(token).FindFirstValue("id")!;
                FilterDefinitionBuilder<JournalEntry> builder = Builders<JournalEntry>.Filter;
                FilterDefinition<JournalEntry> filter = builder.And(builder.Eq(document => document.UserId, ObjectId.Parse(userId)), builder.Eq(document => document.Id, ObjectId.Parse(id)));
                JournalEntry entry = await _journalEntriesCollection.Find(filter).FirstOrDefaultAsync();
                return Ok(new { id = entry.Id.ToString(), metadata = entry.Id, content = entry.Content, snippet = entry.Snippet });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get journal entry by ID error");
                return e is NullReferenceException ? StatusCode(400, "Hold up! That journal entry doesn't exist.") : StatusCode(500, "Oops! there was an error loading your journal entry, please try again later.");
            }
        }

        [Authorize]
        [HttpPatch("save-entry", Name = "SaveEntryRoute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        async public Task<ActionResult> SaveEntry(IncomingUpdatedEntry updatedEntry)
        {
            try
            {
                string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                string userId = _tokenDecryptor.DecryptToken(token).FindFirstValue("id")!;

                FilterDefinitionBuilder<JournalEntry> builder = Builders<JournalEntry>.Filter;
                FilterDefinition<JournalEntry> filter = builder.And(builder.Eq(document => document.UserId, ObjectId.Parse(userId)), builder.Eq(document => document.Id, ObjectId.Parse(updatedEntry.Id)));
                UpdateDefinition<JournalEntry> update = Builders<JournalEntry>.Update.Set(document => document.Content, updatedEntry.Content).Set(document => document.Snippet, updatedEntry.Snippet);
                await _journalEntriesCollection.UpdateOneAsync(filter, update);

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Save journal entry error");
                return StatusCode(500, "Oops! There was an error saving your journal entry, please try again later.");
            }
        }

        [Authorize]
        [HttpDelete("delete-entry-by-id", Name = "DeleteEntryByIdRoute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        async public Task<ActionResult> DeleteEntryById(string id)
        {
            try
            {
                string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                string userId = _tokenDecryptor.DecryptToken(token).FindFirstValue("id")!;
                FilterDefinitionBuilder<JournalEntry> builder = Builders<JournalEntry>.Filter;
                FilterDefinition<JournalEntry> filter = builder.And(builder.Eq(document => document.UserId, ObjectId.Parse(userId)), builder.Eq(document => document.Id, ObjectId.Parse(id)));
                await _journalEntriesCollection.DeleteOneAsync(filter);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get journal entry by ID error");
                return StatusCode(500, "Oops! There was an error deleting your journal entry, please try again later.");
            }
        }
    }
}