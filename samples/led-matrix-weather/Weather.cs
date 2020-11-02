// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Iot.Device.LEDMatrix;
using Iot.Device.Graphics;

namespace LedMatrixWeather
{
    class Weather
    {
        private string _openWeatherApiKey;
        private string _city;
        private string _countyCode;

        public Weather(string openWeatherApiKey, string city = "Redmond", string countryCode = "US")
        {
            _openWeatherApiKey = openWeatherApiKey;
            _city = city;
            _countyCode = countryCode;
        }

        public OpenWeatherResponse GetWeatherFromOpenWeather()
        {
            string json = GetJsonResponseAsTextFromOpenWeather();
            Debug.WriteLine($"Received: {json}");
            return OpenWeatherResponse.FromJson(json);
        }

        private string GetJsonResponseAsTextFromOpenWeather()
        {
            return GetJsonResponseAsTextFromOpenWeather(_city, _countyCode, _openWeatherApiKey);
        }

        private static string GetJsonResponseAsTextFromOpenWeather(string city, string countryCode, string weatherKey)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(String.Format("http://api.openweathermap.org/data/2.5/weather?q={0},{1}&mode=json&units=imperial&APPID={2}",
                    city,
                    countryCode,
                    weatherKey));
                HttpResponseMessage response = client.GetAsync("").Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
