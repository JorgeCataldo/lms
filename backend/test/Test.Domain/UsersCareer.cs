using Domain.Aggregates.UsersCareer;
using Domain.Aggregates.UsersCareer.Commands;
using Domain.Aggregates.UsersCareer.Queries;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Test.Domain
{
    public class UsersCareer : IClassFixture<ContainerFixture>
    {
        private readonly ContainerFixture _fixture;

        public UsersCareer(ContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldBlockUpdateUserCareerInfo()
        {
            var career = UserCareer.Create(
                ObjectId.GenerateNewId()
            ).Data;

            await ContainerFixture.ExecuteDbContextAsync(async dbContext =>
            {
                var value = await dbContext.UserCareerCollection.EstimatedDocumentCountAsync();
                Assert.True(value > 0);
            });
        }
    }
}
