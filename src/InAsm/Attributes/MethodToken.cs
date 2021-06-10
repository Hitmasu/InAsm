using System;

namespace InAsm.Attributes
{
    internal class MethodToken : Attribute
    {
        public int MetadataToken { get; }

        public MethodToken(int metadataToken)
        {
            MetadataToken = metadataToken;
        }
    }
}