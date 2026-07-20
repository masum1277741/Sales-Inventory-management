using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ClothingERP.Infrastructure.PaymentGateways;

// Nagad এর API flow RSA encryption দিয়ে sensitive data wrap করে পাঠাতে হয়।
// এখানে কাঠামো দেখানো হচ্ছে — production এ যাওয়ার আগে Nagad এর official
// integration team এর সাথে keys ও exact payload format যাচাই করে নিন।
public class NagadApiClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public NagadApiClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
        _http.BaseAddress = new Uri(_config["NagadConfig:BaseUrl"]!);
    }

    // ── Sensitive Data RSA দিয়ে Encrypt করো (Nagad এর Public Key দিয়ে) ──────
    private string EncryptWithNagadPublicKey(string plainText)
    {
        var publicKeyPem = File.ReadAllText(_config["NagadConfig:PublicKeyPath"]!);
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        var encrypted = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.Pkcs1);
        return Convert.ToBase64String(encrypted);
    }

    // ── নিজের ডাটা Merchant এর Private Key দিয়ে Sign করো ──────────────────────
    private string SignWithMerchantPrivateKey(string plainText)
    {
        var privateKeyPem = File.ReadAllText(_config["NagadConfig:PrivateKeyPath"]!);
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        var signature = rsa.SignData(Encoding.UTF8.GetBytes(plainText), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signature);
    }

    // ── Payment Initialize ────────────────────────────────────────────────
    public async Task<(bool Success, string? PaymentRefId, string? RedirectUrl, string RawJson)>
        InitializePaymentAsync(decimal amountBDT, string orderId, string customerMsisdn)
    {
        var merchantId = _config["NagadConfig:MerchantId"];
        var dateTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        var sensitiveData = JsonSerializer.Serialize(new
        {
            merchantId,
            datetime = dateTime,
            orderId
        });

        var payload = new
        {
            accountNumber = customerMsisdn,
            dateTime,
            sensitiveData = EncryptWithNagadPublicKey(sensitiveData),
            signature = SignWithMerchantPrivateKey(sensitiveData)
        };

        var response = await _http.PostAsJsonAsync($"/check-out/initialize/{merchantId}/{orderId}", payload);
        var rawJson = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(rawJson).RootElement;

        if (!response.IsSuccessStatusCode || !json.TryGetProperty("paymentReferenceId", out var refId))
            return (false, null, null, rawJson);

        var callbackUrl = json.TryGetProperty("callBackUrl", out var cb) ? cb.GetString() : null;
        return (true, refId.GetString(), callbackUrl, rawJson);
    }

    // ── Verify Payment ────────────────────────────────────────────────────
    public async Task<(string Status, string? TrxId, string RawJson)> VerifyPaymentAsync(string paymentRefId)
    {
        var response = await _http.GetAsync($"/verify/payment/{paymentRefId}");
        var rawJson = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(rawJson).RootElement;

        var status = json.TryGetProperty("status", out var s) ? s.GetString() : "Unknown";
        var trxId = json.TryGetProperty("issuerPaymentRefNo", out var t) ? t.GetString() : null;

        return (status ?? "Unknown", trxId, rawJson);
    }
}