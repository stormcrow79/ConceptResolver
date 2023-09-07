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

    public interface IProvider<TModel>
    {
        TModel Get(Session session);
    }

    public interface ICollectionProvider<TModel>
    {
        IEnumerable<TModel> Get(Session session, Filter filter);
    }
}
