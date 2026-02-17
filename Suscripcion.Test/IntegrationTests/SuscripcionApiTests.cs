using FluentAssertions;
using Moq;
using Polly;
using Polly.Retry;
using Suscripcion.Application.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Suscripcion.Test.IntegrationTests;

public class SuscripcionApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SuscripcionApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(Skip = "Requiere base de datos configurada")]
    public async Task RegisterUser_ValidRequest_ReturnsSuccess()
    {
        var uniqueEmail = $"john{Guid.NewGuid():N}@test.com";
        var request = new RegisterUserRequestDto("John Doe", uniqueEmail);

        var response = await _client.PostAsJsonAsync("/api/user/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RegisterUserResponseDto>();
        result.Should().NotBeNull();
        result!.UserId.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Requiere base de datos y RabbitMQ configurados")]
    public async Task Subscribe_ValidRequest_ReturnsAccepted()
    {
        var uniqueEmail = $"jane{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequestDto("Jane Doe", uniqueEmail);
        var registerResponse = await _client.PostAsJsonAsync("/api/user/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponseDto>();

        var subscribeRequest = new SubscribeRequestDto(registerResult!.UserId, "credit_card");

        var response = await _client.PostAsJsonAsync("/api/suscripcion/subscribe", subscribeRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<SubscribeResponseDto>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Procesando");
    }

    [Fact(Skip = "Requiere RabbitMQ corriendo para procesar mensajes asíncronos")]
    public async Task GetSubscription_AfterSubscribe_ReturnsSubscription()
    {
        var uniqueEmail = $"test{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequestDto("Test User", uniqueEmail);
        var registerResponse = await _client.PostAsJsonAsync("/api/user/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponseDto>();

        var subscribeRequest = new SubscribeRequestDto(registerResult!.UserId, "credit_card");
        await _client.PostAsJsonAsync("/api/suscripcion/subscribe", subscribeRequest);

        var result = await WaitForSubscriptionAsync(registerResult!.UserId);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(registerResult.UserId);
        result.Status.Should().BeOneOf("Active", "PaymentFailed");
    }

    private async Task<GetSubscriptionResponseDto?> WaitForSubscriptionAsync(int userId)
    {
        var retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 10,
                Delay = TimeSpan.FromMilliseconds(500),
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
            })
            .Build();

        return await retryPipeline.ExecuteAsync(async ct =>
        {
            var response = await _client.GetAsync($"/api/suscripcion/{userId}", ct);
            
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new HttpRequestException("Subscription not ready yet");
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GetSubscriptionResponseDto>(ct);
        });
    }

    [Fact(Skip = "Requiere base de datos configurada")]
    public async Task GetSubscription_SubscriptionNotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/suscripcion/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
