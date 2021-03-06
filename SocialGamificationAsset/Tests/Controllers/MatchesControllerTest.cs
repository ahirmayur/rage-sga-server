﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using SocialGamificationAsset.Middlewares;
using SocialGamificationAsset.Models;

using Xunit;

namespace SocialGamificationAsset.Tests.Controllers
{
    public class MatchesControllerTest : ControllerTest
    {
        [Fact]
        public async Task GetMyMatchesWithoutSession()
        {
            using (var client = _server.AcceptJson())
            {
                // Get my matches without session header
                var matchesResponse = await client.GetAsync($"/api/matches");
                Assert.Equal(HttpStatusCode.Unauthorized, matchesResponse.StatusCode);

                var fetched = await matchesResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No {SessionAuthorizeFilter.SessionHeaderName} Header found.", fetched.Error);
            }
        }

        [Fact]
        public async Task GetMyMatchesWithInvalidSession()
        {
            var sessionId = "unknown";

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(sessionId);

                // Get my matches with invalid session header
                var matchesResponse = await client.GetAsync("/api/matches");
                Assert.Equal(HttpStatusCode.Unauthorized, matchesResponse.StatusCode);

                var fetched = await matchesResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"Invalid {SessionAuthorizeFilter.SessionHeaderName} Header.", fetched.Error);
            }
        }

        [Fact]
        public async Task GetMyMatchesWithNonExistingSession()
        {
            var sessionId = Guid.NewGuid();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(sessionId);

                // get my matches with non-existing session header
                var matchesResponse = await client.GetAsync("/api/matches");
                Assert.Equal(HttpStatusCode.NotFound, matchesResponse.StatusCode);

                var fetched = await matchesResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"Session {sessionId} is Invalid.", fetched.Error);
            }
        }

        [Fact]
        public async Task GetOngoingMatchesWithoutSession()
        {
            using (var client = _server.AcceptJson())
            {
                // Get ongoing matches without session header
                var matchesResponse = await client.GetAsync($"/api/matches/ongoing");
                Assert.Equal(HttpStatusCode.Unauthorized, matchesResponse.StatusCode);

                var fetched = await matchesResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No {SessionAuthorizeFilter.SessionHeaderName} Header found.", fetched.Error);
            }
        }

        [Fact]
        public async Task GetOngoingMatchesWithInvalidSession()
        {
            var sessionId = "unknown";

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(sessionId);

                // Get ongoing matches with invalid session header
                var matchesResponse = await client.GetAsync("/api/matches/ongoing");
                Assert.Equal(HttpStatusCode.Unauthorized, matchesResponse.StatusCode);

                var fetched = await matchesResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"Invalid {SessionAuthorizeFilter.SessionHeaderName} Header.", fetched.Error);
            }
        }

        [Fact]
        public async Task GetOngoingMatchesWithNonExistingSession()
        {
            var sessionId = Guid.NewGuid();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(sessionId.ToString());

                // get ongoing matches with non-existing session header
                var matchesResponse = await client.GetAsync("/api/matches/ongoing");
                Assert.Equal(HttpStatusCode.NotFound, matchesResponse.StatusCode);

                var fetched = await matchesResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"Session {sessionId} is Invalid.", fetched.Error);
            }
        }

        [Fact]
        public async Task GetParticipatedMatches()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                // Get Matches that Player have participated in
                var matchesResponse = await client.GetAsync("/api/matches");
                Assert.Equal(HttpStatusCode.OK, matchesResponse.StatusCode);

                var matches = await matchesResponse.Content.ReadAsJsonAsync<IList<Match>>();
                Assert.IsType(typeof(List<Match>), matches);
            }
        }

        [Fact]
        public async Task GetOngoingMatches()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id.ToString());

                // Get Matches that Player is currently involved with
                var matchesResponse = await client.GetAsync("/api/matches/ongoing");
                Assert.Equal(HttpStatusCode.OK, matchesResponse.StatusCode);

                var matchRead = await matchesResponse.Content.ReadAsStringAsync();
                var matches = JsonConvert.DeserializeObject<List<Match>>(matchRead, Actor.JsonSerializerSettings());
                Assert.IsType(typeof(List<Match>), matches);
            }
        }

        [Fact]
        public async Task GetOwnedMatches()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                // Get Matches that Player Own
                var matchesResponse = await client.GetAsync("/api/matches/owned");
                Assert.Equal(HttpStatusCode.OK, matchesResponse.StatusCode);

                var matches = await matchesResponse.Content.ReadAsJsonAsync<IList<Match>>();
                Assert.IsType(typeof(List<Match>), matches);

                foreach (var match in matches)
                {
                    Assert.True(match.Tournament.OwnerId.Equals(session.Player.Id));
                }
            }
        }

        [Fact]
        public async Task CreateQuickMatchActorsLessThan2Actors()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                var quickMatch = new QuickMatchActors { Actors = new List<Guid>(new[] { session.Player.Id }) };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.BadRequest, matchResponse.StatusCode);

                var content = await matchResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal("Minimum 2 Actors are required for a Match", content.Error);
            }
        }

        [Fact]
        public async Task CreateQuickMatchActorsInvalidActor()
        {
            var session = await Login();
            var invalidId = Guid.NewGuid();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { session.Player.Id, invalidId })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.NotFound, matchResponse.StatusCode);

                var content = await matchResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No Player with Id {invalidId} exists.", content.Error);
            }
        }

        [Fact]
        public async Task CreateQuickMatchActors()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchGet = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchGet, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);
            }
        }

        [Fact]
        public async Task CreateQuickMatch()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                var quickMatch = new QuickMatch();

                var matchResponse = await client.PostAsJsonAsync("/api/matches", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchGet = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchGet, Actor.JsonSerializerSettings());
                Assert.Equal(session.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);
            }
        }

        [Fact]
        public async Task CreateQuickMatchWithAlliance()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                var quickMatch = new QuickMatch { AlliancesOnly = true };

                var matchResponse = await client.PostAsJsonAsync("/api/matches", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchGet = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchGet, Actor.JsonSerializerSettings());
                Assert.Equal(session.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);
            }
        }

        [Fact]
        public async Task CreateQuickMatchWithAllianceForNonAlliedPlayer()
        {
            var ben = await Login("ben", "ben");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(ben.Id);

                var quickMatch = new QuickMatch { AlliancesOnly = true };

                var matchResponse = await client.PostAsJsonAsync("/api/matches", quickMatch);
                Assert.Equal(HttpStatusCode.NotFound, matchResponse.StatusCode);

                var response = await matchResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal("No Players available for match at this moment.", response.Error);
            }
        }

        [Fact]
        public async Task GetInvalidMatch()
        {
            var session = await Login();
            var invalidId = Guid.NewGuid();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                // Get Match with Invalid Id
                var matchResponse = await client.GetAsync($"/api/matches/{invalidId}");
                Assert.Equal(HttpStatusCode.NotFound, matchResponse.StatusCode);

                var content = await matchResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No Match found with Id {invalidId}.", content.Error);
            }
        }

        [Fact]
        public async Task GetInvalidMatchDetailed()
        {
            var session = await Login();
            var invalidId = Guid.NewGuid();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                // Get 'detailed' Match with Invalid Id
                var matchResponse = await client.GetAsync($"/api/matches/{invalidId}/detailed");
                Assert.Equal(HttpStatusCode.NotFound, matchResponse.StatusCode);

                var content = await matchResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No Match found with Id {invalidId}.", content.Error);
            }
        }

        [Fact]
        public async Task GetMatchOwnerInvalidMatch()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                // Get Match Owner with Invalid Id
                var invalidMatchId = Guid.NewGuid();
                var matchOwnerResponse = await client.GetAsync($"/api/matches/{invalidMatchId}/owner");
                Assert.Equal(HttpStatusCode.NotFound, matchOwnerResponse.StatusCode);

                var content = await matchOwnerResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No Match found for ID {invalidMatchId}.", content.Error);
            }
        }

        [Fact]
        public async Task GetMatchOwnerValidMatch()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Get Match Owner with Valid Id
                var matchOwnerResponse = await client.GetAsync($"/api/matches/{match.Id}/owner");
                Assert.Equal(HttpStatusCode.OK, matchOwnerResponse.StatusCode);

                var matchOwnerGet = await matchOwnerResponse.Content.ReadAsStringAsync();
                var matchOwner = JsonConvert.DeserializeObject<Actor>(matchOwnerGet, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, matchOwner.Id);
            }
        }

        [Fact]
        public async Task GetMatchValid()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Get Match with Valid Id
                matchResponse = await client.GetAsync($"/api/matches/{match.Id}");
                Assert.Equal(HttpStatusCode.OK, matchResponse.StatusCode);

                var matchRead = await matchResponse.Content.ReadAsStringAsync();
                var matchGet = JsonConvert.DeserializeObject<Match>(matchRead, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, matchGet.Tournament.OwnerId);
                Assert.Equal(match.Id, matchGet.Id);
            }
        }

        [Fact]
        public async Task GetMatchDetailedValid()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Get 'detailed' Match with Valid Id
                matchResponse = await client.GetAsync($"/api/matches/{match.Id}/detailed");
                Assert.Equal(HttpStatusCode.OK, matchResponse.StatusCode);

                var matchRead = await matchResponse.Content.ReadAsStringAsync();
                var matchGet = JsonConvert.DeserializeObject<Match>(matchRead, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, matchGet.Tournament.OwnerId);
                Assert.Equal(match.Id, matchGet.Id);
            }
        }

        [Fact]
        public async Task GetMatchActorsWithInvalidMatch()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                // Get Match Actors with Invalid Id
                var invalidMatchId = Guid.NewGuid();
                var matchActorResponse = await client.GetAsync($"/api/matches/{invalidMatchId}/actors");
                Assert.Equal(HttpStatusCode.NotFound, matchActorResponse.StatusCode);

                var content = await matchActorResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No such Match found.", content.Error);
            }
        }

        [Fact]
        public async Task GetMatchActorsWithValidMatch()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Get Match Actors with Valid Id
                var matchActorResponse = await client.GetAsync($"/api/matches/{match.Id}/actors");
                Assert.Equal(HttpStatusCode.OK, matchActorResponse.StatusCode);

                var matchActorGet = await matchActorResponse.Content.ReadAsStringAsync();
                var matchActors = JsonConvert.DeserializeObject<List<MatchActor>>(
                    matchActorGet,
                    Actor.JsonSerializerSettings());
                Assert.IsType(typeof(List<MatchActor>), matchActors);
            }
        }

        [Fact]
        public async Task GetMatchRoundsInvalidMatch()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                // Get Match Rounds with Invalid Id
                var invalidMatchId = Guid.NewGuid();
                var matchRoundsResponse = await client.GetAsync($"/api/matches/{invalidMatchId}/rounds");
                Assert.Equal(HttpStatusCode.NotFound, matchRoundsResponse.StatusCode);

                var content = await matchRoundsResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No such Match found.", content.Error);
            }
        }

        [Fact]
        public async Task GetMatchRoundsWithValidMatch()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Get Match Rounds with Valid Id
                var matchRoundsResponse = await client.GetAsync($"/api/matches/{match.Id}/rounds");
                Assert.Equal(HttpStatusCode.OK, matchRoundsResponse.StatusCode);

                var matchRounds = await matchRoundsResponse.Content.ReadAsJsonAsync<List<MatchRoundResponse>>();
                Assert.IsType(typeof(List<MatchRoundResponse>), matchRounds);
            }
        }

        [Fact]
        public async Task FinishInvalidMatch()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                // Finish Match with Invalid Id
                var invalidMatchId = Guid.NewGuid();
                var response = await client.DeleteAsync($"/api/matches/{invalidMatchId}");
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

                var content = await response.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No Match found.", content.Error);
            }
        }

        [Fact]
        public async Task FinishValidMatch()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Finish Match with Valid Id
                var response = await client.DeleteAsync($"/api/matches/{match.Id}");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var matchRead = await response.Content.ReadAsStringAsync();
                var finishedMatch = JsonConvert.DeserializeObject<Match>(matchRead, Actor.JsonSerializerSettings());
                Assert.True(finishedMatch.IsFinished);
            }
        }

        [Fact]
        public async Task UpdateMatchRoundScoreInvalidMatch()
        {
            var session = await Login();

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(session.Id);

                // Update Round Score Match with Invalid Id
                var invalidMatchId = Guid.NewGuid();

                var matchRoundForm = new MatchRoundForm { ActorId = session.Player.Id, RoundNumber = 1, Score = 10 };

                var response = await client.PutAsJsonAsync($"/api/matches/{invalidMatchId}/rounds", matchRoundForm);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

                var content = await response.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No such Match found.", content.Error);
            }
        }

        [Fact]
        public async Task UpdateMatchRoundScoreFinishedMatch()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Finish Match with Valid Id
                var response = await client.DeleteAsync($"/api/matches/{match.Id}");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var finishedMatch = await response.Content.ReadAsJsonAsync<Match>();
                Assert.True(finishedMatch.IsFinished);

                // Update Finished Match Round Score with Valid Id
                var matchRoundForm = new MatchRoundForm { ActorId = mayur.Player.Id, RoundNumber = 1, Score = 10 };
                var matchRoundsResponse = await client.PutAsJsonAsync($"/api/matches/{match.Id}/rounds", matchRoundForm);
                Assert.Equal(HttpStatusCode.BadRequest, matchRoundsResponse.StatusCode);

                var content = await matchRoundsResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"This match is already finished.", content.Error);
            }
        }

        [Fact]
        public async Task UpdateMatchRoundScoreInvalidActor()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Update Match Round Score with Invalid Round
                var matchRoundForm = new MatchRoundForm { ActorId = mayur.Player.Id, RoundNumber = 2, Score = 10 };
                var matchRoundsResponse = await client.PutAsJsonAsync($"/api/matches/{match.Id}/rounds", matchRoundForm);
                Assert.Equal(HttpStatusCode.NotFound, matchRoundsResponse.StatusCode);

                var content = await matchRoundsResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal(
                    $"No Round #{matchRoundForm.RoundNumber} found for Actor {matchRoundForm.ActorId}",
                    content.Error);
            }
        }

        [Fact]
        public async Task UpdateMatchRoundScoreInvalidRound()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Update Match Round Score with Invalid Round
                var invalidActorId = Guid.NewGuid();
                var matchRoundForm = new MatchRoundForm { ActorId = invalidActorId, RoundNumber = 1, Score = 10 };
                var matchRoundsResponse = await client.PutAsJsonAsync($"/api/matches/{match.Id}/rounds", matchRoundForm);
                Assert.Equal(HttpStatusCode.NotFound, matchRoundsResponse.StatusCode);

                var content = await matchRoundsResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No Actor {invalidActorId} found for this Match.", content.Error);
            }
        }

        [Fact]
        public async Task UpdateMatchRoundScore()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Update Match Round Score with Valid Id
                // Player 1
                var matchRoundForm = new MatchRoundForm { ActorId = mayur.Player.Id, RoundNumber = 1, Score = 10 };
                var matchRoundsResponse = await client.PutAsJsonAsync($"/api/matches/{match.Id}/rounds", matchRoundForm);
                Assert.Equal(HttpStatusCode.OK, matchRoundsResponse.StatusCode);

                var matchRoundScore = await matchRoundsResponse.Content.ReadAsJsonAsync<MatchRoundScoreResponse>();
                Assert.Equal(matchRoundForm.Score, matchRoundScore.Score);
                Assert.Equal(matchRoundForm.ActorId, matchRoundScore.ActorId);

                // Player 2
                var matchRoundForm2 = new MatchRoundForm { ActorId = matt.Player.Id, RoundNumber = 1, Score = 5 };
                matchRoundsResponse = await client.PutAsJsonAsync($"/api/matches/{match.Id}/rounds", matchRoundForm2);
                matchRoundScore = await matchRoundsResponse.Content.ReadAsJsonAsync<MatchRoundScoreResponse>();
                Assert.Equal(matchRoundForm2.Score, matchRoundScore.Score);
                Assert.Equal(matchRoundForm2.ActorId, matchRoundScore.ActorId);
            }
        }

        [Fact]
        public async Task UpdateMatchInvalidMatch()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Update Match with Invalid Id
                var invalidMatchId = Guid.NewGuid();
                var matchForm = new Match();
                var matchUpdateResponse = await client.PutAsJsonAsync($"/api/matches/{invalidMatchId}", matchForm);
                Assert.Equal(HttpStatusCode.NotFound, matchUpdateResponse.StatusCode);

                var content = await matchUpdateResponse.Content.ReadAsJsonAsync<ApiError>();
                Assert.Equal($"No such Match found.", content.Error);
            }
        }

        [Fact]
        public async Task UpdateMatchValid()
        {
            var mayur = await Login();
            var matt = await Login("matt", "matt");

            using (var client = new HttpClient { BaseAddress = new Uri(ServerUrl) })
            {
                client.AcceptJson().AddSessionHeader(mayur.Id);

                var quickMatch = new QuickMatchActors
                {
                    Actors = new List<Guid>(new[] { mayur.Player.Id, matt.Player.Id })
                };

                var matchResponse = await client.PostAsJsonAsync("/api/matches/actors", quickMatch);
                Assert.Equal(HttpStatusCode.Created, matchResponse.StatusCode);

                var matchReturn = await matchResponse.Content.ReadAsStringAsync();
                var match = JsonConvert.DeserializeObject<Match>(matchReturn, Actor.JsonSerializerSettings());
                Assert.Equal(mayur.Player.Id, match.Tournament.OwnerId);
                Assert.False(match.IsFinished);
                Assert.False(match.IsDeleted);

                // Update Match with Valid Id
                var matchForm = new Match { Title = "Updated Title" };
                var matchUpdateResponse = await client.PutAsJsonAsync($"/api/matches/{match.Id}", matchForm);
                Assert.Equal(HttpStatusCode.OK, matchUpdateResponse.StatusCode);

                var matchGet = await matchUpdateResponse.Content.ReadAsStringAsync();
                var content = JsonConvert.DeserializeObject<Match>(matchGet, Actor.JsonSerializerSettings());
                Assert.Equal(match.Id, content.Id);
                Assert.Equal(matchForm.Title, content.Title);
            }
        }
    }
}