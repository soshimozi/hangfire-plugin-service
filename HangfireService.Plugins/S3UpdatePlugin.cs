using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Amazon.S3;
using Amazon.S3.Model;
using HangfireService.Contracts;
using HangfireService.Plugins.AWS;
using HangfireService.Plugins.Model;
using HangfireService.Plugins.Repository;
using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService.Plugins
{
    [Export(typeof(IPluginHandler))]
    [ExportMetadata("Name", "S3UpdatePlugin")]
    public class S3UpdatePlugin : IPluginHandler
    {
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(typeof(S3UpdatePlugin));

        private readonly string _connectionString;
        private readonly string _bucketName;
        private readonly string _folder;
        private readonly string _regionName;
        private readonly string _profileName;
        private readonly string _domain;

        private readonly IRepository<DocumentVersion, string> _repository;

        public S3UpdatePlugin()
        {
            _connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=SSPI;";

            _repository = new VersionRepository(_connectionString);

            _bucketName = ConfigurationManager.AppSettings["WatchBucket"];
            _folder = ConfigurationManager.AppSettings["WatchFolder"];
            _regionName = ConfigurationManager.AppSettings["WatchRegion"];
            _profileName = ConfigurationManager.AppSettings["AWSProfileName"];
            _domain = ConfigurationManager.AppSettings["Domain"];
        }

        public async Task Handle()
        {
            // parameters: bucket,folder
            logger.Info("S3UpdatePlugin.Handle");

            var s3VersionId = GetCurrentDocumentVersion(_domain);
            var document = await _repository.FindById(_domain);

            if(document == null || document.Version != s3VersionId)
            {
                await DownloadFilesForDomain(_domain);


                var model = new DocumentVersion { DocumentKey = _domain, Version = s3VersionId };

                // if there was an existing document, update it's version
                if (document != null)
                {
                    await _repository.Update(model);
                }
                else
                {
                    await _repository.Add(model);
                }

                // TODO: Notification?
            }
        }

        private async Task DownloadFilesForDomain(string domain)
        {
            using (var client = S3ClientFactory.GetS3Client(_regionName, _profileName))
            {
                var documentName = $"{_folder}/{domain}.fullchain.pem";
                var metadata = await GetMetadata(documentName, client);

                var document = await GetDocument(documentName, client);

                var kmsClient = new AmazonKeyManagementServiceClient(RegionEndpoint.GetBySystemName(_regionName));



                var ms = new MemoryStream(Convert.FromBase64String(metadata["x-amz-meta-tmkciphertext"]));

                try
                {
                    var decryptResponse = await kmsClient.DecryptAsync(new DecryptRequest { CiphertextBlob = ms });

                    // TODO: put this in guarded memory somehow?
                    var keyId = decryptResponse.KeyId;
                    var tmkPlinText = Convert.ToBase64String(decryptResponse.Plaintext.ToArray());
                    var plainTextArray = decryptResponse.Plaintext.ToArray();

                    var iv = new byte[16];
                    using (var rijndaelManaged =
                            new RijndaelManaged { Key = plainTextArray, IV = iv, Mode = CipherMode.CBC })
                    {
                        rijndaelManaged.BlockSize = 128;
                        rijndaelManaged.KeySize = 256;
                        rijndaelManaged.Padding = PaddingMode.None;
                        using (var memoryStream =
                               new MemoryStream(Convert.FromBase64String(metadata["x-amz-meta-tdkciphertext"])))
                        using (var cryptoStream =
                               new CryptoStream(memoryStream,
                                   rijndaelManaged.CreateDecryptor(plainTextArray, iv),
                                   CryptoStreamMode.Read))
                        {
                            var data = new StreamReader(cryptoStream).ReadToEnd();

                            //var buffer = new byte[256];
                            //List<byte> bytes = new List<byte>();
                            //int lastIndex = 0;
                            //int bytesRead = 0;
                            //do
                            //{
                            //    bytesRead = await cryptoStream.ReadAsync(buffer, lastIndex, buffer.Length);
                            //    lastIndex += bytesRead;

                            //    bytes.AddRange(buffer);

                            //} while (bytesRead == 256);

                            //var data = Convert.ToBase64String(bytes.ToArray());

                            var bytes = Encoding.UTF8.GetBytes(data);
                        }
                    }


                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }


                // TODO: save data to filename
                var path = Path.Combine(ConfigurationManager.AppSettings["DocumentFolder"], $"{domain}.fullchain.pem");

            }
        }

        private async Task<Document> GetDocument(string documentName, AmazonS3Client client)
        {
            var request = new GetObjectRequest { BucketName = _bucketName, Key = documentName };
            var response = await client.GetObjectAsync(request);

            return new Document
            {
                Bucket = _bucketName,
                Name = documentName,
                VersionId = response.VersionId,
                Data = await ReadResponseStream(response.ResponseStream, (int)response.ContentLength)
            };
        }

        private async Task<byte[]> ReadResponseStream(Stream responseStream, int length)
        {
            var data = new byte[length];
            await responseStream.ReadAsync(data, 0, length);
            return data;
        }

        private string GetCurrentDocumentVersion(string domain)
        {

            using (var client = S3ClientFactory.GetS3Client(_regionName, _profileName))
            {
                try
                {
                    var iterator = new S3VersionIterator(client, _bucketName, _folder);

                    var version = iterator.Where((v) => { return v.IsLatest && !v.IsDeleteMarker && Path.GetFileName(v.Key).StartsWith(domain); }).FirstOrDefault();
                    if (version == null) return null;

                    return version.VersionId;
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }

            return null;
        }

        private async Task<MetadataCollection> GetMetadata(string objectKey, AmazonS3Client s3Client, string secretVersion = null)
        {
            var getObjMetadataReq = new GetObjectMetadataRequest { BucketName = _bucketName, Key = objectKey };

            GetObjectMetadataResponse metadataResponse = null;
            try
            {
                metadataResponse = await s3Client.GetObjectMetadataAsync(getObjMetadataReq);
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw ex;
            }

            return metadataResponse.Metadata;
        }

    }
}