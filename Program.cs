using System;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SteamGameTopMusicOpener {
    class Program {
        static async Task Main(string[] args) {
            try {
                var app = new Application();
                await app.RunAsync();
            } catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Hata: {ex.Message}");
                Console.ResetColor();
            } finally {
                //Console.WriteLine("\nÇıkmak için bir tuşa bas...");
                Console.ReadKey();
            }
        }
    }
}