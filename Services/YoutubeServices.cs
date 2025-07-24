using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace SteamGameTopMusicOpener.Services {
    public class YoutubeService {
        private readonly YoutubeClient _youtube;
        private readonly ConsoleService _console;

        public YoutubeService(ConsoleService consoleService) {
            _youtube = new YoutubeClient();
            _console = consoleService;
        }

        public async Task OpenGameMusicFullscreen(string gameName) {
            try {
                _console.WriteSlow($"\n🔍 '{gameName}' müziği aranıyor...", color: ConsoleColor.Yellow);

                // 1. Videoları topla ve listele (önemli!)
                var videoResults = await _youtube.Search
                    .GetVideosAsync($"GAME '{gameName}' soundtrack or OST or MUSIC")
                    .CollectAsync(5);

                // 2. Listeye çevir, yoksa hata verir!
                var videoList = videoResults.ToList();

                // 3. Her videonun detayını asenkron al (koleksiyona dokunma artık!)
                var videoDetailTasks = videoList.Select(async result => {
                    try {
                        return await _youtube.Videos.GetAsync(result.Id);
                    } catch {
                        return null;
                    }
                }).ToArray(); // ToArray de olur

                // 4. Videoları bekle
                var videoDetails = await Task.WhenAll(videoDetailTasks);

                if (videoDetails == null) {
                    _console.WriteLineSlow("Uygun müzik videosu bulunamadı!", color: ConsoleColor.Red);
                    return;
                }

                // 5. Filtrele
                var bestVideo = videoDetails
                    .Where(v => v != null)
                    .Where(v => IsMusicVideo(v))
                    .OrderByDescending(v => v.Engagement.ViewCount)
                    .FirstOrDefault();


                // Tam ekran embed linki
                string embedUrl = $"https://www.youtube.com/embed/{bestVideo.Id}?autoplay=1&fs=1&rel=0";
                string normalUrl = $"https://www.youtube.com/watch?v={bestVideo.Id}&autoplay=1";

                _console.WriteLineSlow($"\n🎵 Bulunan: {bestVideo.Title}", color: ConsoleColor.Cyan);
                _console.WriteLineSlow($"👀 Görüntülenme: {bestVideo.Engagement.ViewCount:N0}", color: ConsoleColor.Gray);
                _console.WriteSlow("▶️ Açılıyor...", color: ConsoleColor.Green);

                Process.Start(new ProcessStartInfo {
                    FileName = normalUrl,
                    UseShellExecute = true
                });
            } catch (Exception ex) {
                _console.WriteLineSlow($"Hata: {ex.Message}", color: ConsoleColor.Red);
            }
        }

        private bool IsMusicVideo(Video video) {
            return video.Duration < System.TimeSpan.FromMinutes(15) &&
                  (video.Title.Contains("soundtrack", System.StringComparison.OrdinalIgnoreCase) ||
                   video.Title.Contains("ost", System.StringComparison.OrdinalIgnoreCase));
        }
    }
}