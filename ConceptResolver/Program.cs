using System.IO;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ConceptResolver.Model;

namespace ConceptResolver
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .Build();

            host.Services.GetRequiredService<Resolver>().Dump();
            await host.Services.GetRequiredService<Application>().Run();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddScoped<IProvider, PatientProvider>();
            services.AddScoped<IProvider, LabResultProvider>();

            services.AddScoped<Resolver>();

            services.AddTransient<Application>();
        }
    }

    internal class Application
    {
        public Application(Resolver resolver)
        {
            this.resolver = resolver;
        }

        public async Task Run()
        {
            await HandleRequest(@"C:\Git\ConceptResolver\ConceptResolver\bin\Debug\request.xml");
        }

        public async Task HandleRequest(string filename)
        {
            // test a sample replacement
            var session = new Session
            {
                PatientId = 1111,
                UserId = 9999
            };

            var template = new XmlDocument();
            template.Load(filename);

            var output = template.Clone() as XmlDocument;
            resolver.Replace(session, output.DocumentElement);

            output.Save(Path.ChangeExtension(filename, ".out.xml"));
        }

        private readonly Resolver resolver;
    }
}
