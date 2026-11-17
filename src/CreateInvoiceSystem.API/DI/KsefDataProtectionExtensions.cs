using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;

namespace CreateInvoiceSystem.API.DI;

public static class KsefDataProtectionExtensions
{
    public static IServiceCollection AddKsefDataProtection(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var keyDirectory = configuration["KsefDataProtection:KeyDirectory"];
        var certificatePath = configuration["KsefDataProtection:CertificatePath"];
        var certificatePassword = configuration["KsefDataProtection:CertificatePassword"];

        var dataProtection = services.AddDataProtection().SetApplicationName("CreateInvoiceSystem");
        if (string.IsNullOrWhiteSpace(keyDirectory)
            || string.IsNullOrWhiteSpace(certificatePath)
            || string.IsNullOrWhiteSpace(certificatePassword))
        {
            if (!environment.IsDevelopment())
                throw new InvalidOperationException(
                    "Skonfiguruj KsefDataProtection:KeyDirectory, CertificatePath i CertificatePassword.");

            if (!string.IsNullOrWhiteSpace(keyDirectory)
                || !string.IsNullOrWhiteSpace(certificatePath)
                || !string.IsNullOrWhiteSpace(certificatePassword))
                throw new InvalidOperationException("Konfiguracja KsefDataProtection jest niekompletna.");

            return services;
        }

        if (!Path.IsPathFullyQualified(keyDirectory) || !Path.IsPathFullyQualified(certificatePath))
            throw new InvalidOperationException("Ścieżki KsefDataProtection muszą być bezwzględne.");

        var certificate = X509CertificateLoader.LoadPkcs12FromFile(
            certificatePath, certificatePassword, X509KeyStorageFlags.EphemeralKeySet);
        if (!certificate.HasPrivateKey)
            throw new InvalidOperationException("Certyfikat KsefDataProtection musi zawierać klucz prywatny.");

        dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keyDirectory))
            .ProtectKeysWithCertificate(certificate);
        return services;
    }
}
