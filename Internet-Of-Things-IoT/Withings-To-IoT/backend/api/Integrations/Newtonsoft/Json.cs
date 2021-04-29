// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Newtonsoft
{
    using global::Newtonsoft.Json;
    using H3.Core.Domain;

    public class Json : IJson
    {
        public string Dump<T>(T value)
            where T : class
        {
            return JsonConvert.SerializeObject(value);
        }

        public T Load<T>(string value)
            where T : class
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
