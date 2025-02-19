using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Data.Data
{
    public class MongoDbContext : DbContext
    {
        public DbSet<AdminLogInHistory> AdminLogInHistories { get; init; }

        public DbSet<UserLogInHistory> UserLogInHistories { get; init; }

        public DbSet<AdminUserPhoto> UserPhotos { get; init; }

        public DbSet<AuthorPhoto> AuthorPhotos { get; init; }

        public DbSet<UniversityPhoto> UniversityPhotos { get; init; }

        public DbSet<PublicationFile> PublicationFiles { get; init; }

        public DbSet<LogActionModel> LogActionModels { get; init; }

        public MongoDbContext(DbContextOptions options) : base(options)
        {

        }
        public MongoDbContext() : base() { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AdminLogInHistory>().ToCollection("admin_log_in_histories");

            modelBuilder.Entity<UserLogInHistory>().ToCollection("user_log_in_histories");

            modelBuilder.Entity<AdminUserPhoto>().ToCollection("admin_user_photos");

            modelBuilder.Entity<AuthorPhoto>().ToCollection("author_photos");

            modelBuilder.Entity<PublicationFile>().ToCollection("publication_files");

            modelBuilder.Entity<UniversityPhoto>().ToCollection("university_photos");

            modelBuilder.Entity<LogActionModel>().ToCollection("log_actions");
        }
    }
}
