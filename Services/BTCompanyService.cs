using BugBurner.Data;
using BugBurner.Models;
using BugBurner.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugBurner.Services
{
	public class BTCompanyService : IBTCompanyService
	{
		private readonly ApplicationDbContext _context;
		private readonly IBTRolesService _rolesService;

		public BTCompanyService(ApplicationDbContext context, IBTRolesService bTRolesService)
		{
			_context = context;
			_rolesService = bTRolesService;
		}

		public async Task<Company> GetCompanyInfoAsync(int? companyId)
		{
			if (companyId == null){ return new Company(); }
			try
			{
				Company? company = await _context.Companies
					.Include(c => c.Projects)
					.Include(c => c.Members)
					.Include(c => c.Invites)
					.FirstOrDefaultAsync(m => m.Id == companyId);
				return company!;

			}
			catch (Exception)
			{

				throw;
			}

		}

		public async Task<List<Invite>> GetInvitesAsync(int? companyId)
		{
			try
			{
				List<Invite> invites = await _context.Invites
											   .Include(i => i.Company)
											   .Include(i => i.Invitor)
											   .Include(i => i.Invitee)
											   .Include(i => i.Project)
											   .ToListAsync();											   ;
				return invites;
			}
			catch (Exception)
			{

				throw;
			}
				

		}

		public async Task<List<BTUser>> GetMembersAsync(int? companyId)
		{
			try
			{
				List<BTUser> members = new();
				members = await _context.Users.ToListAsync();
				return members;
			}
			catch (Exception)
			{

				throw;
			}
		}

        public async Task<List<BTUser>> GetCompanyMembersByRoleAsync(string? roleName, int? companyId)
        {
            try
            {
                List<BTUser> members = new();
                if ( companyId != null && !string.IsNullOrEmpty(roleName))
                {

					Company? company = await GetCompanyInfoAsync(companyId);

                    if (company != null)
                    {
                        foreach (BTUser member in company.Members)
                        {
                            if (await _rolesService.IsUserInRoleAsync(member, roleName))
                            {
                                members.Add(member);
                            }
                        }
                        List<string> developers = (await _rolesService.GetUsersInRoleAsync(roleName, companyId))?.Select(u => u.Id).ToList()!;
                        List<BTUser> users = company.Members.Where(m => developers.Contains(m.Id)).ToList();
                    }
                }
                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetProjectsAsync(int? companyId)
		{
			try
			{								
				List<Project> projects = await _context.Projects
				.Where(p => p.Archived == false && p.CompanyId == companyId)
				.Include(p => p.Company)
				.Include(p => p.ProjectPriority)
				.ToListAsync();
				return projects;
			}
			catch (Exception)
			{

				throw;
			}
		}

        public async Task<List<Project>> GetArchivedProjectsAsync(int? companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                .Where(p => p.Archived == true && p.CompanyId == companyId)
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .ToListAsync();
                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Project> GetProjectDetailsAsync(int? id)
		{
			if (id == null) { return new Project(); }
			try
			{
				Project? project = await _context.Projects
				.Include(p => p.Members)
				.Include(p => p.Company)
				.Include(p => p.ProjectPriority)
				.Include(p => p.Tickets)
				.ThenInclude(p => p.TicketPriority)
				.FirstOrDefaultAsync(m => m.Id == id);
				return project!;
			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
