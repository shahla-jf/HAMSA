using System.Net.Http.Headers;
using System.Text;
using HAMSA.Domain.Interfaces.Services;

namespace HAMSA.Infrastructure.ExternalServices;

public class FileStorageService : IFileStorageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    
    private readonly string _baseUrl;
    private readonly string _username;
    private readonly string _password;
    private readonly string _uploadUrl;
    
    public FileStorageService(
        IHttpClientFactory factory,
        IConfiguration configuration,
        ILogger<FileStorageService> logger)
    {
        _httpClientFactory = factory;
        _configuration = configuration;
        _logger = logger;
        
        _baseUrl = configuration["N8N:BaseUrl"]!;
        _uploadUrl = $"{_baseUrl}/webhook/upload-image";
        _username = configuration["N8N:Username"]!;
        _password = configuration["N8N:Password"]!;
    }

    public async Task<string> UploadImageAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();

        client.Timeout = TimeSpan.FromMinutes(5);
        
        var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(byteArray));
        
        using var content = new MultipartFormDataContent();

        content.Add(
            new StreamContent(stream),
            "data",
            fileName);
        
        //content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        
        var response = await client.PostAsync(
            _uploadUrl,
            content,
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
        
        return result!.Url;
    }
}


public class UploadResponse
{
    public string Url { get; set; } = "";
}