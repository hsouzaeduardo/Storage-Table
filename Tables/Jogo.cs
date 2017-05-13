using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tables
{
    public class Jogo : TableEntity
    {
        public Jogo(string pk, string rk)
        {
            this.PartitionKey = pk;
            this.RowKey = rk;
        }

        public Jogo()
        {

        }

        public string TimeA { get; set; }
        public string TimeB { get; set; }
        public string UrlTimeA { get; set; }
        public string UrlTimeB { get; set; }
        public DateTime DataJogo { get; set; }
    }
}
