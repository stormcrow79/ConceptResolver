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

    public class Filter
    {
        public DateTime? MinDateTime { get; set; }
    }

    public interface IProvider<TModel>
    {
        TModel Get();
    }

    public interface ICollectionProvider<TModel>
    {
        IEnumerable<TModel> Get(Filter filter);
    }
}
