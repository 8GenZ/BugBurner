using BugBurner.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BugBurner.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<BugBurner.Models.Company> Companies { get; set; } = default!;
        public DbSet<BugBurner.Models.Invite> Invites { get; set; } = default!;
        public DbSet<BugBurner.Models.Notification> Notifications { get; set; } = default!;
        public DbSet<BugBurner.Models.NotificationType> NotificationTypes { get; set; } = default!;
        public DbSet<BugBurner.Models.Project> Projects { get; set; } = default!;
        public DbSet<BugBurner.Models.ProjectPriority> ProjectPriorities { get; set; } = default!;
        public DbSet<BugBurner.Models.Ticket> Tickets { get; set; } = default!;
        public DbSet<BugBurner.Models.TicketAttachment> TicketAttachments { get; set; } = default!;
        public DbSet<BugBurner.Models.TicketComment> TicketComments { get; set; } = default!;
        public DbSet<BugBurner.Models.TicketHistory> TicketHistories { get; set; } = default!;
        public DbSet<BugBurner.Models.TicketPriority> TicketPriorities { get; set; } = default!;
        public DbSet<BugBurner.Models.TicketStatus> TicketStatuses { get; set; } = default!;
        public DbSet<BugBurner.Models.TicketType> TicketTypes { get; set; } = default!;
    }
}