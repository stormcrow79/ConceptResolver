using System;
using System.IO;
using System.Xml;

using ConceptResolver.Model;

namespace ConceptResolver
{
    internal class Program
    {
        static Resolver _resolver;

        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("usage: ReflectionText.exe <xml_filename>");
                return;
            }

            // set up & test the resolver
            _resolver = new Resolver();
            _resolver.Dump();

            // test a sample replacement
            var template = new XmlDocument();
            template.Load(args[0]);

            var output = template.Clone() as XmlDocument;
            _resolver.Replace(output.DocumentElement);

            output.Save(Path.ChangeExtension(args[0], ".out.xml"));
        }
    }
}
