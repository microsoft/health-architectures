using System;

using Microsoft.WindowsAzure.Storage.Table;

namespace FHIRProxy
{
    public class LinkEntity : TableEntity
    {
        public LinkEntity()
        {
        }

        public LinkEntity(string resourceType, string principalId)
        {
            PartitionKey = resourceType;
            RowKey = principalId;
        }

        public string LinkedResourceId { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
