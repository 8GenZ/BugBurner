using BugBurner.Models;

namespace BugBurner.Services.Interfaces
{
	public interface IBTCompanyService
	{
		public Task<Company> GetCompanyInfoAsync(int? companyId);

		public Task<List<BTUser>> GetMembersAsync(int? companyId);

		public Task<List<Project>> GetProjectsAsync(int? companyId);

		public Task<List<BTUser>> GetCompanyMembersByRoleAsync(string? roleName, int? companyId);

        public Task<List<Project>> GetArchivedProjectsAsync(int? companyId);

		public Task<List<Invite>> GetInvitesAsync(int? companyId);

		public Task<Project> GetProjectDetailsAsync(int? companyId);
	}
}
