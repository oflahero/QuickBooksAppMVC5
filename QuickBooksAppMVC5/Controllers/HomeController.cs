using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using QuickBooksAppMVC5.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuickBooksAppMVC5.Controllers
{
    [QuickBooksAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        // A very basic call (well, 3 calls) to the QB API to find customers with an 'e' somewhere in their name.
        // Lacks basic robustness (proper exception handling), and obviously the OAuth setup should be abstracted away in (at least) a helper class,
        // if not a full repository/data layer for API access.
        //
        // A non-trivial implementation would also hive the .edmx (data model) away in a separate DAL.
        public ActionResult CallQuickbooks()
        {
            var exampleResult = "Authorisation problem.";

            Models.ApplicationUser userModel = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>()
                    .FindById(User.Identity.GetUserId());

            var oauthProfile = new QBAppMVC5Entities().OAuthProfiles.SingleOrDefault(p => p.Id == userModel.OAuthProfileId);
            if (oauthProfile != null) // 'Guaranteed' given that [QuickBooksAuthorize] ensures a non-null userModel.OAuthProfileId.
            {
                try
                {
                    var oAuthRequestValidator = new OAuthRequestValidator( // this is an Intuit object
                        Utility.Decrypt(oauthProfile.AccessToken, ConfigurationManager.AppSettings["StorageSecurityKey"]),
                        Utility.Decrypt(oauthProfile.AccessSecret, ConfigurationManager.AppSettings["StorageSecurityKey"]),
                        ConfigurationManager.AppSettings["QBConsumerKey"],
                        ConfigurationManager.AppSettings["QBConsumerSecret"]);
                    var serviceContext = new ServiceContext(oauthProfile.RealmId.ToString(), IntuitServicesType.QBO, oAuthRequestValidator);

                    QueryService<Customer> queryService = new QueryService<Customer>(serviceContext);

                    var dummyMatch = "e";
                    var cs = queryService.ExecuteIdsQuery($"select * from customer where displayname like '%{dummyMatch}%'");
                    var cs2 = queryService.ExecuteIdsQuery($"select * from customer where givenname like '%{dummyMatch}%'");
                    var cs3 = queryService.ExecuteIdsQuery($"select * from customer where familyname like '%{dummyMatch}%'");

                    var allCs = cs.Concat(cs2).Concat(cs3).GroupBy(c => c.Id).Select(g => g.First());
                    var top5cs = allCs.Take(5);
                    exampleResult = "No customers found with 'e' in their names!";
                    if (top5cs.Count() > 0)
                    {
                        exampleResult = string.Empty;
                        foreach (var c in top5cs)
                        {
                            exampleResult += $"{c.DisplayName} ({c.GivenName} {c.FamilyName});";
                        }
                        if (exampleResult.Length > 0)
                            exampleResult = "Max 5 customers found with 'e' in their name: " + exampleResult;

                        if (exampleResult.EndsWith(";"))
                            exampleResult = exampleResult.Substring(0, exampleResult.Length - 1);
                    }
                }
                catch (Exception qbEx)
                {
                    exampleResult = "Exception encountered while calling the Quickbooks API: "+qbEx.ToString();
                }
            }

            return View(model: exampleResult);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}