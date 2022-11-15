// <copyright file="CreateManifest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AZDEV_DSP_FA
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
   using System.Text;
  
    /// <summary>
    /// CreateManifest description.
    /// </summary>
    public class CreateManifest
    {
        private string _Name;
        private Dictionary<string, string> _Rows;
        private string _Conn;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateManifest"/> class.
        /// </summary>
        /// <param name="service">service.</param>
        public CreateManifest()
        {
        }
        /// <summary>
        /// Run.
        /// </summary>
        /// <param name="req">req.</param>
        /// <param name="log">log.</param>
        /// <param name="context">context.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("CreateManifest")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = data?.name;

            Dictionary<string, string> rowsdict = this.GetTableData(data);

            if (await this.Create(name, rowsdict))
            {
                return new OkObjectResult("{\"created\":\"OK\"}");
            }
            else
            {
                return new ObjectResult("{\"created\":\"Failed\"}");
            }
        }
            public async Task<bool> Create(string Name, Dictionary<string, string> Rows)
            {
                this._Name = Name;
                this._Rows = Rows;
                this._Conn = Environment.GetEnvironmentVariable("StorageConnectionAppSetting");
                string storageAccount_connectionString = this._Conn;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccount_connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(this._Name);

                try
                {
                    CloudBlobDirectory dira = container.GetDirectoryReference(string.Empty);

                    //specified container with all blobs
                    var rootDirFolders = dira.ListBlobsSegmentedAsync(true, BlobListingDetails.Metadata, null, null, null, null).Result;
                    var sb = new StringBuilder();
                    int NumberofFileMatched = 0;

                    //Loop through all files in a specified container
                    foreach (var blob in rootDirFolders.Results)
                    {
                        CloudBlockBlob blob2 = (CloudBlockBlob)blob;

                        //Checking ADF pipeline Specified file list contians aspecific blob from container.If match then adding to string builder for manifest records.
                        if (this._Rows.ContainsKey(blob2.Name))
                        {
                            // blob2.Name -> File name from Container, _Rows[blob2.Name]-> count from ADF pipeline parameter values  , blob2.Properties.ETag-> ETag from blob
                            NumberofFileMatched = NumberofFileMatched + 1;
                            sb.AppendLine(string.Format("{0},{1},{2},{3}", blob2.Name, this._Rows[blob2.Name], blob2.Properties.ETag, blob2.Properties.LastModified.Value.ToString("yyyy'-'MM'-'dd'T'HHmmss")));
                        }
                    }
                    // No empty Manifest file creation. Also All files from ADF pipeline parameters should exist in container.
                    if (sb.Length > 0 && NumberofFileMatched.Equals(Rows.Count))
                    {
                        await container.GetBlockBlobReference(string.Format("manifest{0}.chk", DateTime.Now.ToString("'_'yyyyMMdd'_'HHmmss"))).UploadTextAsync(sb.ToString());
                        return true;
                    }
                    else
                    {
                        throw new Exception("All files from ADF pipeline parameters should exist in container or No empty Manifest file creation");
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: ", e);
                    throw;
                }
            }

            ///<summary>
            /// Dictionary object contians file name with extention as Key  and Count as Value. Spliting second parameter into two parts. Filename and count.
            ///<Example>
            ///{
            ///     "name": "input",
            ///      "values": [
            ///      "test.csv|2",
            ///      "test_115035.csv|3"
            ///                 ]
            ///}
            /// </Example>
            /// </summary>  
            public Dictionary<string, string> GetTableData(dynamic data)
            {
                var rowsdict = new Dictionary<string, string>();

                for (int i = 0; i < data.values.Count; i++)
                {
                    string s = data.values[i].ToString();
                    string[] sp = s.Split('|');
                    rowsdict.Add(sp[0], sp[1]);
                }

                return rowsdict;
            }
        }
        
}
