using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugBurner.Data;
using BugBurner.Models;
using BugBurner.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.IO;
using NuGet.Protocol;
using BugBurner.Models.ViewModels;
using BugBurner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using BugBurner.Extensions;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.AspNetCore.SignalR;
using System.Security.AccessControl;

namespace BugBurner.Controllers
{
    [Authorize]
    public class TicketsController : BTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketHistoryService _ticketHistoryService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTCompanyService _companyService;

        public TicketsController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTFileService bTFileService, IBTTicketService bTTicketService, IBTProjectService bTProjectService, IBTTicketHistoryService bTTicketHistoryService, IBTNotificationService bTNotificationService, IBTCompanyService bTCompanyService)
        {
            _userManager = userManager;
            _context = context;
            _fileService = bTFileService;
            _ticketService = bTTicketService;
            _projectService = bTProjectService;
            _ticketHistoryService = bTTicketHistoryService;
            _notificationService = bTNotificationService;
            _companyService = bTCompanyService;

        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId);

            return View(tickets);
        }

        public async Task<IActionResult> Index2()
        {
            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId);

            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            string? userId = _userManager.GetUserId(User);

            Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);

            if (ModelState.IsValid)
            {
                try
                {
                    //set updated date
                    ticket.Updated = DateTime.Now;

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

					Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);
					await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);

				}
				catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Tickets", new {id = ticket.Id.ToString()});
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);





            return View(ticket);
        }

        [Authorize]
        public async Task<IActionResult> ArchivedTickets()
        {
            IEnumerable<Ticket> tickets = await _ticketService.GetArchivedTicketsByCompanyIdAsync(_companyId);

            return View(tickets);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> ArchiveTicket(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            await _ticketService.ArchiveTicketAsync(ticket);


            return RedirectToAction(nameof(ArchivedTickets));


        }
        public async Task<IActionResult> UndoArchive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            await _ticketService.RestoreTicketAsync(ticket);

            return RedirectToAction(nameof(ArchivedTickets));

        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id , _companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Archived/Tickets/Details/5
        public async Task<IActionResult> ArchivedDetails(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetArchivedTicketByIdAsync(id, _companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create

        public IActionResult Create()
        {

            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId")] Ticket ticket)
        {
            string? currentUserId = _userManager.GetUserId(User);
            ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                //sets the submitted user
                ticket.SubmitterUserId = _userManager.GetUserId(User);
                //set created date 
                ticket.Created = DateTime.Now;
                //adds and saves ticket to the database
                _context.Add(ticket);
                await _context.SaveChangesAsync();


                
                //in development
                string? userId = _userManager.GetUserId(User);

                int companyId = User.Identity!.GetCompanyId();
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _ticketHistoryService.AddHistoryAsync(null!, newTicket, userId);

                await _notificationService.NewTicketNotificationAsync(ticket.Id, currentUserId);


                return RedirectToAction(nameof(Index));
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;
            ModelState.Remove("BTUserId");
            ModelState.Remove("FormFile");

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTime.Now;
                ticketAttachment.BTUserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName!).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        [Authorize(Roles = "ProjectManager, Admin")]
        public async Task<IActionResult> AssignTicket(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            AssignTicketViewModel viewModel = new();
            viewModel.Ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            string? currentDeveloper = viewModel.Ticket?.DeveloperUserId;
            viewModel.Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(viewModel.Ticket?.ProjectId, nameof(BTRoles.Developer), _companyId), "Id", "FullName", currentDeveloper);


            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="ProjectManager, Admin")]
       
        public async Task<IActionResult> AssignTicket(AssignTicketViewModel viewModel)
        {

            if (viewModel.DeveloperId != null && viewModel.Ticket?.Id != null)
            {
				string? currentUserId = _userManager.GetUserId(User);

				Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket?.Id, _companyId);
                string? userId = _userManager.GetUserId(User);
                try
                {
                    await _ticketService.AssignTicketAsync(viewModel.Ticket?.Id, viewModel.DeveloperId);

                }
                catch (Exception)
                {

                    throw;
                }
				//add history
				Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(viewModel.Ticket?.Id, _companyId);
				await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);
                //add notification
                await _notificationService.NewDeveloperNotificationAsync(viewModel.Ticket?.Id, viewModel.DeveloperId, currentUserId);

                return RedirectToAction(nameof(Details), new { id = viewModel.Ticket!.Id });
			}
			return View(viewModel);
        }
    }
}
