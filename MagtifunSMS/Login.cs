using System.Text;
using System.Net;
using System.IO;

namespace MagtifunSMS
{
    public class Login
    {
        readonly CookieContainer _cookieJar;
        string _pageContent;

        public Login()
        {
            _cookieJar = new CookieContainer();
        }

        public string PostRequest(string postData, string requestUrl)
        {
            byte[] data = Encoding.ASCII.GetBytes(postData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "POST";
            request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.CookieContainer = _cookieJar;
            request.ContentType = @"application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);

                    _pageContent = reader.ReadToEnd();
                }
            }
            response.Close();

            return _pageContent;
        }
    }
}





/*CookieContainer cookieJar = new CookieContainer();
HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.google.com");
request.CookieContainer = cookieJar;

HttpWebResponse response = (HttpWebResponse)request.GetResponse();*/