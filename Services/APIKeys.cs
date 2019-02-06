using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using AudiocraticAPI.Models;

namespace AudiocraticAPI.Services
{
    public interface IAPIKeyService
    {
        Task<APIKey> GetAPIKeyAsync();
        Task CreateNewAPIKeyAsync();
        Task UpdateAPIKeyAsync(APIKey key);
        Task<APIKey> GetAPIKeyByKeyAsync(string key);
    }

    public class APIKeyService : IAPIKeyService
    {
        
        private APIKey APIKey { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AudiocraticAPI.APIContext _context;
        
        public APIKeyService(
            IHttpContextAccessor httpContextAccessor,
            AudiocraticAPI.APIContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        
        public async Task<APIKey> GetAPIKeyAsync()
        {
            if(APIKey == null)
            {
                HttpContext httpContext = _httpContextAccessor.HttpContext;

                APIKey = await _context.APIKey
                .FirstOrDefaultAsync(k => k.User.UserName == httpContext.User.Identity.Name);
            }
            
            return APIKey;
        }

        public async Task<APIKey> GetAPIKeyByKeyAsync(string key)
        {
            return await _context.APIKey.FirstOrDefaultAsync(k => k.Key == key);
        }

        public async Task UpdateAPIKeyAsync(APIKey key)
        {
            APIKey currentKey = await GetAPIKeyAsync();
            
            if(currentKey == null) throw new Exception("Unable to retrieve API Key for current user.");

            _context.Attach(currentKey).State = EntityState.Modified;

            currentKey.HubSpotKey = key.HubSpotKey;
            currentKey.ConstantContactPublicKey = key.ConstantContactPublicKey;
            currentKey.ConstantContactPrivateKey = key.ConstantContactPrivateKey;

            await _context.SaveChangesAsync();
        }

        public async Task CreateNewAPIKeyAsync()
        {
            APIKey key = new APIKey();

            key.Key = CreateNewKey();

            key.User = await _context.AspNetUsers
                .FirstOrDefaultAsync(
                    u => u.UserName == _httpContextAccessor.HttpContext.User.Identity.Name);
            
            await _context.AddAsync(key);
            await _context.SaveChangesAsync();
        }

        private string CreateNewKey()
        {
            string key = string.Empty;
            
            for(int i = 1; i <= 4; i++)
            {
                if(i > 1) key += "-";
                key += CreateAPIKeyPiece();
            }

            return key;
        }

        private string CreateAPIKeyPiece()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
    }
}

