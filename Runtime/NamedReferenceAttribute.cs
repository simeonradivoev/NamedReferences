using UnityEngine;

namespace NamedReferences.Attributes
{
    public class NamedReferenceAttribute : PropertyAttribute
    {
        public string Name { get; }
        
        public bool IsDirect { get; set; }

        public NamedReferenceAttribute(string name)
        {
            Name = name;
        }
        
        public NamedReferenceAttribute(string name, bool isDirect)
        {
            Name = name;
            IsDirect = isDirect;
        }
    }
}