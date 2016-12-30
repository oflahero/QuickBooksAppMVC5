# QuickbooksAppMVC5 #

### ASP.NET MVC standard boilerplate modified just for Intuit QuickBooks OpenID and OAuth site access. ###

The bulk of this repo is simply the boilerplate project you get when you add a new MVC ASP.NET project in your Visual Studio solution.

The changes are inspired by and draw heavily from i) the 'TimeTracking' sample code already provided by Intuit for a time tracking system (by Sumod Madhavan) - see https://intuitdeveloper.github.io/ and ii) suggested code by 'RiverNut' @ Blogger (http://rivernut.blogspot.ie/2016/02/single-sign-on-with-intuit-quickbooks.html) for an OpenID addition for QuickBooks.

Where it gets useful is the addition of both an OpenID option for authenticating yourself via Intuit's OpenID (and thereby registering yourself in your local AspNetUsers table) and an implementation of the OAuth v1.0a flow used by Intuit to create access credentials linked to that user.

In summary, an OAuthProfiles table has been added to store these credentials, and each AspNetUser has a (nullable) OAuthProfileId. The [QuickbooksAuthorize] attribute initiates the OAuth authorisation flow if this ID is null for the current logged-in user.

### How can I use this? ###
If you use this project as the basis of your development, you will already have OpenID registration and OAuth authorization built into your website before you even crack your knuckles. To ensure that you are ready to call the API in a controller method, just decorate your class or method with the [QuickbooksAuthorize] attribute.

Just run the project as is, and you will see an end-to-end trivial demonstration.

#### To use this repo: ####
  i. Requirements - VS2015, LocalDB v13.0 (SQL Server 2014 version - you probably have it anyway if using VS2015)
  ii. A developer app registered with Quickbooks Online (see developer.intuit.com), and its consumer key & secret.

1. First, just open the .sln file as normal in Visual Studio. 
2. Execute the DBScripts/CreateDB.sql script in your LocalDB. I like to use SQL Server Management Studio to connect to (LocalDB)\v13.0 that way, but you can do it from the command line or 'New data connection' in VS' Server Explorer if you prefer. This will just create a minimalist 'QBAppMVC5' database locally that contains the bare Windows Identity tables plus an extra [OAuthProfiles] table to retain access credentials.
3. Create an app in the Intuit QuickBooks developer area if you haven't already. Add its consumer key and secret to web.config in the app settings' QBConsumerKey and QBConsumerSecret keys. Alter the StorageSecurityKey setting to an arbitrary string of your choice (it's just a salt for encrypting the tokens.)
4. That should do it. Try it out with F5. Use 'Log in with Intuit' on the login page and go from there.

### Limitations, caveats and suggested improvements: ###
* This is a demonstration project. Everything is lumped into the one place. In a real implementation, there would be better exception handling and general robustness, and there would be far superior separation of concerns. For example, the EF data model (.edmx) currently sits in the web project itself, where a more flexible and future-proof model would have it abstracted away in at least a DAL project, if not another layer down under a repo for more consistent access.

* The Home/CallQuickbooks controller method demonstrates the flow of calling the Quickbooks API given a logged-in user who has already been authorized via Quickbooks. In your implementation, you might not need to associate one set of OAuth credentials per user, and instead share if necessary. This is up to your own requirements.

* In any case, most of 'CallQuickbooks' would be generalised away in library classes in a real implementation. This project is intended to show off the nuts and bolts, nothing more.

* The access token credentials are stored in the DB using the same encryption method as the TimeTracking project uses. How you prefer to do this is up to you. Just remember not to store in the clear!

* A useful development of this project would be the use of claims to store the OAuth credentials, if available. Right now, you need a round trip to the DB every request to retrieve the details. Using properly-encrypted claims would solve this problem. I've included some sample starter code (commented out) in the project's ApplicationUser class.

* Don't forget - Intuit's OAuth access tokens are only good for 180 days. A great idea for a project enhancement would be an automatic token refresh at the 90-day-plus mark in the QuickbooksAuthorize attribute.

* The QBBaseURL AppSettings key currently refers to the developer sandbox. Don't forget to set this to the production version if trying out real (non-developer) apps.