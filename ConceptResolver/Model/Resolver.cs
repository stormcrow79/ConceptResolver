using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace ConceptResolver.Model
{
    public class Resolver
    {
        public Resolver()
        {
            var singletonProviders = typeof(Program).Assembly.GetTypes()
                .Where(t => t.GetInterface("IProvider`1") != null)
                .ToArray();

            foreach (var providerType in singletonProviders)
            {
                var model = providerType.GetInterface("IProvider`1")
                    .GetGenericArguments()[0];

                var concepts = model.GetProperties()
                    .Select(p => p.GetCustomAttribute<ConceptAttribute>())
                    .Where(a => a != null);

                foreach (var concept in concepts)
                    providerTypes.Add(concept.Name, providerType);
            }

            var collectionProviders = typeof(Program).Assembly.GetTypes()
                .Where(t => t.GetInterface("ICollectionProvider`1") != null)
                .ToArray();

            foreach (var providerType in collectionProviders)
            {
                var concept = providerType.GetCustomAttribute<ConceptAttribute>();
                if (concept != null)
                    providerTypes.Add(concept.Name, providerType);
            }
        }

        public object GetProvider(string conceptName)
        {
            if (!providerTypes.TryGetValue(conceptName, out var providerType))
                return null;

            if (providers.TryGetValue(providerType, out var providerInstance))
                return providerInstance;

            providerInstance = providerType.GetConstructor(new Type[0]).Invoke(new object[0]);
            providers.Add(providerType, providerInstance);
            return providerInstance;
        }

        public Filter ParseFilter(XmlElement element)
        {
            var filter = new Filter();

            var minDateTime = element.GetAttributeNode("minDateTime");
            if (minDateTime != null)
                filter.MinDateTime = DateTime.ParseExact(minDateTime.InnerText, "yyyy-MM-ddTHH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);

            return filter;
        }

        public void Replace(Session session, XmlElement element)
        {
            var conceptName = element.GetAttribute("conceptName");
            var provider = GetProvider(conceptName);
            if (provider != null)
            {
                var providerType = provider.GetType();

                if (Implements(providerType, typeof(IProvider<>)))
                {
                    var model = providerType.GetMethod("Get").Invoke(provider, new object[] { session });
                    element.InnerText = GetConceptValue(model, conceptName).ToString();
                }

                if (Implements(providerType, typeof(ICollectionProvider<>)))
                {
                    // concept is satisfied by a collection provider
                    var filter = ParseFilter(element);

                    // clone the XmlElement for each member of the collection and
                    // process those recursively, but with using the specified member
                    foreach (var value in providerType.GetMethod("Get").Invoke(provider, new object[] { session, filter }) as IEnumerable)
                    {
                        var clone = element.ParentNode.AppendChild(element.Clone()) as XmlElement;
                        // TODO: process children
                    }

                    element.ParentNode.RemoveChild(element);
                }
            }

            foreach (var child in element.ChildNodes.OfType<XmlElement>())
                Replace(session, child);
        }

        public object GetConceptValue(object model, string conceptName) => model.GetType().GetProperties()
            .FirstOrDefault(p => p.GetCustomAttribute<ConceptAttribute>()?.Name == conceptName)?
            .GetValue(model);

        public bool Implements(Type concreteType, Type interfaceType) => concreteType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);

        public void Dump()
        {
            foreach (var entry in providerTypes)
                Console.WriteLine($"{entry.Key}: {entry.Value.Name}");
        }

        private Dictionary<string, Type> providerTypes { get; } = new Dictionary<string, Type>();
        private Dictionary<Type, object> providers { get; } = new Dictionary<Type, object>();
    }
}
