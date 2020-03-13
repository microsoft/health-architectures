using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FHIRProxy
{
    public class Utils
    {
        public static string genOOErrResponse(string code,string desc)
        {

            return $"{{\"resourceType\": \"OperationOutcome\",\"id\": \"{Guid.NewGuid().ToString()}\",\"issue\": [{{\"severity\": \"error\",\"code\": \"{code ?? ""}\",\"diagnostics\": \"{desc ?? ""}\"}}]}}";

        }
    }
}
