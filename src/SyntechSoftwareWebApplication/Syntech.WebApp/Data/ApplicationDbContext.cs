using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Models;
using Syntech.WebApp.Models.Ticket;

namespace Syntech.WebApp.Data
{
    /// <summary>
    /// Entity Framework Core database context for the Syntech application.
    ///
    /// This class provides the application's connection between C# objects
    /// and the existing SQL Server database.
    ///
    /// IMPORTANT:
    /// The database already exists and is shared by the project team.
    /// This context is therefore intended to map to the existing schema.
    /// We are NOT using EF Core migrations to create a new database.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of ApplicationDbContext.
        /// </summary>
        /// <param name="options">
        /// Configuration options supplied by ASP.NET Core's dependency
        /// injection system.
        /// </param>
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Represents the dbo.User table.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Represents the dbo.Admin table.
        /// </summary>
        public DbSet<Admin> Admins { get; set; }

        /// <summary>
        /// Represents the dbo.Developer table.
        /// </summary>
        public DbSet<Developer> Developers { get; set; }

        /// <summary>
        /// Represents the dbo.Client table.
        /// </summary>
        public DbSet<Client> Clients { get; set; }

        /// <summary>
        /// Configures the database model and relationships.
        ///
        /// This method explicitly describes the existing database structure
        /// so Entity Framework does not have to guess how the role-specific
        /// tables relate to User.
        /// </summary>
        /// <param name="modelBuilder">
        /// Builder used to configure Entity Framework's database model.
        /// </param>

        public DbSet<DeveloperProject> DeveloperProjects { get; set; }

        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketReply> TicketReplies { get; set; }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUpdate> ProjectUpdates { get; set; }
        public DbSet<Document> Documents { get; set; }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------------------------------------------
            // User table configuration
            // -------------------------------------------------------------

