using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClothingERP.Infrastructure.PaymentGateways;

public class BkashApiClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<BkashApiClient> _logger;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public BkashApiClient(HttpClient http, IConfiguration config, ILogger<BkashApiClient> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
        _http.BaseAddress = new Uri(_config["BkashConfig:BaseUrl"]!);
    }

    // ── STEP 1: Grant Token (OAuth) ─────────────────────────────────────────
    private async Task<string> GetTokenAsync()
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        var request = new HttpRequestMessage(HttpMethod.Post, "/tokenized/checkout/token/grant");
        request.Headers.Add("username", _config["BkashConfig:Username"]);
        request.Headers.Add("password", _config["BkashConfig:Password"]);
        request.Content = JsonContent.Create(new
        {
            app_key = _config["BkashConfig:AppKey"],
            app_secret = _config["BkashConfig:AppSecret"]
        });

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode || !json.TryGetProperty("id_token", out var tokenProp))
        {
            _logger.LogError("bKash token grant failed: {Response}", json);
            throw new InvalidOperationException("bKash token গ্রহণ করা যায়নি।");
        }

        _cachedToken = tokenProp.GetString();
        var expiresIn = json.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60); // ১ মিনিট আগেই expire ধরে নিরাপদ থাকা

        return _cachedToken!;
    }

    private async Task<HttpRequestMessage> BuildAuthorizedRequestAsync(HttpMethod method, string path, object? body = null)
    {
        var token = await GetTokenAsync();
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("Authorization", token);
        request.Headers.Add("X-App-Key", _config["BkashConfig:AppKey"]);
        if (body != null) request.Content = JsonContent.Create(body);
        return request;
    }

    // ── STEP 2: Create Payment ───────────────────────────────────────────────
    public async Task<(bool Success, string PaymentId, string? BkashURL, string RawJson, string? Error)>
        CreatePaymentAsync(decimal amountBDT, string merchantInvoiceNumber)
    {
        var request = await BuildAuthorizedRequestAsync(HttpMethod.Post, "/tokenized/checkout/create", new
        {
            mode = "0011",   // checkout (URL) mode
            payerReference = merchantInvoiceNumber,
            callbackURL = _config["BkashConfig:CallbackUrl"],
            amount = amountBDT.ToString("F2"),
            currency = "BDT",
            intent = "sale",
            merchantInvoiceNumber = merchantInvoiceNumber
        });

        var response = await _http.SendAsync(request);
        var rawJson = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(rawJson).RootElement;

        if (!response.IsSuccessStatusCode || !json.TryGetProperty("paymentID", out var pidProp))
        {
            var statusMsg = json.TryGetProperty("statusMessage", out var sm) ? sm.GetString() : "Unknown error";
            return (false, "", null, rawJson, statusMsg);
        }

        var bkashUrl = json.TryGetProperty("bkashURL", out var url) ? url.GetString() : null;
        return (true, pidProp.GetString()!, bkashUrl, rawJson, null);
    }

    // ── STEP 3: Execute Payment (customer approve করার পরে confirm করতে) ──────
    public async Task<(bool Success, string Status, string? TrxId, string RawJson, string? Error)>
        ExecutePaymentAsync(string paymentId)
    {
        var request = await BuildAuthorizedRequestAsync(HttpMethod.Post, "/tokenized/checkout/execute", new { paymentID = paymentId });
        var response = await _http.SendAsync(request);
        var rawJson = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(rawJson).RootElement;

        var status = json.TryGetProperty("transactionStatus", out var ts) ? ts.GetString() : "Failed";
        var trxId = json.TryGetProperty("trxID", out var tid) ? tid.GetString() : null;
        var errorMsg = json.TryGetProperty("statusMessage", out var sm) ? sm.GetString() : null;

        return (status == "Completed", status ?? "Failed", trxId, rawJson, errorMsg);
    }

    // ── STEP 4: Query Payment (status verify/poll করার জন্য) ─────────────────
    public async Task<(string Status, string? TrxId, string RawJson)> QueryPaymentAsync(string paymentId)
    {
        var request = await BuildAuthorizedRequestAsync(HttpMethod.Post, "/tokenized/checkout/payment/status", new { paymentID = paymentId });
        var response = await _http.SendAsync(request);
        var rawJson = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(rawJson).RootElement;

        var status = json.TryGetProperty("transactionStatus", out var ts) ? ts.GetString() : "Unknown";
        var trxId = json.TryGetProperty("trxID", out var tid) ? tid.GetString() : null;

        return (status ?? "Unknown", trxId, rawJson);
    }
}