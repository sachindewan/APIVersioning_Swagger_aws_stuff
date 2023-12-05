using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;
using Amazon.CloudFront.Model;
using Amazon.CloudFront;
using System.ComponentModel.DataAnnotations;
using Amazon.Runtime;
using QueryEngine.Application.Exceptions;
using System.Security.Cryptography;
using ThirdParty.BouncyCastle.OpenSsl;
using ThirdParty.BouncyCastle.Asn1;
using ThirdParty.BouncyCastle.Utilities.IO.Pem;
using System.Reflection.PortableExecutable;
using System.Collections;
using ThirdParty.BouncyCastle.Utilities;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace FileManager
{
    public class FileManagerService : IFileManagerService
    {
        private readonly IAmazonS3 client;
        private readonly IAmazonCloudFront _cloudFrontClient;
        public FileManagerService(IAmazonS3 client, IAmazonCloudFront cloudFrontClient)
        {
            this.client = client;
            _cloudFrontClient = cloudFrontClient;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UploadAsync(dynamic data)
        {
            var key = Path.GetRandomFileName() + Guid.NewGuid().ToString();
            //MemoryStream stream = SerializeToStream(data);
            //byte[] byteArray = Encoding.ASCII.GetBytes(data);
            //MemoryStream stream = new MemoryStream(byteArray);

            var objectRequest = new PutObjectRequest()
            {
                BucketName = "query-engine-test-bucket",
                Key = key,
                //InputStream = stream,
               ContentBody = JsonConvert.SerializeObject(data),
                ContentType = "application/json"
            };
            throw new QueryEngine.Application.Exceptions.ApplicationException("Bucket already exist");
            //await client.PutObjectAsync(objectRequest);
            return true;
        }
        public RSAParameters ReadPrivatekey(TextReader reader)
        {
            PemObject pemObject = ReadPemObject1(reader);
            pemObject.Type.Substring(0, pemObject.Type.Length - "PRIVATE KEY".Length).Trim();
            Asn1Sequence asn1Sequence = (Asn1Sequence)Asn1Object.FromByteArray(pemObject.Content);
            if (asn1Sequence.Count != 9)
            {
                throw new Exception("malformed sequence in RSA private key");
            }

            return new RSAParameters();//convertSequenceToRSAParameters(asn1Sequence);
        }
        public PemObject ReadPemObject1(TextReader reader)
        {
            string text = reader.ReadLine();
            if (text != null && text.StartsWith("-----BEGIN "))
            {
                text = text.Substring("-----BEGIN ".Length);
                int num = text.IndexOf('-');
                string type = text.Substring(0, num);
                if (num > 0)
                {
                    return LoadObject(type,reader);
                }
            }

            return null;
        }

        private PemObject LoadObject(string type,TextReader reader)
        {
            string text = "-----END " + type;
            IList list = new List<PemHeader>();
            StringBuilder stringBuilder = new StringBuilder();
            string text2;
            while ((text2 = reader.ReadLine()) != null && text2.IndexOf(text) == -1)
            {
                int num = text2.IndexOf(':');
                if (num == -1)
                {
                    stringBuilder.Append(text2.Trim());
                    continue;
                }

                string text3 = text2.Substring(0, num).Trim();
                if (text3.StartsWith("X-"))
                {
                    text3 = text3.Substring(2);
                }

                string val = text2.Substring(num + 1).Trim();
                list.Add(new PemHeader(text3, val));
            }

            if (text2 == null)
            {
                throw new IOException(text + " not found");
            }

            if (stringBuilder.Length % 4 != 0)
            {
                throw new IOException("base64 data appears to be truncated");
            }

            return new PemObject(type, list, Convert.FromBase64String(stringBuilder.ToString()));
        }
        public async Task<string> GenerateUrl()
        {
            // Your secret key for signing (keep this secret!)
           // string keyPairId = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvd8uwvlsm27lPduFmEzT1EpDT7cAgqJQeHDKM++PvTIZhpeZ2rLrQjwu7XukHoeJaukeVIitZ8uj5FfOe7njvE6Z0isWdxveaWXfFXbqmd3Zuun4YU+lK+qmIzTmN2inylv85FWpCdeC8ySMCZcwm91bT8b51dB36uWbdfngNR1qQLbM7MkY0vRaz84tgQloeXRDPZFg/S8TTJ8rjt5g5i7wMCgCjejlv/nNOaTc5qqVYBBaGtfWbmFb23PPOF2fe93JL0WQ77QALmW+mCFls3PhSj51WXYfglos0rIexVDDm+VoOf74/6FN5/vQT9wiSP93J2YyJ8/tKANyDIb5OwIDAQAB";
            string privateKey = "-----BEGIN PRIVATE KEY-----\nMIIEowIBAAKCAQEAmy47WokEiIMVQN4AkjAxhg5ZB2cDr8fUyVWUBohlMTDZ9LxJ53YFu+RbDmQn3GGRjSdUgf96dUDTm0SdEQ1m2pmpJ+neIl2yYcDLAfTVdm9BZBCaRm89g+hG82pwHigBK7kblK6BCI/S/J5hB49M3EcdJrXkUo+wyKxqgwRwAyPwQ0GnNk7OhzytK/BBdeEiCZHT9Vd4WbcGk0c5GlgYxQ5AlE3UexXuXE69Uy2nE0nC1lwQHCgjv8bc9zciVxgviop/lpPCgzkJ5Do5fc8btn6EkQV2sl/6sDLacBwVM9DFlBrkqWrLXxbQcw+HSJZA+7iUxOXpekYrEaOPzlB8qQIDAQABAoIBAAo5HfzlaPCLXjOTTItU9HZF7LSRo0sJyaln42QYyT3X34wBeYbvngoWTa9hhsS8gse/TxBvYcRmvGoOt6A/d0awd9Xi0NkHvjA1cMpUtFlkk/TBKUC5pF5Tx5TSrsc10HnubHQ7mNGVFAvDbVX/qUsbvj5mHkAYKkfWIAA4ox55vdkVRjuvdvCqIFrwn1LRvGGg+jTrObCfoGKSUT+Kj/btAdq1eo2jF5ad6VgZCUUL/auSoVTK+ub5Z2Jj28G1PaZp4Vk+lDdfDurhIXDoOaN9HyS99nrdGPW6QpOZC0/M/3cTjAiNbI8Ug0LulhA5CNb89SxD7C0tV2yrxmRZJLkCgYEA0HqObMlwwklpW0NKzUSR/I7KVi54HEO5sb6mBizxp8T3FGmSxbyHixCBU+1B5/U74/3D2Cf7AOmEASWHwbfbg0K+9SCDgT9BKHTeebYTL4InagW4Lh9CDP6Qmd83jS5bBgd1pL6gGnExzbb+oCQPIR3v2bodifLbCrXV7PdTElUCgYEAvo2MKibM3/igPMRFYfaa4c3UCqoegDv6Xb4WDC/36vfKhNt3D90zzbc2c9zmiVzj0Yfoaze5Dq4KvuViwu1Een6NB6b0kTI5JUzYwCnTT6+2+Tqvm9VIsIr9sculPaWQWvQVGbnoH7B96Jbl4ryUlJWbLwHS3x9yNLmnNcqnnQUCgYBynu8vYJFeQH5sbtLxiIG7GcalG2duIs41q1wciRlT5Db0QhcwIDdlfe2c9xUFMw96ikrizRhzp++rsbayCyPlRw02OGoU5XItjBPFVxW1SRnicMKSmRz3h/54mwEv3gytg+xqZ2QfJZz8PWBBK5Il4w+75VfYQG2ttcuoPVKvAQKBgHuxiqD6GtfwnqOTjK5w1E1rvQ9HKxF23ajocYtrv/Uo8K+ZfwovPyd0nMZNSOE2CTTEkly3CUu8MoGEib2bI26Yo5eqCuwNvAa4gaMMA32gxJe4PrJbmxnuv4wgc5020iQYeXH/ueyINtM60POEErb4/olfF7F6yjnrod1LNc65AoGBAMmfpddtLANoJDH+vUWXRntC4hGSJz6bUk0PvucUAhFqZeyWyYOXnKyy6na63P57AbLw3hnz6Q0pVVsuUHSUOo6Fvkk7yVrddo8UH3mgH96l1tNr0CxwvEiwHk3h3p/rUOehNYjkqXbCP3xyy6pUwi7OBBMnpt4nKhit6FTRour2\n-----END PRIVATE KEY-----";
            //string privateKey1 = "-----BEGIN PRIVATE KEY-----\nMIIEpAIBAAKCAQEA4i/wALMPoZOI59J6BnbFJIl+Q6nanRTE00uAMS+LVGHPoY0j3yJ0GHfkdzEcnvt7dNSIRwio6OTfA+10Y1HcsHwW3GBRE+yQF3xaKupUAUtBofxFWinAxQam2PnlIzxI9kKMZ2yDwwXzVd5lnst8tq12rk2Phe8Q4bOwjvRaHsuTQMPq/U/XRMq+1Vtr00zuhwVtNAIG3nJRQY1+HtUX9ZrW2+IH3xETAPgn5HzHDpjxjGEz1s5ifCSQz3CbeVPSoD5/KQfLKVaGFtnGKNeSO/Gp8UeNp0xvreH21jn22UzqUNbTNNi90ZnFztOH4wIPfLPOcbKx0ZWoOVzMOl2PVQIDAQABAoIBAELwEuPyGgckRVjt5iTxk0hL1G0r4EGqetFZP7fOrTostCjIPzxy0s7G9Hr9ss2t+QxLNYVoH6zExuc/p7kY6enk31MawgPWd5Irv/eBX5j+b48DvHaBcsYcQNZnphM2bRVL/odBCwF1Sl04MvS6nc7mKQBOfdISPPm+ltrHn8AKyrS+nwS1t5uMvLhbQyRXILgPQEKZ6Dp3z58wlVqt7UGUFlPl8jxnuWn2uD8ezzemMXeCThyHYrZGXwSh60DWLkF5xKMyjYBMwzjnahtj5xrpVjzRN5wlyFd0ScqPjzAw4eWXJhuFuAcHFUWYpFHjlpCL6piY5Ze49xeZhy8KSyECgYEA9cb2tTUZVDj6rUUjbTYWdUK8YEMq4QRml/OX2kmJ3WimQhrQweod+631T46A2qvpKx8U6n0Qxu6HrZAyRCJ1KX02IieA6tPE5qVo+J3jckFTdiZNSBJOpeWvptZxnIW+fdkLEjNaOqkW8AI9eM8J+PxJmcqbmqVeIgAWzOFc7ckCgYEA65hhSGAgfcavADfgTiG+QP7pDEhjlyY+pT3ajEF8ER/l/0Jj8EaTDjiLd8/JSVeKf7IpYSNJfYrhJoYSWXe3kC5544iQ8CmBlCzjbN+0v6cxVSv8fzEo+ERJr2fQkOwBz4W1FODeLGcZwHJsTChB8fAWbY+rDUqe4+r+V2ffKy0CgYBCuyzM8ofYCEh3tyfxRRg/6ki/Uj1YLxuz8h2u1PyrsvhBRoqbu/c8GHVcjnKJ7Nc4MA47xI1DlgwcoeQj/78T7r099L/aAnmZrhucNTJGGVqR38Bn6yCThpN0lxDt0JpyDPYmkO7UsohZziHmpUAt7EIHka1CBdhTHvcR1bankQKBgQC3R4wm6eFc97wcx1tQbR+9IJHcZFRVugLyOfRTOH50NASRV+y3d6/fWE3nn1ZQIDI2tTtLvk/lwqz1c0fDIkdDe2SHhi57J08PHvyuLjpG/qJ6SAXeTTg6K6XHN1Qh6fheoP4GiAlPMcXu2RU3CfqDZOuNwD6p0Gtbk+zZwozy4QKBgQCVSUzOMmGwXSYnir3zIgISttqpARtundUPXtpmGzMO47nUKSR5yhTZ/0EDRo4tU2GZWN5TeJGobBEmcnFNYctaQTfLRhnBozxywZcJTvlk45s1Na/4wSvqvypUGTlvj0O2Za2B/Dd8dwiXxx0YETzWEQl9Yx/6otTgndDrtMyotg==\n-----END PRIVATE KEY-----";
            //// URL to be signed
            //RSAParameters rSAParameters;
            //try
            //{
            //    TextReader textReader1 = new StringReader(privateKey);
            //    TextReader textReader2 = new StringReader(privateKey1);

            //    var v = new ThirdParty.BouncyCastle.OpenSsl.PemReader(textReader1);

            //    ReadPrivatekey(textReader1);


            //    var v1 = new ThirdParty.BouncyCastle.OpenSsl.PemReader(textReader2);
            //    ReadPrivatekey(textReader2);

            //}
            //catch (Exception innerException)
            //{
            //    throw new AmazonClientException("Invalid RSA Private Key", innerException);
            //}

            //string cloudFrontUrl = "https://d2nbl8bn7nz9ef.cloudfront.net/test.txt";

            //// Set the expiration time (e.g., 1 hour from now)
            //DateTime expiryTime = DateTime.UtcNow.AddMinutes(2);
            //long expiryTimestamp = (long)(expiryTime - new DateTime(1970, 1, 1)).TotalSeconds;

            // Generate the signature
           // string signedUrl = SignCloudFrontUrl(cloudFrontUrl, keyPairId, privateKey, expiryTimestamp);

            // Append the signature and expiry timestamp to the URL
            //string signedUrl = $"{urlToSign}&signature={signature}&expires={expiryTimestamp}";

            //Console.WriteLine($"Original URL: {urlToSign}");
            TextReader textReader = new StringReader(privateKey);
            var url2 = AmazonCloudFrontUrlSigner.GetCannedSignedURL("https://d2nbl8bn7nz9ef.cloudfront.net/test.txt", textReader, "KB2ZXI6C3EEMX", DateTime.UtcNow.AddMinutes(2));

            var pk = new StreamReader(@"C:\Users\sachin.kumar16\Downloads\pk-APKA3UUSCCNLUDROHAOE.pem");
           // var d  = new StreamReader()
            //var rSAParameters = new PemReader(textReader).ReadPrivatekey();
    //        var url = AmazonCloudFrontUrlSigner.GetCannedSignedURL("https://d2nbl8bn7nz9ef.cloudfront.net/test.txt", pk, "KB2ZXI6C3EEMX", DateTime.UtcNow.AddMinutes(1));
    //        FileInfo privateKey12 = new FileInfo(@"C:\Users\sachin.kumar16\Downloads\pk-APKA3UUSCCNL4AVSVMV4.pem");
    //        var url1 = AmazonCloudFrontUrlSigner.GetCannedSignedURL(
    //AmazonCloudFrontUrlSigner.Protocol.http,
    //"https://d2nbl8bn7nz9ef.cloudfront.net",
    //privateKey12,
    //"test.txt",
    //"",
    //DateTime.Now.AddSeconds(20));
            //var da = AmazonCloudFrontUrlSigner.ge(("https://d2nbl8bn7nz9ef.cloudfront.net/test.txt", textReader, keyPairId, DateTime.UtcNow.AddMinutes(2));
            //var urlSigner = AmazonCloudFrontUrlSigner.;
            //var request = new GetDistributionConfigRequest
            //{
            //    Id = "E2W3DKYMQWFR4L"
            //};
            //var response = await _cloudFrontClient.GetDistributionConfigAsync(request);

            //var distributionConfig = response.DistributionConfig;
            //var domainName = distributionConfig;
            return "";
            //return $"https://{domainName}/xdb2xsah.grhe195dbb7-dad9-4a45-a9b3-450e55ed2c2f";
        }
        static DateTime UnixTimeStampToDateTime(long unixTimestamp)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return unixEpoch.AddSeconds(unixTimestamp).ToLocalTime();
        }
        public MemoryStream SerializeToStream(dynamic o)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string> FolderExistAsync()
        {

            var bucketName = "query-engine-test-bucket";
            var folderKey = "arvhkybb.h5h57bf7e9f-dd26-4724-b21d-9977620c60c4"; // Note the trailing slash to specify a folder

            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = folderKey
            };

            var response = await client.ListObjectsV2Async(request);

            if (response.S3Objects.Count > 0)
            {
                // The folder exists; you can proceed to retrieve the files within it.
                return  await Task.FromResult("true");
            }
            else
            {
                return await Task.FromResult("false");
            }
        }
    }
    
}