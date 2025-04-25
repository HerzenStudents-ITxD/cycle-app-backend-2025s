using System;
using System.Collections.Concurrent;
using CycleApp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CycleApp.Services
{
    public class CodeStorageService : ICodeStorageService
    {
        private static readonly ConcurrentDictionary<string, TempCode> _codes = new();
        private readonly ILogger<CodeStorageService> _logger;

        public CodeStorageService(ILogger<CodeStorageService> logger)
        {
            _logger = logger;
        }

        public void StoreCode(string email, string code, TimeSpan expiration)
        {
            var lowerEmail = email.ToLowerInvariant();
            _codes[lowerEmail] = new TempCode
            {
                Email = lowerEmail,
                Code = code,
                Expiration = DateTime.UtcNow.Add(expiration),
                IsUsed = false
            };
            _logger.LogInformation($"Код сохранён: {code} для {email} (действителен до {_codes[lowerEmail].Expiration})");
        }

        public bool ValidateCode(string email, string code)
        {
            var lowerEmail = email.ToLowerInvariant();

            if (!_codes.TryGetValue(lowerEmail, out var tempCode))
            {
                _logger.LogWarning($"Код не найден для {email}");
                return false;
            }

            if (tempCode.IsUsed)
            {
                _logger.LogWarning($"Код уже использован для {email}");
                return false;
            }

            if (DateTime.UtcNow > tempCode.Expiration)
            {
                _logger.LogWarning($"Код просрочен для {email} (истёк {tempCode.Expiration})");
                return false;
            }

            if (tempCode.Code != code)
            {
                _logger.LogWarning($"Неверный код для {email} (ожидался: {tempCode.Code}, получен: {code})");
                return false;
            }

            tempCode.IsUsed = true;
            _logger.LogInformation($"Код подтверждён для {email}");
            return true;
        }

        public void InvalidateCode(string email)
        {
            var lowerEmail = email.ToLowerInvariant();
            _codes.TryRemove(lowerEmail, out _);
        }
    }

    public class TempCode
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; }
    }
}