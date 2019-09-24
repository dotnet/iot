// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Iot.Device.LEDMatrix;
using Iot.Device.Graphics;

namespace LedMatrixWeather
{
    internal struct OpenWeatherCoord
    {
        public float Lon { get; set; }
        public float Lat { get; set; }
    }

    internal struct OpenWeather
    {
        public int Id { get; set; }
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    internal struct OpenWeatherWind
    {
        public float Speed { get; set; }
        public float Deg { get; set; }
    }
    
    internal struct OpenWeatherClouds
    {
        public int All { get; set; }
    }
    
    internal struct OpenWeatherSys
    {
        public int Type { get; set; }
        public int Id { get; set; }
        public float Message { get; set; }
        public string Country { get; set; }
        public int Sunrise { get; set; }
        public int Sunset { get; set; }
    }
    
    internal struct OpenWeatherReadings
    {
        public float Temp { get; set; }
        public float Pressure { get; set; }
        public float Humidity { get; set; }
        public float TempMin { get; set; }
        public float TempMax { get; set; }
    }
    
    internal struct OpenWeatherResponse
    {
        // This is also self-test since this is an actual response from day of first testing.
        private const string ExampleJsonResponse = "{\"coord\":{\"lon\":-122.12,\"lat\":47.67},\"weather\":[{\"id\":500,\"main\":\"Rain\",\"description\":\"light rain\",\"icon\":\"10d\"},{\"id\":701,\"main\":\"Mist\",\"description\":\"mist\",\"icon\":\"50d\"}],\"base\":\"stations\",\"main\":{\"temp\":59.52,\"pressure\":1011,\"humidity\":87,\"temp_min\":57.99,\"temp_max\":61},\"visibility\":16093,\"wind\":{\"speed\":8.05,\"deg\":140},\"rain\":{\"1h\":0.63},\"clouds\":{\"all\":90},\"dt\":1569171772,\"sys\":{\"type\":1,\"id\":3417,\"message\":0.0124,\"country\":\"US\",\"sunrise\":1569160502,\"sunset\":1569204450},\"timezone\":-25200,\"id\":5808079,\"name\":\"Redmond\",\"cod\":200}";
        public OpenWeatherCoord Coord { get; set; }
        public OpenWeather[] Weather { get; set; }
        public string Base { get; set; }
        public OpenWeatherReadings Main { get; set; }
        public int Visibility { get; set; }
        public OpenWeatherWind Wind { get; set; }
        public OpenWeatherClouds Clouds { get; set; }
        public int Dt { get; set; }
        public OpenWeatherSys Sys { get; set; }
        public int Timezone { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cod { get; set; }

        public static OpenWeatherResponse FromJson(string json)
        {
            return JsonSerializer.Deserialize<OpenWeatherResponse>(json,
                new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
        }

        public static OpenWeatherResponse GetExampleResponse()
        {
            return FromJson(ExampleJsonResponse);
        }
    }
}
