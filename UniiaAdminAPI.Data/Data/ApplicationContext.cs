using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Data.Data
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<Author> Authors { get; set; }

        public DbSet<Faculty> Faculties { get; set; }

        public DbSet<Favorite> Favorites { get; set; }

        public DbSet<Keyword> Keywords { get; set; }

        public DbSet<Publication> Publications { get; set; }

        public DbSet<PublicationLanguage> PublicationLanguages { get; set; }

        public DbSet<PublicationType> PublicationTypes { get; set; }

        public DbSet<Subject> Subjects { get; set; }

        public DbSet<Collection> Collections { get; set; }

        public DbSet<CollectionPublication> CollectionPublications { get; set; }

        public DbSet<University> Universities { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<PublicationRoleClaim> PublicationRoleClaims { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> dbContextOptions) : base(dbContextOptions) { }

        public ApplicationContext() : base() { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CollectionPublication>(entity =>
            {
                entity.HasKey(cp => new { cp.CollectionId, cp.PublicationId });

                entity.HasOne(cp => cp.Collection)
                    .WithMany(c => c.CollectionPublications)
                    .HasForeignKey(cp => cp.CollectionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cp => cp.Publication)
                    .WithMany(p => p.CollectionPublications)
                    .HasForeignKey(cp => cp.PublicationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PublicationRoleClaim>(entity =>
            {
                entity.HasKey(prc => new { prc.PublicationId, prc.IdentityRoleClaimId });

                entity.HasOne(prc => prc.Publication)
                    .WithMany(p => p.PublicationRoleClaims)
                    .HasForeignKey(prc => prc.PublicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(prc => prc.IdentityRoleClaim)
                    .WithMany()
                    .HasForeignKey(prc => prc.IdentityRoleClaimId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
