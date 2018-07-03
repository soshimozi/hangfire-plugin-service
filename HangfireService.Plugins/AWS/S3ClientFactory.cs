using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using System.Collections.Generic;

namespace HangfireService.Plugins.AWS
{
    static class S3ClientFactory
    {
        public static AmazonS3Client GetS3Client(string region, string profile = null)
        {
            return new AmazonS3Client(GetAwsCredentials(profile), new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(region) });
        }

        private static AWSCredentials _savedCredentials;

        private static AWSCredentials GetAwsCredentials(string profile)
        {
            return _savedCredentials ?? (_savedCredentials = BuildCredentials(profile));
        }

        private static AWSCredentials BuildCredentials(string profile)
        {
            if (!string.IsNullOrEmpty(profile))
            {
                var credentialProfileStoreChain = new CredentialProfileStoreChain();
                if (credentialProfileStoreChain.TryGetAWSCredentials(profile, out AWSCredentials credentials))
                {
                    return credentials;
                }
            }

            FallbackCredentialsFactory.CredentialsGenerators = new List<FallbackCredentialsFactory.CredentialsGenerator>
            {
                () => GetDefaultCredentials(),
                () => new EnvironmentVariablesAWSCredentials()
            };

            return FallbackCredentialsFactory.GetCredentials();
        }

        private static AWSCredentials GetDefaultCredentials()
        {
            var credentialProfileStoreChain = new CredentialProfileStoreChain();
            if (credentialProfileStoreChain.TryGetAWSCredentials("default", out AWSCredentials defaultCredentials))
            {
                return defaultCredentials;
            }

            throw new AmazonClientException("Unable to find a default profile in CredentialProfileStoreChain.");
        }

    }
}
