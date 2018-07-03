using Amazon.S3;
using Amazon.S3.Model;
using System.Collections;
using System.Collections.Generic;

namespace HangfireService.Plugins.AWS
{
    class S3VersionIterator : IEnumerable<S3ObjectVersion>
    {
        private readonly AmazonS3Client _client;

        private readonly string _bucket;
        private readonly string _folder;

        private ListVersionsResponse _lastResponse;
        private string _nextVersionMarker;

        public S3VersionIterator(AmazonS3Client client, string bucket, string folder)
        {
            _client = client;
            _bucket = bucket;
            _folder = folder;
        }

        public IEnumerator<S3ObjectVersion> GetEnumerator()
        {
            _nextVersionMarker = null;
            _lastResponse = null;

            do
            {
                var request = new ListVersionsRequest { BucketName = _bucket, Prefix = _folder, KeyMarker = _nextVersionMarker };
                _lastResponse = _client.ListVersionsAsync(request).Result;

                foreach(var version in _lastResponse.Versions)
                {
                    yield return version;
                }

                _nextVersionMarker = _lastResponse.NextKeyMarker;

            } while (_lastResponse.IsTruncated);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
