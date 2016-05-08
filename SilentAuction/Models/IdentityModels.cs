using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace SilentAuction.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

		/// <summary>
		/// Preferences selected by the user which override default behaviours
		/// </summary>
		public ICollection<ConfigurationParameter> Preferences { get; set; }

        /// <summary>
        /// Payments which an admin has logged as being made
        /// </summary>
        public ICollection<Payment> PaymentsLogged { get; set; }

        /// <summary>
        /// Payments which this user has made
        /// </summary>
        public ICollection<Payment> PaymentsMade { get; set; }

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
		public DbSet<ConfigurationParameter> SystemParameters { get; set; }
		public DbSet<Auction> Auctions { get; set; }
		public DbSet<Lot> Lots { get; set; }
		public DbSet<ImageFile> ImageFiles { get; set; }
		public DbSet<LotImageFile> LotImageFiles { get; set; }
        public DbSet<Locale> Locales { get; set; }
        public DbSet<Payment> Payments { get; set; }    // just to force into the data model. Shouldn't ever be actually loaded.

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // bid requires a lot
            modelBuilder
                .Entity<Bid>()
                .HasRequired(a => a.Lot)    // Bid requires lot
                .WithMany(a => a.Bids);     // Lot may have many bids

            // lot MAY have a last bid
            modelBuilder
                .Entity<Lot>()                      // The lot entity...
                .HasOptional(b => b.HighestBid);   // MAY have a highest bid (or no bids at all)


            // Turn off cascading deletes for the table with 2 FKs to the User table
            // otherwise the Entity model doesn't create correctly.
            // Payments logged
            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.LoggedByUser)
                .WithMany(p => p.PaymentsLogged)
                .HasForeignKey(p => p.LoggedBy)
                .WillCascadeOnDelete(false);

            // Payments made
            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.User)
                .WithMany(p => p.PaymentsMade)
                .HasForeignKey(p => p.LoggedBy)
                .WillCascadeOnDelete(false);
                


            base.OnModelCreating(modelBuilder);
        }


    }
}