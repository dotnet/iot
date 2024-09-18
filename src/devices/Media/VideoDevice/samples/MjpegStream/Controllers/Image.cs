// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Graphics;
using Iot.Device.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Iot.Device.Graphics.SkiaSharpAdapter;

namespace CameraIoT.Controllers
{
    /// <summary>
    /// The main image controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private readonly Camera _camera;

        /// <summary>
        /// Controller creation
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="camera">The camera singleton</param>
        public ImageController(ILogger<ImageController> logger, Camera camera)
        {
            _logger = logger;
            _camera = camera;
        }

        /// <summary>
        /// Get a single image http(s)://url/image
        /// </summary>
        /// <returns>A JPEG Image</returns>
        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                return File(_camera.TakePicture(), "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Get an MJPEG stream http(s)://url/image/stream
        /// </summary>
        [HttpGet("stream")]
        public void GetStream()
        {
            var bufferingFeature = HttpContext.Response.HttpContext.Features.Get<IHttpResponseBodyFeature>();
            bufferingFeature?.DisableBuffering();

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.ContentType = "multipart/x-mixed-replace; boundary=--frame";
            HttpContext.Response.Headers["Connection"] = "Keep-Alive";
            HttpContext.Response.Headers["CacheControl"] = "no-cache";
            _camera.NewImageReady += WriteBufferBody;

            try
            {
                _logger.LogWarning($"Entering streaming loop");
                _camera.StartCapture();
                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in streaming: {ex}");
            }
            finally
            {
                HttpContext.Response.Body.Close();
                _logger.LogInformation("End of streaming");
            }

            _camera.NewImageReady -= WriteBufferBody;
            _camera.StopCapture();
        }

        private async void WriteBufferBody(object sender, NewImageBufferReadyEventArgs e)
        {
            try
            {
                await HttpContext.Response.BodyWriter.WriteAsync(CreateHeader(e.Length));
                await HttpContext.Response.BodyWriter.WriteAsync(e.ImageBuffer.AsMemory().Slice(0, e.Length));
                await HttpContext.Response.BodyWriter.WriteAsync(CreateFooter());
            }
            catch (ObjectDisposedException)
            {
                // ignore this as its thrown when the stream is stopped
            }

            ArrayPool<byte>.Shared.Return(e.ImageBuffer);
        }

        /// <summary>
        /// Get an modified MJPEG stream http(s)://url/image/modified
        /// </summary>
        [HttpGet("modified")]
        public void GetModifiedStream()
        {
            var bufferingFeature = HttpContext.Response.HttpContext.Features.Get<IHttpResponseBodyFeature>();
            bufferingFeature?.DisableBuffering();

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.ContentType = "multipart/x-mixed-replace; boundary=--frame";
            HttpContext.Response.Headers["Connection"] = "Keep-Alive";
            HttpContext.Response.Headers["CacheControl"] = "no-cache";
            _camera.NewImageReady += WriteModifiedBufferBody;

            try
            {
                _logger.LogWarning($"Entering streaming loop");
                _camera.StartCapture();
                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in streaming: {ex}");
            }
            finally
            {
                HttpContext.Response.Body.Close();
                _logger.LogInformation("End of streaming");
            }

            _camera.NewImageReady -= WriteModifiedBufferBody;
            _camera.StopCapture();
        }

        private async void WriteModifiedBufferBody(object sender, NewImageBufferReadyEventArgs e)
        {
            try
            {
                // using System.Drawing has serious performance implications in the context of video streaming from low powered devices,
                // here is a 'simple' example of modifying the image, which will not be fast enough in most use cases.
                using var stream = new MemoryStream(e.ImageBuffer.AsMemory().Slice(0, e.Length).ToArray());
                var myBitmap = BitmapImage.CreateFromStream(stream);
                var g = myBitmap.GetDrawingApi();
                g.DrawText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Tahoma", 20, Color.White, new Point(0, 0));
                using (var ms = new MemoryStream())
                {
                    myBitmap.SaveToStream(ms, ImageFileType.Jpg);

                    ms.Position = 0;
                    await HttpContext.Response.BodyWriter.WriteAsync(CreateHeader(e.Length));
                    await HttpContext.Response.BodyWriter.WriteAsync(ms.ToArray());
                    await HttpContext.Response.BodyWriter.WriteAsync(CreateFooter());
                }
            }
            catch (ObjectDisposedException)
            {
                // ignore this as its thrown when the stream is stopped
            }

            ArrayPool<byte>.Shared.Return(e.ImageBuffer);
        }

        /// <summary>
        /// Create a MJPEG header.
        /// </summary>
        /// <param name="length">The length of the data</param>
        /// <returns></returns>
        private byte[] CreateHeader(int length)
        {
            string header =
                "--frame\r\n" +
                "Content-Type:image/jpeg\r\n" +
                "Content-Length:" + length + "\r\n\r\n";

            return Encoding.ASCII.GetBytes(header);
        }

        /// <summary>
        /// Create the MJPEG footer
        /// </summary>
        /// <returns></returns>
        private byte[] CreateFooter()
        {
            return Encoding.ASCII.GetBytes("\r\n");
        }
    }
}
