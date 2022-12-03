using DotnetUploadFile2022.BackgroundTasks;
using DotnetUploadFile2022.Services;
using Microsoft.AspNetCore.Mvc;
using Xabe.FFmpeg;

namespace DotnetUploadFile2022.Controllers
{
    public class HomeController:Controller
    {
        private FileManager _fileManager;
        private IBackgroundQueue _queue;
        private string _dir;

        public HomeController(
            FileManager fileManager,
            IBackgroundQueue queue,
            IWebHostEnvironment env)
        {
            _fileManager = fileManager;
            _queue = queue;
            _dir = env.WebRootPath;
        }

        public IActionResult Index() => View(_fileManager.GetOptimizedFiles());

        [HttpPost]
        public IActionResult Index(IFormFile file)
        {
            _fileManager.SaveFileOptimize(file);
            return RedirectToAction("Index");
        }

        public IActionResult GetImage(int id, int width) =>
           new FileStreamResult(_fileManager.GetImageStream(id, width), "image/*");

        public IActionResult DownloadImage(int id) =>
     File(_fileManager.GetImageStream(id, 0), "image/*", "image.png");

        
        public IActionResult Video()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Video(double start, double end)
        {
            _queue.QueueTask(async token =>
            {
                await ConvertVideo(start, end, token);
            });

            return RedirectToAction("Video");
        }

        public async Task<bool> ConvertVideo(double start, double end, CancellationToken ct)
        {
            try
            {
                var input = Path.Combine(_dir, "file.mp4");
                var output = Path.Combine(_dir, "converted.mp4");

                FFmpeg.SetExecutablesPath(Path.Combine(_dir, "ffmpeg"));

                var startSpan = TimeSpan.FromSeconds(start);
                var endSpan = TimeSpan.FromSeconds(end);
                var duration = endSpan - startSpan;

                var info = await FFmpeg.GetMediaInfo(input);

                var videoStream = info.VideoStreams.First()
                    .SetCodec(VideoCodec.h264)
                    .SetSize(VideoSize.Hd480);
                    //.Split(startSpan, duration);

                await FFmpeg.Conversions.New()
                    .AddStream(videoStream)
                    .SetOutput(output)
                    .Start(ct);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return true;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            using (var fileStream =
                new FileStream(Path.Combine(_dir, "file.mp4"), FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream);
            }

            return Ok();
        }

    }
}
