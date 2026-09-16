using Xunit;

namespace VoteBattle.IntegrationTests.Infrastructure;

[CollectionDefinition("integration")]
public class IntegrationCollection : ICollectionFixture<VoteBattleWebAppFactory>
{
}
