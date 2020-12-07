// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Utilities
{
    using System;
    using H3.Core.Domain;
    using H3.Core.Models.Api;

    public class EnvironmentSettings : ISettings
    {
        public string GetSetting(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);

            if (value == null)
            {
                throw new MissingSettingException
                {
                    Key = key,
                };
            }

            return value;
        }
    }
}
