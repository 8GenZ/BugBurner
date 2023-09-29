using BugBurner.Data;
using BugBurner.Models;
using BugBurner.Models.Enums;
using BugBurner.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.ComponentModel.Design;

namespace BugBurner.Services
{

    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTRolesService bTRolesService)
        {

            _context = context;
            _rolesService = bTRolesService;
        }

        public async Task AddMembersToProjectAsync(IEnumerable<string>? userIds, int? projectId, int? companyId)
        {
            try
            {
                if (userIds != null)
                {
                    Project? project = await GetProjectByIdAsync(projectId, companyId);
                    foreach (string userId in userIds)
                    {
                        BTUser? bTUser = await _context.Users.FindAsync(userId);
                        if (project != null && bTUser != null)
                        {
                            bool IsOnProject = project.Members.Any(m => m.Id == userId);

                            if (!IsOnProject)
                            {
                                project.Members.Add(bTUser);
                                bTUser.Projects.Add(project);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                if (member != null && projectId != null)
                {
                    Project? project = await GetProjectByIdAsync(projectId, member.CompanyId);
                    if (project != null)
                    {
                        bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                        if (IsOnProject == false)
                        {
                            project.Members.Add(member);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddProjectAsync(Project project)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddProjectManagerAsync(string? userId, int? projectId)
        {
            try
            {
                if (userId != null && projectId != null)
                {
                    BTUser? currentPM = await GetProjectManagerAsync(projectId);
                    BTUser? selectedPM = await _context.Users.FindAsync(userId);

                    if (currentPM != null)
                    {
                        await RemoveProjectManagerAsync(projectId);
                    }


                    try
                    {
                        await AddMemberToProjectAsync(selectedPM!, projectId);
                        return true;
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public async Task ArchiveProjectAsync(Project? project, int? companyId)
        {
            try
            {
                project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == companyId);


                if (project != null)
                {
                    IEnumerable<Ticket>? tickets = project.Tickets;

                    project.Archived = true;

                    foreach (Ticket ticket in tickets)
                    {
                        ticket.ArchivedByProject = true;
                    }

                    await _context.SaveChangesAsync();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int? companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                .Where(p => p.CompanyId == companyId)
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

        public async Task<List<Project>> GetActiveProjectsByCompanyIdAsync(int? companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                .Where(p => p.CompanyId == companyId && p.Archived == false)
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

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int? companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                .Where(p => p.CompanyId == companyId && p.Archived == true)
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

        public async Task<Project> GetProjectByIdAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = new();
                if (projectId != null && companyId != null)
                {
                    project = await _context.Projects
                                            .Include(p => p.ProjectPriority)
                                            .Include(p => p.Company)
                                            .Include(p => p.Members)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(c => c.Comments)
                                                    .ThenInclude(c => c.User)
                                            .Include(p => p.Tickets)
                                                    .ThenInclude(c => c.Attachments)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(c => c.History)
                                                    .ThenInclude(c => c.User)
                                            .Include(p => p.Tickets)
                                                     .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)
                                                     .ThenInclude(t => t.DeveloperUser)
                                            .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                }
                return project!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                if (project != null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                        {
                            return member;
                        }
                    }
                }
                return null!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int? projectId, string? roleName, int? companyId)
        {
            try
            {
                List<BTUser> members = new();
                if (projectId != null && companyId != null && !string.IsNullOrEmpty(roleName))
                {

                    Project? project = await GetProjectByIdAsync(projectId, companyId);

                    if (project != null)
                    {
                        foreach (BTUser member in project.Members)
                        {
                            if (await _rolesService.IsUserInRoleAsync(member, roleName))
                            {
                                members.Add(member);
                            }
                        }
                        List<string> developers = (await _rolesService.GetUsersInRoleAsync(roleName, companyId))?.Select(u => u.Id).ToList()!;
                        List<BTUser> users = project.Members.Where(m => developers.Contains(m.Id)).ToList();
                    }
                }
                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Project>?> GetUserProjectsAsync(string? userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                if (member != null && projectId != null)
                {
                    Project? project = await GetProjectByIdAsync(projectId, member.CompanyId);

                    if (project != null)
                    {
                        bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                        if (IsOnProject)
                        {
                            project.Members.Remove(member);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveMembersFromProjectAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);
                foreach (var member in project.Members)
                {
                    if (!await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        project.Members.Remove(member);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        await RemoveMemberFromProjectAsync(member, projectId);
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreProjectAsync(Project? project, int? companyId)
        {
            try
            {
                if (project != null && companyId != null)
                {
                    foreach (Ticket ticket in project.Tickets)
                    {
                        ticket.ArchivedByProject = false;
                    }

                    project.Archived = false;

                    await UpdateProjectAsync(project);

                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project? project)
        {
            if (project == null) { return; }
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
