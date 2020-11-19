// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Utilities
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using H3.Core.Domain;

    public class GuidFactory : IGuidFactory
    {
        public Guid Create(params string[] values)
        {
            var stringBuilder = new StringBuilder();

            foreach (var value in values)
            {
                stringBuilder.Append(value);
            }

            var provider = new MD5CryptoServiceProvider();

            var hash = provider.ComputeHash(Encoding.ASCII.GetBytes(stringBuilder.ToString()));

            return new Guid(hash);
        }
    }
}
