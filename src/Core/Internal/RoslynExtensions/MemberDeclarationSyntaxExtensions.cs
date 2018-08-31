﻿using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fettle.Core.Internal.RoslynExtensions
{
    internal static class MemberDeclarationSyntaxExtensions
    {
        public static bool CanInstrument(this MemberDeclarationSyntax memberDeclaration)
        {
            if (memberDeclaration is MethodDeclarationSyntax methodDeclaration)
            {
                return CanInstrumentMethod(methodDeclaration);
            }
            else if (memberDeclaration is PropertyDeclarationSyntax propertyDeclaration)
            {
                return CanInstrumentProperty(propertyDeclaration);
            }

            return false;
        }

        private static bool CanInstrumentMethod(MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.Body != null || methodDeclaration.ExpressionBody != null;
        }

        private static bool CanInstrumentProperty(PropertyDeclarationSyntax propertyDeclaration)
        {
            var hasAccessors = propertyDeclaration.AccessorList != null;
            if (hasAccessors)
            {
                var accessors = propertyDeclaration.AccessorList.ChildNodes().OfType<AccessorDeclarationSyntax>();
                var getter = accessors.Single(a => a.Kind() == SyntaxKind.GetAccessorDeclaration);

                var getterIsAutoAccessor = getter.Body == null && getter.ExpressionBody == null;

                // Assumption: you can't mix auto accessors and non-auto accessors in a single property.
                // E.g. these won't compile:
                //      int Thing { get; set => x = value; }
                //      int Thing { get { return x; } set; }
                // Therefore, if the getter has a body/expression body then the setter will too (and vice versa).
                // Therefore we only need to check one.

                if (getterIsAutoAccessor)
                {
                    // Auto-accessor means that there is no body/expression body, so nothing to mutate
                    return false;
                }
            }

            return true;
        }
    }
}