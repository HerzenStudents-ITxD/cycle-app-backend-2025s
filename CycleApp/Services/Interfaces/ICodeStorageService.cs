using System;

namespace CycleApp.Services.Interfaces
{
    public interface ICodeStorageService
    {
        void StoreCode(string email, string code, TimeSpan expiration);
        bool ValidateCode(string email, string code);
        void InvalidateCode(string email);
    }
}