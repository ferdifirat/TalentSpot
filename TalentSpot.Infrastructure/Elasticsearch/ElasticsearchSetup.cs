using Microsoft.EntityFrameworkCore;
using Nest;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Data;

namespace TalentSpot.Infrastructure.ElasticSearch
{
    public class ElasticsearchSetup
    {
        private readonly IElasticClient _elasticClient;

        public ElasticsearchSetup(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task EnsureIndexCreatedAsync()
        {
            var indexExists = await _elasticClient.Indices.ExistsAsync("jobs");
            if (!indexExists.Exists)
            {
                var createIndexResponse = await _elasticClient.Indices.CreateAsync("jobs", c => c
                    .Map<JobDocument>(m => m
                        .Properties(p => p
                            .Keyword(k => k.Name(n => n.Id))
                            .Text(t => t.Name(n => n.Position))
                        )
                    )
                );

                if (!createIndexResponse.IsValid)
                {
                    var debugInfo = createIndexResponse.DebugInformation;
                    var error = createIndexResponse.ServerError.Error;
                }
            }
        }
    }

    public class JobDocument
    {
        public string Id { get; set; }
        public string Position { get; set; }
        public string Description { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int Salary { get; set; }
        public string CompanyId { get; set; }
    }

}

