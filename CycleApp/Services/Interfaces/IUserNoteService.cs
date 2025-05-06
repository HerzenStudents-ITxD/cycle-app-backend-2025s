using CycleApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CycleApp.Services.Interfaces
{
    public interface IUserNoteService
    {
        Task<List<Entry>> GetUserNotes(int userId, DateTime? startDate, DateTime? endDate);
        Task<Entry> AddUserNote(Entry entry);
        Task<Entry> UpdateUserNote(Entry entry);
        Task<bool> DeleteUserNote(int entryId, int userId);
    }
}
