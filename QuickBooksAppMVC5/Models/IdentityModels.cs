using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using QuickBooksAppMVC5.Extensions;
using System.Configuration;

namespace QuickBooksAppMVC5.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here. Ideal place to store OAuth access credentials.
            // This isn't currently used but, if determined that the claim is sufficiently encrypted, would be a superior implementation of
            // the innards of the [QuickBooksAuthorize] attribute. It would save the database roundtrip by checking the claim instead.
            if (OAuthProfileId != null)
                userIdentity.AddClaim(new Claim("OAuthProfileId", OAuthProfileId.ToString()));

            // Sample improvement claims-based code:
            /*
            if (OAuthProfileId!=null) // && check claim not added already?
            {
                var profile = await new QBAppMVC5Entities().OAuthProfiles.SingleOrDefaultAsync(p => p.Id == OAuthProfileId);
                if (profile != null)
                {
                    userIdentity.AddClaim(new Claim("OAuthProfileAccessToken", Utility.Decrypt(profile.AccessToken, ConfigurationManager.AppSettings["StorageSecurityKey"])));
                    userIdentity.AddClaim(new Claim("OAuthProfileRealmId", profile.RealmId.ToString()));
                    userIdentity.AddClaim(new Claim("OAuthProfileDatasource", profile.Datasource));
                }
            }*/

            return userIdentity;
        }

        public long? OAuthProfileId { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}