using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UserFileSyncCli
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: UserFileSync [pathToCsv] [appId] [baseUrl]");
                return;
            }

            try
            {
                var fileContent = File.ReadAllBytes(args[0]);
                var base64 = Convert.ToBase64String(fileContent);

                var baseAddress = $"{args[2]}/api/user/AutomaticSyncProcess";

                var http = (HttpWebRequest)WebRequest.Create(new Uri(baseAddress));
                http.Accept = "application/json";
                http.ContentType = "application/json";
                http.Method = "POST";

                string parsedContent = $"{{\"syncAppId\":\"{args[1]}\",\"fileContent\":\"{base64}\"}}";
                ASCIIEncoding encoding = new ASCIIEncoding();
                Byte[] bytes = encoding.GetBytes(parsedContent);

                Stream newStream = http.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();

                var response = http.GetResponse();

                var stream = response.GetResponseStream();
                var sr = new StreamReader(stream);
                var content = sr.ReadToEnd();
                Console.WriteLine(content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
