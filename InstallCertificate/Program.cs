// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography.X509Certificates;
using Core;

var root = CertUtil.GetCertificate(".NET Test CA", StoreName.Root, StoreLocation.CurrentUser) ?? 
           CertUtil.CreateRoot(".NET Test CA");

if (CertUtil.GetCertificate(root.GetCommonName(), StoreName.Root, StoreLocation.CurrentUser) is null)
{
    CertUtil.AddCertToStore(root, StoreName.Root, StoreLocation.CurrentUser);
    
    Console.WriteLine("Root certificate installed");
}
else
{
    Console.WriteLine("Root certificate already exists");
}

var signer = CertUtil.GetCertificate(".NET Test", StoreName.My, StoreLocation.CurrentUser);
if (signer is null)
{
    signer = CertUtil.CreateSigner(root, ".NET Test");

    CertUtil.AddCertToStore(signer, StoreName.My, StoreLocation.CurrentUser);

    Console.WriteLine("Signer certificate installed");
}
else
{
    Console.WriteLine("Signer certificate already exists");
}
