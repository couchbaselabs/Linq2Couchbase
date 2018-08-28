The BucketContext
=================
The public API for Linq2Couchbase is the BucketContext; this object is similar to the DbContext in Linq2SQL and the DataContext from the EntityFramework. It's primary purpose is to provide and interface for building and submitting queries to a Couchbase server Bucket. Internally, the BucketContext uses a Cluster object and CouchbaseBucket to handle communication and to send queries and updates to the server. 

## Creating a BucketContext##
The BucketContext has a dependency on the Cluster object; in your application you will need to have instantiated and initialized a Cluster object before you can create a BucketContext. 

    using (var cluster = new Cluster())
    {
		using(var bucket = cluster.OpenBucket("travel-sample"))
		{
        	var context = new BucketContext(bucket);
		}
    }

It's important to note that the Cluster object is a long-lived object, so you will want to create a single (usually) per application and reuse it over the lifespan of the application. A BucketContext is slightly different; it contains no Dispose method and is more ephemeral compared to the Cluster. So a better way to manage the Cluster is by using the ClusterHelper and initializing it when the application starts up and closing it when the application is torn down.

### Using the BucketContext in a Web Application###
As previously mentioned, the Cluster object should be globally scoped to the application. In an ASP.NET application, there are two places this can be done: in the Global.asax and in the Startup.cs class if the application is an Ownin/Katana application.

For example by using the Global.asax:

	public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Initialize the helper
            var config = new ClientConfiguration();
            ClusterHelper.Initialize(config);
        }

        protected void Application_End()
        {
            //Cleanup all resources
            ClusterHelper.Close();
        }
    }

In a Owin/Katana, the initialization and destruction would look like this:

	public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            //initialize the ClusterHelper
            ClusterHelper.Initialize(new ClientConfiguration
            {
                Servers = new List<Uri>
                {
                    new Uri("http://localhost:8091/")
                }
            });

            //Register a callback that will dispose of the ClusterHelper on app shutdown
            var properties = new AppProperties(app.Properties);
            var token = properties.OnAppDisposing;
            if (token != CancellationToken.None)
            {
                token.Register(() =>
                {
                    ClusterHelper.Close();
                });
            }
        }
    }

Note on the both examples, we are using the ClusterHelper which will maintain a reference to the Cluster object, which will later be used to create the BucketContext on each request.

Once you used one of the methods above to handle the initiation/destruction of the Cluster (via the ClusterHelper), you will then inject it into your Controllers and use them in your Action methods:

    public class BeerController : Controller
    {
        private readonly IBucketContext _context;

        public BeerController(IBucketContext context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            var beers = from b in _context.Query<Beer>()
                where b.Name == "21st Amendment Brewery Cafe"
                select b;

            return View(beers);
        }

		...
    }

Using this pattern, the IBucket will be reused in every request as long as you do not close or dispose of it.

### Configuration###
Configuration is done when the Cluster object is created using the ClientConfiguration class. You do this again either when you create the Cluster object or when you initialize the ClusterHelper, for example:

    var config = new ClientConfiguration();
    ClusterHelper.Initialize(config);

The default configuration is to use localhost, so it assumes you are bootstrapping to a local instance of Couchbase server. The most common change to the default configuration would be to change the bootstrapping node to a remote host:

    var config = new ClientConfiguration
	{
		new Servers
		{
			new Uri("http://192.168.77.101:8091/")
		}
	};
    ClusterHelper.Initialize(config);

Note, if you are using the Cluster class directly, you will pass the configuration into the constructor or via the App.Config. You can read more about configuration [here](http://developer.couchbase.com/documentation/server/4.0/sdks/dotnet-2.2/configuring-the-client.html).


