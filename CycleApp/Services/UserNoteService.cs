using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CycleApp.Services
{
    public class UserNoteService : IUserNoteService
    {
        private readonly CycleDbContext _dbContext;
        private readonly ILogger<UserNoteService> _logger;

        public UserNoteService(CycleDbContext dbContext, ILogger<UserNoteService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<Entry>> GetUserNotes(int userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _dbContext.Entries.Where(e => e.UserId == userId);
            
            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate);
                
            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate);
                
            return await query.OrderByDescending(e => e.Date).ToListAsync();
        }

        public async Task<Entry> AddUserNote(Entry entry)
        {
            _dbContext.Entries.Add(entry);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Added new entry for user {UserId} on date {Date}", entry.UserId, entry.Date);
            return entry;
        }

        public async Task<Entry> UpdateUserNote(Entry entry)
        {
            var existingEntry = await _dbContext.Entries
                .FirstOrDefaultAsync(e => e.EntryId == entry.EntryId && e.UserId == entry.UserId);
                
            if (existingEntry == null)
            {
                _logger.LogWarning("Could not find entry {EntryId} for user {UserId}", entry.EntryId, entry.UserId);
                return null;
            }
                
            existingEntry.Mood = entry.Mood;
            existingEntry.Pain = entry.Pain;
            existingEntry.Flow = entry.Flow;
            existingEntry.Symptoms = entry.Symptoms;
            existingEntry.Notes = entry.Notes;
            
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated entry {EntryId} for user {UserId}", entry.EntryId, entry.UserId);
            return existingEntry;
        }

        public async Task<bool> DeleteUserNote(int entryId, int userId)
        {
            var entry = await _dbContext.Entries
                .FirstOrDefaultAsync(e => e.EntryId == entryId && e.UserId == userId);
                
            if (entry == null)
            {
                _logger.LogWarning("Could not find entry {EntryId} for user {UserId} to delete", entryId, userId);
                return false;
            }
                
            _dbContext.Entries.Remove(entry);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Deleted entry {EntryId} for user {UserId}", entryId, userId);
            return true;
        }
    }
}
