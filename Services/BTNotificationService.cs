﻿using BugBurner.Data;
using BugBurner.Models.Enums;
using BugBurner.Models;
using BugBurner.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugBurner.Services
{
	public class BTNotificationService : IBTNotificationService
	{
		private readonly ApplicationDbContext _context;
		private readonly IEmailSender _emailService;
		private readonly IBTRolesService _rolesService;
		private readonly IBTProjectService _projectService;
		private readonly UserManager<BTUser> _userManager;

		public BTNotificationService(ApplicationDbContext context,
									 IEmailSender emailService,
									 IBTRolesService rolesService,
									 UserManager<BTUser> userManager,
									 IBTProjectService projectService)
		{
			_context = context;
			_emailService = emailService;
			_rolesService = rolesService;
			_userManager = userManager;
			_projectService = projectService;
		}

		public async Task AddNotificationAsync(Notification? notification)
		{
			try
			{
				if (notification != null)
				{
					await _context.AddAsync(notification);
					await _context.SaveChangesAsync();
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task NotificationsByRoleAsync(int? companyId, Notification? notification, BTRoles role)
		{
			try
			{
				if (notification != null)
				{
					IEnumerable<string> memberIds = (await _rolesService.GetUsersInRoleAsync(nameof(role), companyId))!.Select(u => u.Id);

					foreach (string adminId in memberIds)
					{
						notification.Id = 0;
						notification.RecipientId = adminId;

						await _context.AddAsync(notification);
					}

					await _context.SaveChangesAsync();
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		public async Task<List<Notification>> GetNotificationsByUserIdAsync(string? userId)
		{
			try
			{
				List<Notification> notifications = new();

				if (!string.IsNullOrEmpty(userId))
				{

					notifications = await _context.Notifications
												  .Where(n => n.RecipientId == userId || n.SenderId == userId && n.HasBeenViewed == false)
												  .Include(n => n.Recipient)
												  .Include(n => n.Sender)
												  .ToListAsync();
				}

				return notifications;


			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<bool> NewDeveloperNotificationAsync(int? ticketId, string? developerId, string? senderId)
		{
			try
			{
				BTUser? btUser = await _userManager.FindByIdAsync(senderId!);
				Ticket? ticket = await _context.Tickets.FindAsync(ticketId);

				int notificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(t => t.Name == nameof(BTNotificationType.Ticket)))!.Id;

				Notification? notification = new()
				{
					TicketId = ticket!.Id,
					Title = "Developer Assigned",
					Message = $"Ticket: {ticket.Title} was assigned by {btUser?.FullName} ",
					Created = DateTime.Now,
					SenderId = senderId,
					RecipientId = developerId,
					NotificationTypeId = notificationTypeId
				};


				await AddNotificationAsync(notification);
				await SendEmailNotificationAsync(notification, "Ticket Developer Assigned");

				return true;

			}
			catch (Exception)
			{
				return false;
				throw;
			}
		}

		public async Task<bool> NewTicketNotificationAsync(int? ticketId, string? senderId)
		{
			BTUser? btUser = await _userManager.FindByIdAsync(senderId!);
			Ticket? ticket = await _context.Tickets.FindAsync(ticketId);
			BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket?.ProjectId);

			if (ticket != null && btUser != null)
			{
				Notification? notification = new()
				{
					TicketId = ticket.Id,
					Title = "New Ticket Added",
					Message = $"New Ticket: {ticket.Title} was created by {btUser.FullName} ",
					Created = DateTime.Now,
					SenderId = senderId,
					RecipientId = projectManager?.Id ?? senderId,
					NotificationType = new NotificationType()
					{
						Name = BTNotificationType.Ticket.ToString()
					}
				};


				if (projectManager != null)
				{
					await AddNotificationAsync(notification);
					await SendEmailNotificationAsync(notification, "New Ticket Added");
				}
				else
				{
					await NotificationsByRoleAsync(btUser.CompanyId, notification, BTRoles.Admin);
					await SendEmailNotificationByRoleAsync(btUser.CompanyId, notification, BTRoles.Admin);
				}
				return true;
			}
			return false;
		}

		public async Task<bool> TicketUpdateNotificationAsync(int? ticketId, string? developerId, string? senderId = null)
		{
			try
			{
				BTUser? btUser = await _userManager.FindByIdAsync(developerId!);
				Ticket? ticket = await _context.Tickets.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == ticketId);
				BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket?.ProjectId);


				if (ticket != null)
				{
					int companyId = ticket.Project!.CompanyId;
					Notification? notification = new()
					{
						TicketId = ticketId,
						Title = "Ticket Updated",
						Message = $"Ticket: {ticket?.Title} was updated by {btUser?.FullName} ",
						Created = DateTime.Now,
						SenderId = senderId,
						RecipientId = projectManager?.Id ?? senderId,
						NotificationType = new NotificationType() { Name = BTNotificationType.Ticket.ToString() }
					};


					if (projectManager != null)
					{
						await AddNotificationAsync(notification);
						await SendEmailNotificationAsync(notification, "New Ticket Added");
					}
					else
					{

						await NotificationsByRoleAsync(companyId, notification, BTRoles.Admin);
						await SendEmailNotificationByRoleAsync(companyId, notification, BTRoles.Admin);
					}

					return true;
				}

				return false;
			}
			catch (Exception)
			{
				return false;
				throw;
			}
		}

		public async Task<bool> SendEmailNotificationByRoleAsync(int? companyId, Notification? notification, BTRoles role)
		{
			try
			{

				if (notification != null)
				{
					IEnumerable<string> memberEmails = (await _rolesService.GetUsersInRoleAsync(nameof(role), companyId))!.Select(u => u.Email)!;

					foreach (string adminEmail in memberEmails)
					{
						await _emailService.SendEmailAsync(adminEmail, notification.Title!, notification.Message!);
					}
					return true;
				}

				return false;
			}
			catch (Exception)
			{

				throw;
			}


		}

		public async Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject)
		{
			try
			{
				if (notification != null)
				{
					BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

					string? userEmail = btUser?.Email;

					if (userEmail != null)
					{
						await _emailService.SendEmailAsync(userEmail, emailSubject!, notification.Message!);
						return true;
					}
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
