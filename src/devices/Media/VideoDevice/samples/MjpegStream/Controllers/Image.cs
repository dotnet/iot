// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        private bool _isNewImage;
        private byte[]? _lastImage;

        /// <summary>
        /// Controller creation
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="camera">The camera singleton</param>
        public ImageController(ILogger<ImageController> logger, ICamera camera)
        {
            _logger = logger;
            _camera = (Camera)camera;
            _isNewImage = false;
            _camera.NewImageReady += CameraNewImageReady;
        }

        private void CameraNewImageReady(object sender, NewImageReadyEventArgs e)
        {
            _isNewImage = true;
            _lastImage = e.Image;
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
                while (!_isNewImage)
                {
                    Thread.Sleep(10);
                }

                return File(_lastImage, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Set the timezone to use on the camera http(s)://url/image/settimezone?timezon=1
        /// </summary>
        /// <param name="timezone">The timezone</param>
        [HttpGet("settimezone")]
        public void SetTimezone(int timezone)
        {
            _camera.Timezone = timezone;
        }

        /// <summary>
        /// Get an MJPEG stream http(s)://url/image/stream
        /// </summary>
        /// <returns>MJPEG stream</returns>
        [HttpGet("stream")]
        public async Task GetStream()
        {
            var bufferingFeature = HttpContext.Response.HttpContext.Features.Get<IHttpResponseBodyFeature>();
            bufferingFeature?.DisableBuffering();

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.ContentType = "multipart/x-mixed-replace; boundary=--frame";
            HttpContext.Response.Headers.Add("Connection", "Keep-Alive");
            HttpContext.Response.Headers.Add("CacheControl", "no-cache");
            try
            {
                _logger.LogWarning($"Entering streaming loop");

                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    while (!_isNewImage)
                    {
                        Thread.Sleep(10);
                    }

                    await HttpContext.Response.BodyWriter.WriteAsync(CreateHeader(_lastImage!.Length));
                    await HttpContext.Response.BodyWriter.WriteAsync(_lastImage);
                    await HttpContext.Response.BodyWriter.WriteAsync(CreateFooter());
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

            _logger.LogWarning($"Lets send the content now");
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
