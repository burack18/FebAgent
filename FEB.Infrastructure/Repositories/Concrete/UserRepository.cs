using FEB.Infrastructure.Configuration;
using FEB.Infrastructure.Dto;
using FEB.Infrastructure.Repositories.Abstract;
using FEBAgent.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure.Repositories.Concrete
{
    public class UserRepository : IUserRepository
    {
        private CosmosClient _client;
        private Container _container;

        public UserRepository(CosmosClient client, IConfiguration configuration)
        {
            var cosmosConfig = configuration
                .GetSection("CosmosDb")
                .Get<CosmosDbSettings>() ?? throw new Exception("CosmosDB configuration Required");
            _client = client;
            _container = _client.GetContainer(cosmosConfig.DatabaseName, "users");
        }
        public async Task AddUserAsync(FEBAgent.Domain.User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(FEBAgent.Domain.User));
            user.Password = await BycrptPassword(user.Password);
            await _container.CreateItemAsync<FEBAgent.Domain.User>(user);
        }

        public async Task<string> BycrptPassword(string password)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            return await Task.FromResult(hash);
        }

        public async Task<bool> CheckPassword(string username, string password)
        {
            var query = _container
                        .GetItemLinqQueryable<FEBAgent.Domain.User>(allowSynchronousQueryExecution: false)
                        .Where(u => u.UserName == username)
                        .ToFeedIterator();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var user = response.FirstOrDefault();
                if (user != null)
                {
                    return await Verify(user.Password,password);
                }
            }
            return await Task.FromResult(false);
        }

        public Task DeleteUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var query = _container.GetItemLinqQueryable<FEBAgent.Domain.User>(false).ToFeedIterator();

            List<UserDto> results = new();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var users = response.Select(user => new UserDto()
                {
                    UserID = user.UserID,
                    UserName = user.UserName,
                    Email = user.Email,
                    CreatedOn = user.CreatedOn,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Id = user.Id
                });
                results = users.ToList();
            }

            return results;
        }



        public async Task<UserDto?> GetUserAsync(string username)
        {
            var query = _container
                .GetItemLinqQueryable<FEBAgent.Domain.User>(allowSynchronousQueryExecution: false)
                .Where(u => u.UserName == username)
                .ToFeedIterator();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var user = response.FirstOrDefault();
                if (user != null)
                {
                    return new UserDto()
                    {
                        UserID = user.UserID,
                        UserName = user.UserName,
                        Email = user.Email,
                        CreatedOn = user.CreatedOn,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Id = user.Id
                    };
                }
            }

            return null;
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var query = _container
                    .GetItemLinqQueryable<FEBAgent.Domain.User>(allowSynchronousQueryExecution: false)
                    .Where(u => u.UserID == userId)
                    .ToFeedIterator();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var user = response.FirstOrDefault();

                if (user != null)
                {
                    return new UserDto()
                    {
                        UserID = user.UserID,
                        UserName = user.UserName,
                        Email = user.Email,
                        CreatedOn = user.CreatedOn,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Id = user.Id
                    };
                }
            }

            return null;
        }


        public Task UpdateUserAsync(UserDto user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Verify(string storedPassword, string password)
        {
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, storedPassword);
            return await Task.FromResult(isPasswordValid);
        }
    }
}
