using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Tables
{
    class Program
    {
        static void Main(string[] args)
        {
            CloudStorageAccount conta = 
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["cCon"]);

            //TableMain(conta);
            //StoragemMain(conta);
            QueueMain(conta);


        }

        #region [Storage Queue]
        private static void QueueMain(CloudStorageAccount conta)
        {
            CloudQueueClient filas = conta.CreateCloudQueueClient();

            CloudQueue filaWhatsApp = filas.GetQueueReference("filawhatsapp");

            filaWhatsApp.CreateIfNotExists();

            for (int i = 0; i < 20; i++)
            {
                EnviarMsg(filaWhatsApp, i);
            }
            
            ReceberMsgAsync(filaWhatsApp, "C1");

            ReceberMsgAsync(filaWhatsApp, "C2");

            Console.ReadKey();
        }

        private static async void  ReceberMsgAsync(CloudQueue filaWhatsApp, string chamada)
        {
            CloudQueueMessage cqm;

            QueueRequestOptions opn = new QueueRequestOptions();

            opn.LocationMode = Microsoft
                .WindowsAzure.Storage
                .RetryPolicies.LocationMode.PrimaryOnly;

            while ((cqm = await filaWhatsApp.GetMessageAsync()) != null)
            {
                Console.WriteLine($"{cqm.AsString} \t {chamada}");

                await filaWhatsApp.DeleteMessageAsync(cqm);
            }
        }

        private static void EnviarMsg(CloudQueue filaWhatsApp, int total)
        {
            CloudQueueMessage msg = new CloudQueueMessage($"MINHA MSG NA FILA: {total}");
            filaWhatsApp.AddMessage(msg);
        }
        
        #endregion

        #region [Storage Private]
        private static void StoragemMain(CloudStorageAccount conta)
        {
            var sasToken = BlobUpload(conta);

            DownloadBlob(sasToken);

            Console.Write($"Token:{sasToken}");
        }

        private static void DownloadBlob(string sasToken)
        {
            var meninasContainer = new CloudBlobContainer(new Uri(sasToken));

            var blobs = meninasContainer.ListBlobs();

            foreach (var item in blobs)
            {
                Console.WriteLine(item.Uri);
                var blob = new CloudBlockBlob(item.Uri);
                blob.DownloadToFile($@"C:\Temp\Blob\{blob.Name}"
                    , FileMode.Create);
            }
        }

        private static string BlobUpload(CloudStorageAccount conta)
        {
            var blob = conta.CreateCloudBlobClient();
            var meninasContainer = 
                blob.GetContainerReference("garotas");

            meninasContainer.CreateIfNotExists();

            foreach (var item in Directory.GetFiles(@"c:\Temp\"))
            {
                FileInfo file = new FileInfo(item);

                var meninaBlob =
                    meninasContainer
                    .GetBlockBlobReference(file.Name);

                meninaBlob.Properties.ContentType = $"image/{file.Extension.Replace(".", "")}";
                meninaBlob.UploadFromFile(file.FullName, null, null, null);

                Console.WriteLine("Na garagem");
            }

            SharedAccessBlobPolicy regras = new SharedAccessBlobPolicy();
            regras.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-10);
            regras.SharedAccessExpiryTime = DateTime.UtcNow.AddMonths(10);
            regras.Permissions =  SharedAccessBlobPermissions.List
                                | SharedAccessBlobPermissions.Delete;
            var token = meninasContainer.GetSharedAccessSignature(regras);

            return $"{meninasContainer.Uri}{token}";
        }

        #endregion

        #region [ Tables ]
        private static void TableMain(CloudStorageAccount conta)
        {
            var sasToken = TableInsertAlunos(conta);
            AtualizarJogos(sasToken);
            ExcluirJogos(sasToken);
            ConsultaJogos(sasToken);
            Console.ReadKey();
        }

        private static void ExcluirJogos(string sasToken)
        {
            CloudTable table = new CloudTable(new Uri(sasToken));
            TableOperation remove =
                TableOperation.Delete(new Jogo("A", "3") { ETag = "*"});

            table.Execute(remove);
        }

        private static void ConsultaJogos(string sasToken)
        {
            CloudTable jogos = new CloudTable(new Uri(sasToken));
            TableQuery<Jogo> query =
                new TableQuery<Jogo>()
                .Where(TableQuery
                .GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "A"));
            
            foreach (var jogo in jogos.ExecuteQuery<Jogo>(query))
            {
                Console.WriteLine($"{jogo.DataJogo} - {jogo.TimeA} vs {jogo.TimeB}");
            }
        }
        
        private static void AtualizarJogos(string sasToken)
        {
            CloudTable jogos = new CloudTable(new Uri(sasToken));
            
            TableOperation search =
                TableOperation.Retrieve<Jogo>("A", "3");

            TableResult restult = jogos.Execute(search);

            var entidade = restult.Result as Jogo;

            entidade.DataJogo = entidade.DataJogo.AddDays(2);

            TableOperation update = TableOperation.Replace(entidade);

            jogos.Execute(update);
        }

        private static string TableInsertAlunos(CloudStorageAccount conta)
        {

            CloudTableClient tableClient = conta.CreateCloudTableClient();
            CloudTable jogos = tableClient.GetTableReference("jogos");
            jogos.CreateIfNotExists();
            Jogo santosFluminense = new Jogo("A", "1")
            {
                TimeA = "Flu",
                TimeB = "SAN",
                DataJogo = new DateTime(2017, 5, 13)
            };

            Jogo palvasco = new Jogo("A", "2")
            {
                TimeA = "PAL",
                TimeB = "VASCO",
                DataJogo = new DateTime(2017, 5, 14)
            };

            Jogo corponte = new Jogo("A", "3")
            {
                TimeA = "COR",
                TimeB = "PONTE",
                DataJogo = new DateTime(2017, 5, 14)
            };

            TableBatchOperation lote = new TableBatchOperation();
            lote.Add(TableOperation.Insert(santosFluminense));
            lote.Add(TableOperation.Insert(palvasco));

           // jogos.ExecuteBatch(lote);

            jogos.Execute(TableOperation.Insert(corponte));

            SharedAccessTablePolicy regra = new SharedAccessTablePolicy();

            regra.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            regra.SharedAccessExpiryTime = DateTime.UtcNow.AddDays(2);
            regra.Permissions =
                SharedAccessTablePermissions.Delete
                | SharedAccessTablePermissions.Query
                | SharedAccessTablePermissions.Update;
            var sasToken =
                jogos.GetSharedAccessSignature(regra, null, null, null, null, null);
            
            return $"{jogos.Uri}{sasToken}";    
        }
        #endregion

       
       
    }
}
