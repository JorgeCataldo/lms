using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Domain.Aggregates.Users;
using Domain.Data;

namespace Domain.IdentityStores
{
    public class UserStore : IUserPasswordStore<User>, IUserEmailStore<User>, IUserRoleStore<User>
    {
        private readonly IDbContext _context;

        public UserStore(IDbContext context)
        {
            _context = context;
        }
        
        public void Dispose()
        {
            
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            user.NormalizedUsername = user.UserName.ToUpper();
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName.ToUpper());
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUsername = normalizedName.ToUpper();
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            var userDb = await FindByNameAsync(user.UserName, cancellationToken);

            if (userDb != null)
                return IdentityResult.Failed(new IdentityError {Code = "001", Description = "UserName já existente."});

            user.NormalizedUsername = user.UserName.ToUpper();
            
            await _context.UserCollection.InsertOneAsync(user, cancellationToken: cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            var userDb = await FindByIdAsync(user.Id.ToString(), cancellationToken);
            
            if (userDb == null)
                return IdentityResult.Failed(new IdentityError {Code = "002", Description = "Usuário não existe"});
            
            if (user.UserName != userDb.UserName)
                return IdentityResult.Failed(new IdentityError {Code = "003", Description = "UserName não pode ser diferente."});
            
            user.UpdatedAt = DateTimeOffset.Now;
            
            await _context.UserCollection.ReplaceOneAsync(x => x.Id == user.Id, user, cancellationToken: cancellationToken);
            
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            var userDb = await FindByIdAsync(user.Id.ToString(), cancellationToken);
            
            if (userDb == null)
                return IdentityResult.Failed(new IdentityError {Code = "002", Description = "Usuário não existe"});
                        
            user.DeletedAt = DateTimeOffset.Now;
            
            await _context.UserCollection.DeleteOneAsync(x => x.Id == user.Id, cancellationToken);
            
            return IdentityResult.Success;
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var existingUsers = await _context.UserCollection
                .FindAsync(x => x.Id == ObjectId.Parse(userId), cancellationToken: cancellationToken);

            return existingUsers.FirstOrDefault();
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var existingUsers = await _context.UserCollection
                .FindAsync(x => x.NormalizedUsername == normalizedUserName && x.DeletedAt != null, cancellationToken: cancellationToken);

            return existingUsers.FirstOrDefault();
        }

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!String.IsNullOrEmpty(user.Password));
        }

        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            user.NormalizedEmail = user.Email.ToUpper();
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.IsEmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            user.IsEmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var existingUsers = await _context.UserCollection
                .FindAsync(x => x.NormalizedEmail == normalizedEmail && x.DeletedAt != null, cancellationToken: cancellationToken);

            return existingUsers.FirstOrDefault();
        }

        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email.ToUpper());
        }

        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail.ToUpper();
            return Task.CompletedTask;
        }

        public Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            user.Roles.Add(roleName);
            return Task.CompletedTask;
        }

        public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            user.Roles.Remove(roleName);
            return Task.CompletedTask;
        }

        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Roles ?? new List<string>());
        }

        public Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Roles.Contains(roleName));
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var existingUsers = await _context.UserCollection
                .FindAsync(x => x.Roles.Contains(roleName) && x.DeletedAt != null, cancellationToken: cancellationToken);

            return existingUsers.ToList();
        }
    }
}