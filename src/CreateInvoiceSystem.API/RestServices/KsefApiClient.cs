using CreateInvoiceSystem.Modules.Ksef.Application.Options;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using KSeF.Client.Api.Builders.Auth;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Authorization;
using Microsoft.Extensions.Options;

namespace CreateInvoiceSystem.API.RestServices;

public sealed class KsefApiClient(
    IAuthorizationClient authorizationClient,
    ICryptographyService cryptographyService,
    IOptions<KsefApiOptions> ksefOptions) : IKsefApiClient
{
    private static readonly TimeSpan AuthStatusPollInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan AuthStatusTimeout = TimeSpan.FromMinutes(2);

    public async Task<string> AuthenticateAsync(CancellationToken cancellationToken)
    {
        KsefApiOptions options = ksefOptions.Value;

        AuthenticationChallengeResponse challengeResponse = await authorizationClient
            .GetAuthChallengeAsync(cancellationToken).ConfigureAwait(false);

        long timeStamp = challengeResponse.Timestamp.ToUnixTimeMilliseconds();
        string tokenWithTimestamp = $"{options.KsefToken}|{timeStamp}";
        byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenWithTimestamp);
        byte[] encryptedBytes = cryptographyService.EncryptKsefTokenWithRSAUsingPublicKey(tokenBytes);
        string encryptedTokenBase64 = Convert.ToBase64String(encryptedBytes);

        IAuthKsefTokenRequestBuilderWithEncryptedToken builder = AuthKsefTokenRequestBuilder
            .Create()
            .WithChallenge(challengeResponse.Challenge)
            .WithContext(AuthenticationTokenContextIdentifierType.Nip, options.ContextIdentifierNip)
            .WithEncryptedToken(encryptedTokenBase64);

        SignatureResponse signatureResponse = await authorizationClient
            .SubmitKsefTokenAuthRequestAsync(builder.Build(), cancellationToken).ConfigureAwait(false);

        AuthStatus authStatus;
        DateTime startTime = DateTime.UtcNow;

        do
        {
            authStatus = await authorizationClient
                .GetAuthStatusAsync(signatureResponse.ReferenceNumber, signatureResponse.AuthenticationToken.Token, cancellationToken)
                .ConfigureAwait(false);

            if (authStatus.Status.Code != 200 && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(AuthStatusPollInterval, cancellationToken).ConfigureAwait(false);
            }
        }
        while (authStatus.Status.Code != 200
            && !cancellationToken.IsCancellationRequested
            && (DateTime.UtcNow - startTime) < AuthStatusTimeout);

        if (authStatus.Status.Code != 200)
        {
            throw new InvalidOperationException(
                $"Uwierzytelnienie w KSeF nie powiodło się lub przekroczono limit czasu. Status: {authStatus.Status.Code}, opis: {authStatus.Status.Description}.");
        }

        AuthenticationOperationStatusResponse authResult = await authorizationClient
            .GetAccessTokenAsync(signatureResponse.AuthenticationToken.Token, cancellationToken).ConfigureAwait(false);

        return authResult.AccessToken.Token;
    }
}