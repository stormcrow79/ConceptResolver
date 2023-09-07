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
        public Resolver(IEnumerable<IProvider> providers)
        {
            foreach (var provider in providers)
            {
                var providerType = provider.GetType();
                if (Implements(providerType, typeof(IProvider<>)))
                {
                    var model = providerType.GetInterface(typeof(IProvider<>).Name).GetGenericArguments()[0];

                    var concepts = model.GetProperties()
                        .Select(p => p.GetCustomAttribute<ConceptAttribute>())
                        .Where(a => a != null);

                    foreach (var concept in concepts)
                        providerLookup.Add(concept.Name, provider);
                }
                else if (Implements(providerType, typeof(ICollectionProvider<>)))
                {
                    var concept = providerType.GetCustomAttribute<ConceptAttribute>();
                    if (concept != null)
                        providerLookup.Add(concept.Name, provider);
                }
            }
        }

        public object GetProvider(string conceptName)
        {
            if (providerLookup.TryGetValue(conceptName, out var provider))
                return provider;
            return null;
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
            foreach (var entry in providerLookup)
                Console.WriteLine($"{entry.Key}: {entry.Value.GetType().Name}");
        }

        private Dictionary<string, object> providerLookup { get; } = new Dictionary<string, object>();
    }
}
