using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace Weather
{
    [Produces("application/json")]
    [Route("api/GetForecast")]
    public class Forecast : Controller
    {
        private string response;
        private int timesCount = 0;
        private struct WeatherConditions
        {
            public static string date1;
            public static string date2;
            public static string temperature;
            public static string windSpeed;
            public static string windDirection;
            public static string cloudiness;
            public static float minTemperature = 1000;
            public static float maxTemperature = -1000;
            public static string previousDate = string.Empty;
        }


        /// <summary>
        /// Get 5 day forecast in specific city.
        /// </summary>
        /// <response code="200">OK. Standard response for successful HTTP requests.</response>
        /// <response code="401">When authentication is required and has failed. It may if invalid API key "appid".</response>
        /// <response code="404">If requested resource could not be found.</response>
        /// <response code="500">Internal Server Error. A generic error message, given when an unexpected condition was encountered.</response>
        [HttpGet]
        public async void GetForecast()
        {
            string apiRequest = "https://api.openweathermap.org/data/2.5/forecast?q=Dnipro,ua&mode=xml&units=metric&appid=4631285a0d0d779d5e026d77f82437dd";
            string forecast = string.Empty;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiRequest);
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                StreamReader strm = new StreamReader(webResponse.GetResponseStream());
                forecast = strm.ReadToEnd();
                response = "5 day forecast in Dnipro:\n\n";
                ParseResponse(forecast);
                await HttpContext.Response.WriteAsync(response);
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)exception.Response).StatusCode;
                    HttpContext.Response.StatusCode = (int)statusCode;
                }
                else
                {
                    WebExceptionStatus status = exception.Status;
                    await HttpContext.Response.WriteAsync($"Error! Status: {status}");
                }
            } 
        }


        private void ParseResponse(string xmlString)
        {
            XmlDocument xmld = new XmlDocument();
            xmld.LoadXml(xmlString);
            XmlNodeReader nodeReader = new XmlNodeReader(xmld);
            XDocument doc = XDocument.Load(nodeReader);

            XElement head = doc.Element("weatherdata");
            foreach (XElement current in head.Element("forecast").Elements("time"))
            {
                XElement temperature = current.Element("temperature");
                XElement windSpeed = current.Element("windSpeed");
                XElement windDirection = current.Element("windDirection");
                XElement clouds = current.Element("clouds");
                XAttribute m_date1 = current.Attribute("from");
                XAttribute m_date2 = current.Attribute("to");
                XAttribute m_temperature = temperature.Attribute("value");
                XAttribute m_windSpeed = windSpeed.Attribute("mps");
                XAttribute m_windDirection = windDirection.Attribute("name");
                XAttribute m_clouds = clouds.Attribute("value");

                WeatherConditions.date1 = m_date1.Value;
                WeatherConditions.date2 = m_date2.Value;
                WeatherConditions.temperature = m_temperature.Value;
                WeatherConditions.temperature += ".";
                WeatherConditions.temperature = WeatherConditions.temperature.Substring(0, 
                    WeatherConditions.temperature.IndexOf('.'));
                WeatherConditions.windSpeed = m_windSpeed.Value;
                WeatherConditions.windDirection = m_windDirection.Value;
                WeatherConditions.cloudiness = m_clouds.Value;

                ResponseFormation();
                SetMinMaxTemperature();
            }
        }


        private void ResponseFormation()
        {
            response += "Date:\t\t";
            string tmp = WeatherConditions.date1;
            response += tmp.Remove(10, tmp.Length - 10);
            response += " (";
            response += tmp.Substring(11, 5);
            response += " - ";
            tmp = WeatherConditions.date2;
            response += tmp.Substring(11, 5);
            response += ")";
            response += "\n";

            response += "Wind speed:\t";
            response += WeatherConditions.windSpeed;
            response += " m/s, ";
            response += WeatherConditions.windDirection;
            response += "\n";

            response += "Temperature:\t";
            response += WeatherConditions.temperature;
            response += " C\n";

            response += "Cloudiness:\t";
            response += WeatherConditions.cloudiness;
            response += "\n\n";
        }


        private void SetMinMaxTemperature()
        {
            timesCount++;
            string tmp = WeatherConditions.temperature;
            float currentTemperature = Convert.ToSingle(tmp);
            string currentDate = WeatherConditions.date2.Substring(11, 5);

            if (currentTemperature <= WeatherConditions.minTemperature)
            {
                WeatherConditions.minTemperature = currentTemperature;
            }
            if (currentTemperature >= WeatherConditions.maxTemperature)
            {
                WeatherConditions.maxTemperature = currentTemperature;
            }
            if (currentDate == "00:00" || timesCount == 40)
            {
                response += $"Minimum temperature: {WeatherConditions.minTemperature}";
                response += $"\nMaximum temperature: {WeatherConditions.maxTemperature}";
                response += "\n--------------------------------------------\n";
                WeatherConditions.minTemperature = 1000;
                WeatherConditions.maxTemperature = -1000;
            }
        }
    }
}