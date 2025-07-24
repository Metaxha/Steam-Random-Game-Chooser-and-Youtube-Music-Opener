using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using System.Drawing;

namespace SteamGameTopMusicOpener.Services {
    public class SteamService {
        public string GetSteamPath() {
            try {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam")) {
                    return key?.GetValue("SteamPath")?.ToString()?.Replace("/", "\\");
                }
            } catch {
                return null;
            }
        }
        public List<GameInfo> GetInstalledGames() {
            var games = new List<GameInfo>();
            string steamPath = GetSteamPath();

            if (string.IsNullOrEmpty(steamPath))
                return games;

            string libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersPath))
                return games;

            var libraries = GetLibraryFolders(libraryFoldersPath);
            foreach (var lib in libraries) {
                string steamAppsPath = Path.Combine(lib, "steamapps");
                if (!Directory.Exists(steamAppsPath))
                    continue;

                var manifestFiles = Directory.GetFiles(steamAppsPath, "appmanifest_*.acf");
                foreach (var manifest in manifestFiles) {
                    try {
                        var vdf = VdfConvert.Deserialize(File.ReadAllText(manifest));
                        games.Add(new GameInfo {
                            Name = vdf.Value["name"]?.ToString(),
                            AppId = vdf.Value["appid"]?.ToString()
                        });
                    } catch { /* Hatalı manifest dosyalarını atla */ }
                }
            }

            return games;
        }

        private string[] GetLibraryFolders(string vdfPath) {
            var result = new List<string>();
            try {
                var vdf = VdfConvert.Deserialize(File.ReadAllText(vdfPath));
                var libraryFolders = vdf.Value["libraryfolders"];

                if (libraryFolders is VObject foldersObj) {
                    foreach (var folder in foldersObj.Children()) {
                        if (folder is VProperty prop &&
                            prop.Value is VObject obj &&
                            obj["path"] != null) {
                            string path = obj["path"].ToString().Replace(@"\\", @"\");
                            result.Add(path);
                        }
                    }
                }
            } catch { /* Hatalı VDF dosyasını atla */ }

            // Varsayılan Steam kütüphanesini ekle
            string steamPath = Path.GetDirectoryName(Path.GetDirectoryName(vdfPath));
            if (!result.Contains(steamPath) && Directory.Exists(steamPath)) {
                result.Add(steamPath);
            }

            return result.ToArray();
        }
        public GameInfo SelectRandomGame(List<GameInfo> games) {
            if (games == null || games.Count == 0)
                return null;

            var random = new Random();
            return games[random.Next(games.Count)];
        }


        public async Task<Color?> GetDominantColorFromSteamApp(string appId) {
            string url = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/header.jpg";

            using var client = new HttpClient();
            try {
                byte[] imageData = await client.GetByteArrayAsync(url);
                using var ms = new MemoryStream(imageData);
                using var bmp = new Bitmap(ms);

                return GetDominantColor(bmp);
            } catch {
                Console.WriteLine("Resim alınamadı veya analiz edilemedi.");
                return null;
            }
        }
        static Color GetDominantColor(Bitmap bmp) {
            var colors = new Dictionary<Color, int>();

            for (int x = 0; x < bmp.Width; x += 10) {
                for (int y = 0; y < bmp.Height; y += 10) {
                    Color pixel = bmp.GetPixel(x, y);
                    if (pixel.A < 200) continue;

                    Color simplified = Color.FromArgb(pixel.R / 20 * 20, pixel.G / 20 * 20, pixel.B / 20 * 20);
                    if (colors.ContainsKey(simplified))
                        colors[simplified]++;
                    else
                        colors[simplified] = 1;
                }
            }

            return colors.OrderByDescending(kvp => kvp.Value).First().Key;
        }
    }
}