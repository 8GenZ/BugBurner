﻿using BugBurner.Data;
using BugBurner.Models;
using BugBurner.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace BugBurner.Services
{
	public class BTRolesService : IBTRolesService
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<BTUser> _userManager;

        public BTRolesService(ApplicationDbContext applicationDbContext, UserManager<BTUser> userManager )
        {
           _context = applicationDbContext;
			_userManager = userManager;
        }


        public async Task<bool> AddUserToRoleAsync(BTUser? user, string? roleName)
		{
			try
			{
				if (user != null && !string.IsNullOrEmpty(roleName))
				{
					bool result =  (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
					return result;
				}

				return false;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<List<IdentityRole>> GetRolesAsync()
		{
			try
			{
				List<IdentityRole> result = new();
				result = await _context.Roles.ToListAsync();
				return result;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<IEnumerable<string>?> GetUserRolesAsync(BTUser? user)
		{
			try
			{
				if(user != null)
				{
					IEnumerable<string>? result = await _userManager.GetRolesAsync(user);
					return result;
				}
				return null;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<List<BTUser>> GetUsersInRoleAsync(string? roleName, int? companyId)
		{
			try
			{
				List<BTUser> result = new();
				List<BTUser> users = new();

				if (!string.IsNullOrEmpty(roleName) && companyId != null)
				{
					users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
					result = users.Where(u => u.CompanyId == companyId).ToList();
				}

					return result;

			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<bool> IsUserInRoleAsync(BTUser? member, string? roleName)
		{
			try
			{
				if(member != null && !string.IsNullOrEmpty(roleName))
				{
					bool result = await _userManager.IsInRoleAsync(member, roleName);
					return result;
				}
				return false;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<bool> RemoveUserFromRoleAsync(BTUser? user, string? roleName)
		{
			try
			{
				if(user != null && !string.IsNullOrEmpty(roleName))
				{
					bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
					return result;
				}
				return false;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<bool> RemoveUserFromRolesAsync(BTUser? user, IEnumerable<string>? roleNames)
		{
			try
			{
				if (user != null && roleNames != null)
				{
					bool result = (await _userManager.RemoveFromRolesAsync(user, roleNames)).Succeeded;
					return result;
				}
				return false;
			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
