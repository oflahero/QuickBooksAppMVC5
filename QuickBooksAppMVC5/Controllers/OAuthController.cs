using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using QuickBooksAppMVC5.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuickBooksAppMVC5.Controllers
{
    [Authorize]
    public class OAuthController : Controller
    {
        public ActionResult Initiate(string returnURL)
        {
            // via directConnectToIntuit():
            Session["OAuthReturnURL"] = returnURL;
            return View();
        }

        public RedirectResult Begin()
        {
            var responseURL = Request.Url.GetLeftPart(UriPartial.Authority) + "/OAuth/Continue";

            var oAuthSession = getOAuthSession();

            try
            {
                var oAuthToken = oAuthSession.GetRequestToken();
                var responseLink = string.Format("https://appcenter.intuit.com/Connect/Begin?oauth_token={0}&oauth_callback={1}",
                    oAuthToken.Token,
                    UriUtility.UrlEncode(responseURL));
                Session["OAuthRequestToken"] = oAuthToken;
                return Redirect(responseLink);
            }
            catch (Intuit.Ipp.Exception.FaultException ex)
            {
                throw ex;
            }
            catch (Intuit.Ipp.Exception.InvalidTokenException ex)
            {
                throw ex;
            }
            catch (Intuit.Ipp.Exception.SdkException ex)
            {
                throw ex;
            }
        }

        public ActionResult Continue()
        {
            var oAuthVerifier = Request.QueryString["oauth_verifier"];
            var realmId = Convert.ToInt64(Request.QueryString["realmId"]);
            var oAuthDataSource = Request.QueryString["dataSource"];

            string sAccessToken, sAccessTokenSecret;

            try
            {
                var oAuthSession = getOAuthSession();

                IToken accessToken = oAuthSession.ExchangeRequestTokenForAccessToken((IToken)Session["OAuthRequestToken"], oAuthVerifier);
                sAccessToken = accessToken.Token;
                sAccessTokenSecret = accessToken.TokenSecret;

                Session["OAuthRequestToken"] = null;
            }
            catch (Intuit.Ipp.Exception.FaultException ex)
            {
                throw ex;
            }
            catch (Intuit.Ipp.Exception.InvalidTokenException ex)
            {
                throw ex;
            }
            catch (Intuit.Ipp.Exception.SdkException ex)
            {
                throw ex;
            }

            if (sAccessToken!=null && sAccessTokenSecret!=null)
            {
                using (var ctx = new QBAppMVC5Entities())
                {
                    var newProfile = new OAuthProfile()
                    {
                        AccessToken = Utility.Encrypt(sAccessToken, ConfigurationManager.AppSettings["StorageSecurityKey"]),
                        AccessSecret = Utility.Encrypt(sAccessTokenSecret, ConfigurationManager.AppSettings["StorageSecurityKey"]),
                        Datasource = oAuthDataSource,
                        RealmId = realmId
                    };

                    ctx.OAuthProfiles.Add(newProfile);

                    var currentUser = ctx.AspNetUsers.SingleOrDefault(u => u.UserName == User.Identity.Name);

                    if (currentUser != null) currentUser.OAuthProfile = newProfile;

                    ctx.SaveChanges();
                }
            }
            // Save...

            if (Session["OAuthReturnURL"] != null)
            {
                var sReturnURL = (string)Session["OAuthReturnURL"];
                Session["OAuthReturnURL"] = null;
                return Redirect(sReturnURL);
            }

            return View();
        }

        OAuthSession getOAuthSession()
        {
            OAuthConsumerContext oAuthConsumerContext = new OAuthConsumerContext
            {
                ConsumerKey = ConfigurationManager.AppSettings["QBConsumerKey"],
                ConsumerSecret = ConfigurationManager.AppSettings["QBConsumerSecret"],
                SignatureMethod = SignatureMethod.HmacSha1
            };

           return new OAuthSession(oAuthConsumerContext,
                                        "https://oauth.intuit.com/oauth/v1/get_request_token",
                                        "https://oauth.intuit.com/oauth/v1",
                                       "https://oauth.intuit.com/oauth/v1//get_access_token");
        }
    }
}