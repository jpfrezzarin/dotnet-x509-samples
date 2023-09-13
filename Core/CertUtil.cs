using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Core;

public static class CertUtil
{
    // Server authentication OID code
    private const string ServerAuthOid = "1.3.6.1.5.5.7.3.1";

    // Client authentication OID code
    private const string ClientAuthOid = "1.3.6.1.5.5.7.3.2";

    // Localhost Subject Alternative Name
    // This byte array is equivalent to this config in openssl:
    // [ alt_names ]
    // IP.1 = 127.0.0.1
    // IP.2 = 0.0.0.0
    // DNS.1 = localhost
    private static readonly byte[] LocalhostSubAltNames =
    {
        48, 23, 135, 4, 127, 0, 0, 1, 135, 4, 0, 0, 0, 0, 130, 9, 108, 111, 99, 97, 108, 104, 111, 115, 116
    };

    /// <summary>
    /// Create a signer certificate
    /// </summary>
    /// <param name="root">The issuer/root/CA certificate</param>
    /// <param name="name">The common name to certificate</param>
    /// <returns></returns>
    public static X509Certificate2 CreateSigner(X509Certificate2 root, string name)
    {
        X509Certificate2? signer;
        
        using var rsa = RSA.Create(2048);
        
        CertificateRequest req = new CertificateRequest(
            $"CN={name}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));

        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.NonRepudiation | X509KeyUsageFlags.DigitalSignature | 
                X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment,
                false));

        req.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection
                {
                    new(ServerAuthOid), 
                    new(ClientAuthOid)
                },
                true));
        
        req.CertificateExtensions.Add(
            new X509SubjectAlternativeNameExtension(LocalhostSubAltNames));

        req.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

        using X509Certificate2 cert = req.Create(
            root,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(360),
            new byte[] { 1, 2, 3, 4, 5, 6 });

        using X509Certificate2 certWithKey = cert.CopyWithPrivateKey(rsa);
        
        var childExport = certWithKey.Export(X509ContentType.Pfx);

        signer = new X509Certificate2(childExport);

        return signer;
    }

    /// <summary>
    /// Create a issuer/root/CA certificate
    /// </summary>
    /// <param name="name">The common name to certificate</param>
    /// <returns></returns>
    public static X509Certificate2 CreateRoot(string name)
    {
        X509Certificate2? root;

        using RSA parent = RSA.Create(4096);
        
        CertificateRequest parentReq = new CertificateRequest(
            $"CN={name}",
            parent,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        parentReq.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(true, false, 0, true));

        parentReq.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(parentReq.PublicKey, false));

        using X509Certificate2 parentCert = parentReq.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-45),
            DateTimeOffset.UtcNow.AddDays(365));
        
        var export = parentCert.Export(X509ContentType.Pfx);
            
        root = new X509Certificate2(export);

        return root;
    }
    
    /// <summary>
    /// Get the certificate from path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static X509Certificate2? GetCertificate(string path)
    {
        return File.Exists(path) ? new X509Certificate2(path) : null;
    }
    
    /// <summary>
    /// Get the certificate from X509 store
    /// </summary>
    /// <param name="commonName">The certificate common name</param>
    /// <param name="storeName">The store name</param>
    /// <param name="location">The store location</param>
    /// <returns></returns>
    public static X509Certificate2? GetCertificate(string commonName, StoreName storeName, StoreLocation location)
    {
        // We will search for a certificate that has a CN (common name) that matches
        // the currently logged-in user.
        using var store = new X509Store(storeName, location);
        
        store.Open(OpenFlags.ReadOnly);
        var certificate = store.Certificates
            .FirstOrDefault(c => c.SubjectName.Name.Split(',').Any(sn => sn.Equals($"CN={commonName}")));
        store.Close();

        return certificate;
    }
    
    /// <summary>
    /// Add the certificate to a specific store
    /// </summary>
    /// <param name="cert">The cert</param>
    /// <param name="name">The store name</param>
    /// <param name="location">The store location</param>
    public static void AddCertToStore(X509Certificate2 cert, StoreName name, StoreLocation location)
    {
        using var store = new X509Store(name, location);
        store.Open(OpenFlags.ReadWrite);
        store.Add(cert);
        store.Close();
    }

    /// <summary>
    /// Get the certificate common name
    /// </summary>
    /// <param name="cert">Certificate</param>
    /// <returns></returns>
    public static string GetCommonName(this X509Certificate2 cert)
        => cert.GetNameInfo(X509NameType.SimpleName, false);
}
