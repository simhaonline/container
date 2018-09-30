﻿using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.ObjectBuilder.BuildPlan.Selection;
using Unity.Policy;
using Unity.ResolverPolicy;

namespace Unity.ObjectBuilder.Policies
{
    /// <summary>
    /// An implementation of <see cref="IConstructorSelectorPolicy"/> that is
    /// aware of the build keys used by the Unity container.
    /// </summary>
    public class DefaultUnityConstructorSelectorPolicy : ConstructorSelectorPolicyBase<InjectionConstructorAttribute>
    {
        /// <summary>
        /// Create a <see cref="IResolverPolicy"/> instance for the given
        /// <see cref="ParameterInfo"/>.
        /// </summary>
        /// <remarks>
        /// This implementation looks for the Unity <see cref="DependencyAttribute"/> on the
        /// parameter and uses it to create an instance of <see cref="NamedTypeDependencyResolverPolicy"/>
        /// for this parameter.</remarks>
        /// <param name="parameter">Parameter to create the resolver for.</param>
        /// <returns>The resolver object.</returns>
        protected override IResolverPolicy CreateResolver(ParameterInfo parameter)
        {
            // Resolve all DependencyAttributes on this parameter, if any
            var attributes = parameter.GetCustomAttributes(false)
                                      .OfType<DependencyResolutionAttribute>()
                                      .ToArray();
            if (attributes.Length > 0)
            {
                // Since this attribute is defined with MultipleUse = false, the compiler will
                // enforce at most one. So we don't need to check for more.
                var attr = attributes[0];
                return attr is OptionalDependencyAttribute dependencyAttribute
                    ? (IResolverPolicy) new OptionalDependencyResolverPolicy(parameter.ParameterType, dependencyAttribute.Name)
                    : new NamedTypeDependencyResolverPolicy(parameter.ParameterType, attr.Name);
            }

            // No attribute, just go back to the container for the default for that type.
            return new NamedTypeDependencyResolverPolicy(parameter.ParameterType, null);
        }
    }
}