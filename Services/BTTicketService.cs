using BugBurner.Data;
using BugBurner.Models;
using BugBurner.Models.ViewModels;
using BugBurner.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace BugBurner.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext applicationDbContext)
        {

            _context = applicationDbContext;
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.BTUser)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int? ticketId, int? companyId)
        {
            try
            {
                Ticket? ticket = new();

                if (ticketId != null && companyId != null)
                {
                    ticket = await _context.Tickets
                                            .Where(t => t.Project!.CompanyId == companyId && t.Project.Archived == false)
                                            .Include(t => t.Project)
                                                .ThenInclude(p => p!.Company)
                                            .Include(t => t.Attachments)
                                            .Include(t => t.Comments)
                                            .Include(t => t.DeveloperUser)
                                            .Include(t => t.History)
                                            .Include(t => t.SubmitterUser)
                                            .Include(t => t.TicketPriority)
                                            .Include(t => t.TicketStatus)
                                            .Include(t => t.TicketType)
                                            .FirstOrDefaultAsync(t => t.Id == ticketId);

                }
                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AssignTicketAsync(int? ticketId, string? userId)
        {
            try
            {
                if (ticketId != null && !string.IsNullOrEmpty(userId))
                {
                    BTUser? bTUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                    Ticket? ticket = await GetTicketByIdAsync(ticketId, bTUser!.CompanyId);


                    if (ticket != null && bTUser != null)
                    {
                        ticket!.DeveloperUserId = userId;

                        await UpdateTicketAsync(ticket);



                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task AddTicketAsync(Ticket? ticket)
        {
            throw new NotImplementedException();
        }

        public Task AddTicketCommentAsync(TicketComment? ticketComment)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateTicketAsync(Ticket? ticket)
        {
            try
            {
                if (ticket != null)
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int? companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                    .Where(t => t.Project!.CompanyId == companyId && t.Archived == false && t.ArchivedByProject == false)
                                                    .Include(t => t.Project)
                                                .ThenInclude(p => p!.Company)
                                            .Include(t => t.Attachments)
                                            .Include(t => t.Comments)
                                            .Include(t => t.DeveloperUser)
                                            .Include(t => t.History)
                                            .Include(t => t.SubmitterUser)
                                            .Include(t => t.TicketPriority)                                            
                                            .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedAndActiveTicketsByCompanyIdAsync(int? companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                    .Where(t => t.Project!.CompanyId == companyId)
                                                    .Include(t => t.Project)
                                                .ThenInclude(p => p!.Company)
                                            .Include(t => t.Attachments)
                                            .Include(t => t.Comments)
                                            .Include(t => t.DeveloperUser)
                                            .Include(t => t.History)
                                            .Include(t => t.SubmitterUser)
                                            .Include(t => t.TicketPriority)
                                            .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsByCompanyIdAsync(int? companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                    .Where(t => t.Project!.CompanyId == companyId && t.Archived == true || t.ArchivedByProject == true)
                                                    .Include(t => t.Project)
                                                .ThenInclude(p => p!.Company)
                                            .Include(t => t.Attachments)
                                            .Include(t => t.Comments)
                                            .Include(t => t.DeveloperUser)
                                            .Include(t => t.History)
                                            .Include(t => t.SubmitterUser)
                                            .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p!.Company)
                                    .Include(t => t.Attachments)
                                    .Include(t => t.Comments)
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.History)
                                    .Include(t => t.SubmitterUser)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId && t.Archived == false);

                return ticket!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<TicketAttachment?> GetTicketAttachmentByIdAsync(int? ticketAttachmentId)
        {
            throw new NotImplementedException();
        }

        public Task<BTUser?> GetTicketDeveloperAsync(int? ticketId, int? companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string? userId, int? companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                    .Where(t => t.Project!.CompanyId == companyId && t.Archived == false && t.ArchivedByProject == false && t.DeveloperUser!.Id == userId)
                                                    .Include(t => t.Project)
                                                .ThenInclude(p => p!.Company)
                                            .Include(t => t.Attachments)
                                            .Include(t => t.Comments)
                                            .Include(t => t.DeveloperUser)
                                            .Include(t => t.History)
                                            .Include(t => t.SubmitterUser)
                                            .Include(t => t.TicketPriority)
                                            .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TicketType>> GetTicketTypesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task ArchiveTicketAsync(Ticket? ticket)
        {
            try
            {
                if (ticket != null)
                {
                    ticket.Archived = true;

                    await UpdateTicketAsync(ticket);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreTicketAsync(Ticket? ticket)
        {
            if (ticket != null)
            {
                ticket.Archived = false;

                await UpdateTicketAsync(ticket);
            }
        }
    }
}
