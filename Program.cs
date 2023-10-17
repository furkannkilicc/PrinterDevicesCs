using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    private static Dictionary<int, string> printerDictionary = new Dictionary<int, string>();
    private static readonly string nodeJsEndpoint = "http://localhost:8080/printers";
    private static readonly string nodeJsPrintRequestEndpoint = "http://localhost:8080/print";

    private static void ListPrinters()
    {
        Console.WriteLine("Yazıcılar:");
        int index = 1;
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            printerDictionary[index] = printer;
            Console.WriteLine($"{index}. {printer}");
            index++;
        }
    }

    private static async Task SendPrinterListToNodeJSAsync(Dictionary<int, string> printerList)
    {
        var printerData = new
        {
            Printers = printerList.Values.ToList()
        };

        string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(printerData);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        using (HttpClient client = new HttpClient())
        {
            var response = await client.PostAsync(nodeJsEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Yazıcı listesi başarıyla gönderildi.");
                await ReceivePrinterAndDocumentFromNodeJS();
            }
            else
            {
                Console.WriteLine("Yazıcı listesi gönderilirken hata oluştu. Durum Kodu: " + response.StatusCode);
            }
        }
    }

    private static async Task ReceivePrinterAndDocumentFromNodeJS()
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = null;

            while (response == null)
            {
                // Belge ve yazıcıyı beklemek için Node.js'den bir istek yapın.
                response = await client.GetAsync(nodeJsPrintRequestEndpoint);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Yazıcı ve belge bekleniyor...");
                    await Task.Delay(1000); 
                }
                else
                {
                    //  
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Node.js'den gelen yanıt: " + responseContent);

   
                }
            }
        }
    }

    static async Task Main(string[] args)
    {
        ListPrinters();
        await SendPrinterListToNodeJSAsync(printerDictionary);

        
        Console.WriteLine("Uygulama çalışıyor. Çıkmak için bir tuşa basın.");
        Console.ReadKey();
    }
}