            modelBuilder.Entity<User>(entity =>
            {
                // Map the entity to the existing dbo.User table.
                entity.ToTable("User");

                // userID is the primary key.
                entity.HasKey(user => user.UserId);

                // Match the existing database column names.
                entity.Property(user => user.UserId)
                    .HasColumnName("userID");

                entity.Property(user => user.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(user => user.Email)
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(user => user.PasswordHash)
                    .HasColumnName("passwordHash")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(user => user.Role)
                    .HasColumnName("role")
                    .HasMaxLength(20)
                    .IsRequired();
                entity.Property(user => user.IsActive)
                    .HasColumnName("isActive")
                    .IsRequired()
                    .HasDefaultValue(true);



                // The database already has a UNIQUE constraint on email.
                // This index tells EF Core about that existing uniqueness.
                entity.HasIndex(user => user.Email)
                    .IsUnique();
            });

            // -------------------------------------------------------------
            // Admin table configuration
            // -------------------------------------------------------------

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("Admin");

                // userID is the primary key.
                entity.HasKey(admin => admin.UserId);

                entity.Property(admin => admin.UserId)
                    .HasColumnName("userID");

                // Admin.userID references User.userID.
                //
                // This is a one-to-one relationship because userID is both
                // the primary key of Admin and its foreign key to User.
                entity.HasOne(admin => admin.User)
                    .WithOne(user => user.Admin)
                    .HasForeignKey<Admin>(admin => admin.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------------------------------------------
            // Developer table configuration
            // -------------------------------------------------------------

            modelBuilder.Entity<Developer>(entity =>
            {
                entity.ToTable("Developer");

                // userID is the primary key.
                entity.HasKey(developer => developer.UserId);

                entity.Property(developer => developer.UserId)
                    .HasColumnName("userID");

                // Developer.userID references User.userID.
                entity.HasOne(developer => developer.User)
                    .WithOne(user => user.Developer)
                    .HasForeignKey<Developer>(
                        developer => developer.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------------------------------------------
            // Client table configuration
            // -------------------------------------------------------------

            modelBuilder.Entity<Client>(entity =>
            {
                entity.ToTable("Client");

                // userID is the primary key.
                entity.HasKey(client => client.UserId);

                entity.Property(client => client.UserId)
                    .HasColumnName("userID");

                // Client.userID references User.userID.
                entity.HasOne(client => client.User)
                    .WithOne(user => user.Client)
                    .HasForeignKey<Client>(
                        client => client.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PROJECT
            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Project");
                entity.HasKey(p => p.ProjectID);
                entity.Property(p => p.ProjectID).HasColumnName("projectID");
                entity.Property(p => p.ProjectName).HasColumnName("projectName");
                entity.Property(p => p.Description).HasColumnName("description");
                entity.Property(p => p.Status).HasColumnName("status");
                entity.Property(p => p.ProgressPercentage).HasColumnName("progressPercentage");
                entity.Property(p => p.StartDate).HasColumnName("startDate");
                entity.Property(p => p.Deadline).HasColumnName("deadline");
                entity.Property(p => p.ClientID).HasColumnName("clientID");
                entity.Property(p => p.AdminID).HasColumnName("adminID");
            });

            // PROJECT UPDATE
            modelBuilder.Entity<ProjectUpdate>(entity =>
            {
                entity.ToTable("ProjectUpdate");
                entity.HasKey(p => p.UpdateID);
                entity.Property(p => p.UpdateID).HasColumnName("updateID");
                entity.Property(p => p.ProjectID).HasColumnName("projectID");
                entity.Property(p => p.UserID).HasColumnName("userID");
                entity.Property(p => p.UpdateTitle).HasColumnName("updateTitle");
                entity.Property(p => p.UpdateDescription).HasColumnName("updateDescription");
                entity.Property(p => p.ProgressPercentage).HasColumnName("progressPercentage");
                entity.Property(p => p.CreatedDate).HasColumnName("createdDate");
            });

            modelBuilder.Entity<ProjectUpdate>()
                .HasOne(pu => pu.Project)
                .WithMany(p => p.ProjectUpdates)
                .HasForeignKey(pu => pu.ProjectID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectUpdate>()
                .HasOne(pu => pu.User)
                .WithMany()
                .HasForeignKey(pu => pu.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // DOCUMENT
            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Document");

                entity.HasKey(d => d.DocumentID);

                entity.Property(d => d.DocumentID)
                    .HasColumnName("documentID");

                entity.Property(d => d.FileName)
                    .HasColumnName("fileName")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(d => d.OriginalFileName)
                    .HasColumnName("originalFileName")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(d => d.FileExtension)
                    .HasColumnName("fileExtension")
                    .HasMaxLength(20);

                entity.Property(d => d.FileType)
                    .HasColumnName("fileType")
                    .HasMaxLength(100);

                entity.Property(d => d.FilePath)
                    .HasColumnName("filePath")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(d => d.FileSize)
                    .HasColumnName("fileSize");

                entity.Property(d => d.UploadedBy)
                    .HasColumnName("uploadedBy")
                    .IsRequired();

                entity.Property(d => d.UploadDate)
                    .HasColumnName("uploadDate");

                entity.Property(d => d.ProjectID)
                    .HasColumnName("projectID")
                    .IsRequired();

                entity.Property(d => d.Category)
                    .HasColumnName("category")
                    .HasMaxLength(30)
                    .IsRequired();

                // Document → User
                entity.HasOne(d => d.Uploader)
                    .WithMany()
                    .HasForeignKey(d => d.UploadedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                // Document → Project
                entity.HasOne(d => d.Project)
                    .WithMany()
                    .HasForeignKey(d => d.ProjectID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DEVELOPER PROJECT
            modelBuilder.Entity<DeveloperProject>(entity =>
            {
                entity.ToTable("DeveloperProject");
                entity.HasKey(dp => new { dp.DeveloperID, dp.ProjectID });
                entity.Property(dp => dp.DeveloperID).HasColumnName("developerID");
                entity.Property(dp => dp.ProjectID).HasColumnName("projectID");

                entity.HasOne(dp => dp.Project)
                    .WithMany()
                    .HasForeignKey(dp => dp.ProjectID);

                entity.HasOne(dp => dp.Developer)
                    .WithMany()
                    .HasForeignKey(dp => dp.DeveloperID);
            });

            modelBuilder.Entity<SupportTicket>(entity =>
            {
                entity.HasOne(t => t.Client)
                    .WithMany()
                    .HasForeignKey(t => t.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.AssignedDeveloper)
                    .WithMany()
                    .HasForeignKey(t => t.AssignedDeveloperId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<TicketReply>(entity =>
            {
                entity.HasOne(r => r.SupportTicket)
                    .WithMany(t => t.Replies)
                    .HasForeignKey(r => r.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Author)
                    .WithMany()
                    .HasForeignKey(r => r.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }


    }
}