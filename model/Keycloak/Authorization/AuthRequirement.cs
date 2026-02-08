using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keycloak.Authorization
{
    public class AuthRequirement : IAuthorizationRequirement, IEquatable<AuthRequirement>
    {
        public string[] RequiredRoles { get; }

        public AuthRequirement(params string[] roles)
        {
            RequiredRoles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        public bool Equals(AuthRequirement? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return RequiredRoles.SequenceEqual(other.RequiredRoles);
        }

        public override bool Equals(object? obj)
        {
            return obj is AuthRequirement requirement && Equals(requirement);
        }

        public override int GetHashCode()
        {
            return RequiredRoles.Aggregate(0, (hash, role) => 
                HashCode.Combine(hash, role.GetHashCode()));
        }
    }
}
