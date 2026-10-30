using System.Text;
using CreateInvoiceSystem.Modules.Ksef.Application.Options;
using CreateInvoiceSystem.Modules.Ksef.Dto;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using KSeF.Client.Api.Builders.Auth;
using KSeF.Client.Api.Builders.Online;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Core.Models.Sessions.OnlineSession;
using Microsoft.Extensions.Options;

namespace CreateInvoiceSystem.API.RestServices;

public sealed class KsefApiClient(
    IAuthorizationClient authorizationClient,
    IKSeFClient ksefClient,
    ICryptographyService cryptographyService,
    IOptions<KsefApiOptions> ksefOptions) : IKsefApiClient
{
    private static readonly TimeSpan AuthStatusPollInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan AuthStatusTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan SessionStatusPollInterval = TimeSpan.FromSeconds(2);
    private const int MaxSessionStatusPollAttempts = 60;

    public async Task<string> AuthenticateAsync(CancellationToken cancellationToken)
    {
        KsefApiOptions options = ksefOptions.Value;

        AuthenticationChallengeResponse challengeResponse = await authorizationClient
            .GetAuthChallengeAsync(cancellationToken).ConfigureAwait(false);

        long timeStamp = challengeResponse.Timestamp.ToUnixTimeMilliseconds();
        string tokenWithTimestamp = $"{options.KsefToken}|{timeStamp}";
        byte[] tokenBytes = Encoding.UTF8.GetBytes(tokenWithTimestamp);
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

    public async Task<KsefSendResult> SendInvoiceAsync(string invoiceXml, CancellationToken cancellationToken)
    {
        string accessToken = await AuthenticateAsync(cancellationToken).ConfigureAwait(false);

        EncryptionData encryptionData = cryptographyService.GetEncryptionData();

        OpenOnlineSessionRequest openRequest = OpenOnlineSessionRequestBuilder
            .Create()
            .WithFormCode(systemCode: "FA (3)", schemaVersion: "1-0E", value: "FA")
            .WithEncryption(
                encryptedSymmetricKey: encryptionData.EncryptionInfo.EncryptedSymmetricKey,
                initializationVector: encryptionData.EncryptionInfo.InitializationVector,
                publicKeyId: encryptionData.EncryptionInfo.PublicKeyId)
            .Build();

        OpenOnlineSessionResponse openSession = await ksefClient
            .OpenOnlineSessionAsync(openRequest, accessToken, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        byte[] invoiceBytes = Encoding.UTF8.GetBytes(invoiceXml);
        byte[] encryptedInvoice = cryptographyService.EncryptBytesWithAES256(
            invoiceBytes, encryptionData.CipherKey, encryptionData.CipherIv);

        FileMetadata invoiceMetadata = cryptographyService.GetMetaData(invoiceBytes);
        FileMetadata encryptedMetadata = cryptographyService.GetMetaData(encryptedInvoice);

        SendInvoiceRequest sendRequest = SendInvoiceOnlineSessionRequestBuilder
            .Create()
            .WithInvoiceHash(invoiceMetadata.HashSHA, invoiceMetadata.FileSize)
            .WithEncryptedDocumentHash(encryptedMetadata.HashSHA, encryptedMetadata.FileSize)
            .WithEncryptedDocumentContent(Convert.ToBase64String(encryptedInvoice))
            .WithOfflineMode(false)
            .Build();

        SendInvoiceResponse sendResponse = await ksefClient
            .SendOnlineSessionInvoiceAsync(sendRequest, openSession.ReferenceNumber, accessToken, cancellationToken)
            .ConfigureAwait(false);

        SessionStatusResponse statusAfterSend = await PollSessionStatusAsync(
            openSession.ReferenceNumber,
            accessToken,
            status => status.SuccessfulInvoiceCount is not null || status.FailedInvoiceCount is not null,
            cancellationToken).ConfigureAwait(false);

        if (statusAfterSend.FailedInvoiceCount is not null && statusAfterSend.FailedInvoiceCount > 0)
        {
            SessionInvoicesResponse failedInvoices = await ksefClient
                .GetSessionFailedInvoicesAsync(openSession.ReferenceNumber, accessToken, pageSize: null, continuationToken: null, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            string details = string.Join(" | ", failedInvoices.Invoices?
                .Select(i => $"{i.Status?.Code}: {i.Status?.Description} [{string.Join(", ", i.Status?.Details ?? [])}]") ?? []);

            throw new InvalidOperationException(
                $"KSeF odrzucił fakturę. Status sesji: {statusAfterSend.Status?.Code}, opis: {statusAfterSend.Status?.Description}. Szczegóły: {details}");
        }

        await ksefClient
            .CloseOnlineSessionAsync(openSession.ReferenceNumber, accessToken, cancellationToken)
            .ConfigureAwait(false);

        SessionInvoicesResponse invoices = await ksefClient
            .GetSessionInvoicesAsync(openSession.ReferenceNumber, accessToken, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        SessionInvoice? sentInvoice = invoices.Invoices?.FirstOrDefault();
        if (sentInvoice is null)
        {
            throw new InvalidOperationException("KSeF nie zwrócił informacji o wysłanej fakturze w sesji.");
        }

        SessionStatusResponse statusAfterClose = await PollSessionStatusAsync(
            openSession.ReferenceNumber,
            accessToken,
            status => status.Upo?.Pages is { Count: > 0 },
            cancellationToken).ConfigureAwait(false);

        string upoXml = await ksefClient
            .GetUpoAsync(sentInvoice.UpoDownloadUrl, cancellationToken)
            .ConfigureAwait(false);

        return new KsefSendResult(
            SessionReferenceNumber: openSession.ReferenceNumber,
            InvoiceReferenceNumber: sendResponse.ReferenceNumber,
            KsefNumber: sentInvoice.KsefNumber,
            UpoXml: upoXml);
    }

    private async Task<SessionStatusResponse> PollSessionStatusAsync(
        string sessionReferenceNumber,
        string accessToken,
        Func<SessionStatusResponse, bool> condition,
        CancellationToken cancellationToken)
    {
        SessionStatusResponse? last = null;

        for (int attempt = 0; attempt < MaxSessionStatusPollAttempts; attempt++)
        {
            last = await ksefClient
                .GetSessionStatusAsync(sessionReferenceNumber, accessToken, cancellationToken)
                .ConfigureAwait(false);

            if (condition(last))
            {
                return last;
            }

            await Task.Delay(SessionStatusPollInterval, cancellationToken).ConfigureAwait(false);
        }

        throw new TimeoutException(
            $"Przekroczono limit czasu oczekiwania na status sesji KSeF. Ostatni kod: {last?.Status?.Code}.");
    }
}