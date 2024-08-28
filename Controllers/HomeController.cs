using AzStorageAccountPrivareEndP.Models;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;

namespace AzStorageAccountPrivareEndP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly AppDatabaseContext _databaseContext;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, AppDatabaseContext databaseContext)
        {
            _logger = logger;
            _configuration = configuration;
            _databaseContext = databaseContext;
        }

        public IActionResult Index()
        {
            return Ok();
            try
            {
                //go to azure stgaccount you can find the uri for blob under the endpoints menu option under the storage account.
                var stgAccBlobUriConfig = _configuration.GetSection("stgAccBlobUri").Value ??
                                                    throw new NullReferenceException("Failed to get stgAccUri from config");
                Uri accountUri = new Uri(stgAccBlobUriConfig);
                //var k = new DefaultAzureCredential();
                BlobServiceClient client = new BlobServiceClient(accountUri, new DefaultAzureCredential(true));
                var containers= client.GetBlobContainers().First();
                return View();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,ex.Message);
            }
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult GetFile(MyFileClass file)
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SaveFile(IFormFile file)
        {
            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                var content = stream.ReadToEnd();
                var WordContent = content.Split(" ").ToList();
                string first5Words="";
                for (int i = 0; i < 5; i++)
                {
                     first5Words = first5Words + WordContent[i] + " ";
                }
                first5Words = first5Words.Substring(0, first5Words.Length - 1);
                var fileObj = new DatabaseModel() 
                {
                    FileName = file.FileName,
                    First5Words = first5Words
                };
                await _databaseContext.FileDetails.AddAsync(fileObj);
                await _databaseContext.SaveChangesAsync();

                //go to azure stgaccount you can find the uri for blob under the endpoints menu option under the storage account.
                var stgAccBlobUriConfig = _configuration.GetSection("stgAccBlobUri").Value ??
                                                    throw new NullReferenceException("Failed to get stgAccUri from config");
                Uri accountUri = new Uri(stgAccBlobUriConfig);
                //var k = new DefaultAzureCredential();
                BlobServiceClient client = new BlobServiceClient(accountUri, new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {   TenantId = _configuration.GetSection("TenantId").Value ??
                                                    throw new NullReferenceException("Failed to get TenantId from config"),
                    ManagedIdentityClientId = _configuration.GetSection("ManagedIdentityClientId").Value ??
                                                    throw new NullReferenceException("Failed to get ManagedIdentityClientId from config"),
                    ExcludeEnvironmentCredential = true,
                    ExcludeWorkloadIdentityCredential = true,
                }));
                BlobContainerClient containerClient = client.GetBlobContainerClient("testcontainer");
                await containerClient.CreateIfNotExistsAsync();
                BlobClient blobClient = containerClient.GetBlobClient(file.FileName + DateTime.UtcNow.Date);
                stream.BaseStream.Position= 0;
                await blobClient.UploadAsync(stream.BaseStream, true);
                stream.BaseStream.Close();
                //var containers = client.GetBlobContainers().First();

                // Process the content as needed
                return Ok(content);
            }
        }
    }
}
