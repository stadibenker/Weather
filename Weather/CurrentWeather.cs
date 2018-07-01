using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace Weather
{
    [Produces("application/json")]
    [Route("api/GetCurrentWeather")]
    public class CurrentWeather : Controller
    {
        private struct WeatherConditions
        {
            public static string date;
            public static string temperature;
            public static string windSpeed;
            public static string windDirection;
            public static string cloudiness;
        }


        /// <summary>
        /// Get current weather in specific city.
        /// </summary>
        /// <response code="200">OK. Standard response for successful HTTP requests.</response>
        /// <response code="401">When authentication is required and has failed. It may if invalid API key "appid".</response>
        /// <response code="404">If requested resource could not be found.</response>
        /// <response code="500">Internal Server Error. A generic error message, given when an unexpected condition was encountered.</response>
        [HttpGet]
        public async void GetCurrentWeather()
        {
            string apiRequest = "https://api.openweathermap.org/data/2.5/weather?q=Dnipro,ua&mode=xml&units=metric&appid=4631285a0d0d779d5e026d77f82437dd";
            string currentWeather = string.Empty;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiRequest);
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                StreamReader strm = new StreamReader(webResponse.GetResponseStream());
                currentWeather = strm.ReadToEnd();
                ParseResponse(currentWeather);
                await HttpContext.Response.WriteAsync(ResponseFormation());
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

            XElement head = doc.Element("current");
            XElement date = head.Element("lastupdate");
            XElement temperature = head.Element("temperature");
            XElement wind = head.Element("wind");
            XElement windSpeed = wind.Element("speed");
            XElement windDirection = wind.Element("direction");
            XElement clouds = head.Element("clouds");
            XAttribute m_date = date.Attribute("value");
            XAttribute m_temperature = temperature.Attribute("value");
            XAttribute m_windSpeed = windSpeed.Attribute("value");
            XAttribute m_windDirection = windDirection.Attribute("name");
            XAttribute m_clouds = clouds.Attribute("name");

            WeatherConditions.date = m_date.Value;
            WeatherConditions.temperature = m_temperature.Value;
            WeatherConditions.temperature += ".";
            WeatherConditions.temperature = WeatherConditions.temperature.Substring(0,
                WeatherConditions.temperature.IndexOf('.'));
            WeatherConditions.windSpeed = m_windSpeed.Value;
            WeatherConditions.windDirection = m_windDirection.Value;
            WeatherConditions.cloudiness = m_clouds.Value;
        }


        private string ResponseFormation()
        {
            string response;
            response = "Current weather in Dnipro:\n";

            response += "Date:\t\t";
            string tmp = WeatherConditions.date;
            response += tmp.Remove(10, tmp.Length - 10);
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

            return response;
        }
    }
}