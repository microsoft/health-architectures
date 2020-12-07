// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using H3.Core.Models.Fhir;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class implements FHIR data access via the Azure API for FHIR.
    /// <a href="https://docs.microsoft.com/en-us/azure/healthcare-apis/overview">See documentation.</a>
    /// </summary>
    /// <remarks>
    /// The implementation of this class assumes that the execution environment has been granted
    /// the <em>FHIR Data Contributor</em> role for the targetted Azure API for FHIR resource.
    /// </remarks>
    public class FhirApiClient : IFhirClient
    {
        private readonly ILogger log;
        private readonly IAccessTokenSource managedIdentity;
        private readonly IHttp http;
        private readonly IJson json;
        private readonly string fhirUrl;

        public FhirApiClient(
            ILoggerFactory log,
            IAccessTokenSource managedIdentity,
            IHttp http,
            IJson json,
            ISettings settings)
        {
            this.log = log.CreateLogger<FhirApiClient>();
            this.managedIdentity = managedIdentity;
            this.http = http;
            this.json = json;
            this.fhirUrl = settings.GetSetting("FHIR_SERVER_URL");
        }

        public async Task<IReadOnlyCollection<Observation>> FetchObservations(string userId, string fhirUserId, DateTimeOffset? after, DateTimeOffset? before, CancellationToken cancellationToken)
        {
            var url = $"{fhirUrl}/Patient/{fhirUserId}/Observation?";

            if (after != null)
            {
                url += string.Format("&date=gt{0:yyyy-MM-ddTHH:mm:ss}Z", after);
            }

            if (before != null)
            {
                url += string.Format("&date=lt{0:yyyy-MM-ddTHH:mm:ss}Z", before);
            }

            return await Fetch<Observation>(url, userId, cancellationToken);
        }

        public async Task<IReadOnlyCollection<Observation>> CreateObservations(string userId, IReadOnlyCollection<Observation> observations, CancellationToken cancellationToken)
        {
            return await Create(userId, observations, cancellationToken);
        }

        public async Task<Patient> CreatePatient(string userId, string? familyName, string? givenName, CancellationToken cancellationToken)
        {
            var patients = new[]
            {
                new Patient
                {
                    ResourceType = "Patient",
                    Names = familyName != null && givenName != null
                        ? new[]
                        {
                            new Name
                            {
                                FamilyName = familyName,
                                GivenNames = new[]
                                {
                                    givenName,
                                },
                            },
                        }
                        : Array.Empty<Name>(),
                },
            };

            var created = await Create(userId, patients, cancellationToken);

            return created.FirstOrDefault();
        }

        public async Task DeleteObservations(string userId, string fhirUserId, Func<Observation, bool> shouldDelete, CancellationToken cancellationToken)
        {
            var observations = await FetchObservations(userId, fhirUserId, after: null, before: null, cancellationToken);

            var observationsToDelete = observations.Where(shouldDelete).ToArray();

            var deleted = await Delete(userId, observationsToDelete, cancellationToken);
        }

        public async Task DeletePatient(string userId, string fhirUserId, CancellationToken cancellationToken)
        {
            var patient = new[]
            {
                new Patient
                {
                    ResourceType = "Patient",
                    Id = fhirUserId,
                },
            };

            var deleted = await Delete(userId, patient, cancellationToken);
        }

        private async Task<IReadOnlyCollection<T>> Delete<T>(string userId, IReadOnlyCollection<T> items, CancellationToken cancellationToken)
            where T : class, IHasId
        {
            if (items.Count == 0)
            {
                return Array.Empty<T>();
            }

            var bundle = new Bundle<T>
            {
                ResourceType = "Bundle",
                Type = "batch",
                Entries = items.Select(item => new BundleEntry<T>
                {
                    Request = new BundleRequest
                    {
                        Method = "DELETE",
                        Url = $"{item.ResourceType}/{item.Id}?hardDelete=true",
                    },
                }).ToArray(),
            };

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{fhirUrl}/"),
                Method = HttpMethod.Post,
                Content = new StringContent(json.Dump(bundle), Encoding.UTF8, "application/json"),
            };

            await AddHeaders(request, userId, cancellationToken);

            log.LogInformation("Deleting bundle with {count} items for user {userId}", bundle.Entries.Length, userId);

            var deletedBundle = await http.Send<Bundle<T>, FhirApiException>(request, cancellationToken);

            var deletedItems = deletedBundle.Entries.Zip(items)
                .Where(t => t.First.Response.Status == "204" && t.First.Response.ETag != null)
                .Select(t => t.Second)
                .ToArray();

            log.LogInformation("Deleted {count} items in bundle for user {userId}", deletedItems.Length, userId);

            return deletedItems;
        }

        private async Task<IReadOnlyCollection<T>> Create<T>(string userId, IReadOnlyCollection<T> items, CancellationToken cancellationToken)
            where T : class, IHasId
        {
            if (items.Count == 0)
            {
                return Array.Empty<T>();
            }

            var bundle = new Bundle<T>
            {
                ResourceType = "Bundle",
                Type = "batch",
                Entries = items.Select(item => new BundleEntry<T>
                {
                    Resource = item,
                    Request = item.Id == null
                        ? new BundleRequest
                        {
                            Method = "POST",
                            Url = item.ResourceType,
                        }
                        : new BundleRequest
                        {
                            Method = "PUT",
                            Url = $"{item.ResourceType}/{item.Id}",
                        },
                }).ToArray(),
            };

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{fhirUrl}/"),
                Method = HttpMethod.Post,
                Content = new StringContent(json.Dump(bundle), Encoding.UTF8, "application/json"),
            };

            await AddHeaders(request, userId, cancellationToken);

            log.LogInformation("Creating bundle with {count} items for user {userId}", bundle.Entries.Length, userId);

            var createdBundle = await http.Send<Bundle<T>, FhirApiException>(request, cancellationToken);

            var createdItems = createdBundle.Entries
                .Where(entry => entry.Response.Status == "201")
                .Select(entry => entry.Resource)
                .ToArray();

            log.LogInformation("Created {count} items in bundle for user {userId}", createdItems.Length, userId);

            return createdItems;
        }

        private async Task<IReadOnlyCollection<T>> Fetch<T>(string? endpoint, string userId, CancellationToken cancellationToken)
            where T : class, IHasId
        {
            var resources = new List<T>();

            while (endpoint != null)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                await AddHeaders(request, userId, cancellationToken);

                log.LogInformation("Making request to {uri}", request.RequestUri);
                var fhirResponse = await http.Send<Bundle<T>, FhirApiException>(request, cancellationToken);

                if (fhirResponse.Entries.Length > 0)
                {
                    log.LogInformation("Got page with {count} results", fhirResponse.Entries.Length);

                    resources.AddRange(fhirResponse.Entries.Select(entry => entry.Resource));

                    endpoint = fhirResponse.Links.FirstOrDefault(link => link.Relation == "next")?.Url;
                }
                else
                {
                    log.LogInformation("Got no results");

                    endpoint = null;
                }
            }

            return resources;
        }

        private async Task AddHeaders(HttpRequestMessage request, string userId, CancellationToken cancellationToken)
        {
            log.LogInformation("Obtaining new access token for {resource}", fhirUrl);
            var accessToken = await managedIdentity.FetchToken(fhirUrl, cancellationToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/fhir+json"));
            request.Headers.Add("X-MS-AZUREFHIR-AUDIT-USERID", userId);
        }
    }
}
