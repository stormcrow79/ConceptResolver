using System;
using System.Collections.Generic;

namespace ConceptResolver.Model
{
    public class ConceptAttribute : Attribute
    {
        public string Name { get; set; }

        public ConceptAttribute(string name)
        {
            Name = name;
        }
    }

    public class Session
    {
        public int PatientId { get; set; }
        public int EncounterId { get; set; }
        public int UserId { get; set; }
    }

    public class Filter
    {
        public DateTime? MinDateTime { get; set; }
    }

    public interface IProvider<TModel> : IProvider
    {
        TModel Get(Session session);
    }

    public interface ICollectionProvider<TModel> : IProvider
    {
        IEnumerable<TModel> Get(Session session, Filter filter);
    }

    public interface IProvider { }
}
