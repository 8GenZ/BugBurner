﻿using BugBurner.Models;
using Microsoft.AspNetCore.Identity;

namespace BugBurner.Services.Interfaces
{
	public interface IBTRolesService
	{
		public Task<bool> AddUserToRoleAsync(BTUser? user, string? roleName);

		public Task<List<IdentityRole>> GetRolesAsync();

		public Task<IEnumerable<string>?> GetUserRolesAsync(BTUser? user);

		public Task<List<BTUser>> GetUsersInRoleAsync(string? roleName, int? companyId);

		public Task<bool> IsUserInRoleAsync(BTUser? member, string? roleName);

		public Task<bool> RemoveUserFromRoleAsync(BTUser? user, string? roleName);

		public Task<bool> RemoveUserFromRolesAsync(BTUser? user, IEnumerable<string>? roleNames);
	}
}
