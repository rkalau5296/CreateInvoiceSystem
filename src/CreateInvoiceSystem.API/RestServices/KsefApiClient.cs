using System.Text;
using CreateInvoiceSystem.Modules.Ksef.Dto;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using KSeF.Client.Api.Builders.Auth;
using KSeF.Client.Api.Builders.Online;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.ApiResponses;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Core.Models.Sessions.OnlineSession;

namespace CreateInvoiceSystem.API.RestServices;

public sealed class KsefApiClient(
    IAuthorizationClient authorizationClient,
    IInvoiceDownloadClient invoiceDownloadClient,
    IKSeFClient ksefClient,
    ICryptographyService cryptographyService,
    IKsefCredentialsStore credentialsStore,
    IKsefInvoiceBytesClient invoiceBytesClient) : IKsefApiClient
{
    private static readonly TimeSpan AuthStatusPollInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan AuthStatusTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan SessionStatusPollInterval = TimeSpan.FromSeconds(2);
    private const int MaxSessionStatusPollAttempts = 60;

    private async Task<string> AuthenticateAsync(int userId, CancellationToken cancellationToken)
    {
        var credentials = await credentialsStore.GetRequiredAsync(userId, cancellationToken).ConfigureAwait(false);

        AuthenticationChallengeResponse challengeResponse = await authorizationClient
            .GetAuthChallengeAsync(cancellationToken).ConfigureAwait(false);

        await cryptographyService.WarmupAsync(cancellationToken).ConfigureAwait(false);

        long timeStamp = challengeResponse.Timestamp.ToUnixTimeMilliseconds();
        string tokenWithTimestamp = $"{credentials.Token}|{timeStamp}";
        byte[] tokenBytes = Encoding.UTF8.GetBytes(tokenWithTimestamp);
        byte[] encryptedBytes = cryptographyService.EncryptKsefTokenWithRSAUsingPublicKey(tokenBytes);
        string encryptedTokenBase64 = Convert.ToBase64String(encryptedBytes);

        IAuthKsefTokenRequestBuilderWithEncryptedToken builder = AuthKsefTokenRequestBuilder
            .Create()
            .WithChallenge(challengeResponse.Challenge)
            .WithContext(AuthenticationTokenContextIdentifierType.Nip, credentials.Nip)
            .WithEncryptedToken(encryptedTokenBase64);

        SignatureResponse signatureResponse = await authorizationClient
            .SubmitKsefTokenAuthRequestAsync(builder.Build(), cancellationToken).ConfigureAwait(false);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(AuthStatusTimeout);
        try
        {
            while (true)
            {
                AuthStatus authStatus = await authorizationClient
                    .GetAuthStatusAsync(signatureResponse.ReferenceNumber, signatureResponse.AuthenticationToken.Token, timeout.Token)
                    .ConfigureAwait(false);

                if (authStatus.Status.Code == AuthenticationStatusCodeResponse.AuthenticationSuccess)
                    break;

                if (authStatus.Status.Code != AuthenticationStatusCodeResponse.AuthenticationInProgress)
                    throw new InvalidOperationException(
                        $"Uwierzytelnienie w KSeF nie powiodło się. Kod statusu: {authStatus.Status.Code}, opis: {authStatus.Status.Description}.");

                await Task.Delay(AuthStatusPollInterval, timeout.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeout.IsCancellationRequested)
        {
            throw new InvalidOperationException(
                "Przekroczono limit czasu oczekiwania na uwierzytelnienie w KSeF.");
        }

        AuthenticationOperationStatusResponse authResult = await authorizationClient
            .GetAccessTokenAsync(signatureResponse.AuthenticationToken.Token, cancellationToken).ConfigureAwait(false);

        return authResult.AccessToken.Token;
    }

    public async Task<KsefSendResult> SendInvoiceAsync(int userId, string invoiceXml, CancellationToken cancellationToken)
    {
        string accessToken = await AuthenticateAsync(userId, cancellationToken).ConfigureAwait(false);

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

            var message = $"KSeF odrzucił fakturę. Status sesji: {statusAfterSend.Status?.Code}, opis: {statusAfterSend.Status?.Description}. Szczegóły: {details}";
            var codes = failedInvoices.Invoices?.Select(invoice => invoice.Status?.Code).ToArray() ?? [];
            if (KsefRejectionClassifier.IsConfirmed(statusAfterSend.FailedInvoiceCount, codes))
                throw new KsefInvoiceRejectedException(message);

            throw new InvalidOperationException(message);
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

    public async Task<KsefInvoicePage> GetIncomingInvoicesAsync(
        int userId,
        DateOnly fromDate,
        DateOnly toDate,
        int pageOffset,
        int pageSize,
        CancellationToken cancellationToken)
    {
        string accessToken = await AuthenticateAsync(userId, cancellationToken).ConfigureAwait(false);
        var filters = new InvoiceQueryFilters
        {
            SubjectType = InvoiceSubjectType.Subject2,
            DateRange = new DateRange
            {
                DateType = DateType.Issue,
                From = new DateTimeOffset(fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)),
                To = new DateTimeOffset(toDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc))
            }
        };

        PagedInvoiceResponse response = await invoiceDownloadClient
            .QueryInvoiceMetadataAsync(filters, accessToken, pageOffset, pageSize, SortOrder.Desc, cancellationToken)
            .ConfigureAwait(false);

        return new KsefInvoicePage(
            response.Invoices?.Select(invoice => new KsefInvoiceDto(
                invoice.KsefNumber,
                invoice.InvoiceNumber,
                invoice.IssueDate,
                invoice.Seller?.Name,
                invoice.Seller?.Nip,
                invoice.Buyer?.Name,
                invoice.NetAmount,
                invoice.VatAmount,
                invoice.GrossAmount,
                invoice.Currency)).ToList() ?? [],
            response.HasMore);
    }

    public async Task<string> DownloadInvoiceXmlAsync(int userId, string ksefNumber, CancellationToken cancellationToken)
    {
        string accessToken = await AuthenticateAsync(userId, cancellationToken).ConfigureAwait(false);
        return await invoiceDownloadClient
            .GetInvoiceAsync(ksefNumber, accessToken, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<byte[]> DownloadInvoiceXmlBytesAsync(int userId, string ksefNumber, CancellationToken cancellationToken)
    {
        string accessToken = await AuthenticateAsync(userId, cancellationToken).ConfigureAwait(false);
        return await invoiceBytesClient.DownloadAsync(ksefNumber, accessToken, cancellationToken).ConfigureAwait(false);
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