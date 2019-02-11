﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Newtonsoft.Json;

namespace dotnetcore
{
    class Program
    {
        static void Main(string[] args)
        {
             var CaCert = X509Certificate.CreateFromCertFile(@"root-CA.crt");

            // new changes
            var CaCertNew = new X509Certificate2(CaCert);
           
            var ClientCert = new X509Certificate2(@"WinIOTDevice.pfx", "password1");
            Thermostat thermostat = new Thermostat();


            while (true)
            {

              
                InvokeHttpPost(CaCertNew, ClientCert, thermostat).Wait();

                Thread.Sleep(5000);
            }
        }

         private static async Task<string> InvokeHttpPost(X509Certificate2 root, X509Certificate2 device, Thermostat th)
        {
                     

            Random r = new Random();


            string requesturi = @"https://a1775y1qp2whis.iot.us-east-1.amazonaws.com:8443/topics/iotbutton/virtualButton?qos=1";

            th.ThermostatID = r.Next(10000);
            th.SetPoint = r.Next(32, 100);

            th.CurrentTemperature = r.Next(32, 100);

            string postData = JsonConvert.SerializeObject(th);

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requesturi);


            request.Method = "POST";
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.KeepAlive = true;
            request.ClientCertificates.Add(root);
            request.ClientCertificates.Add(device);


            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();


            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string responseString;
            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            responseString = responseReader.ReadToEnd();

            if (responseString.Contains("OK"))
            {
                Console.WriteLine(responseString);
            }

            else
            {
                Console.WriteLine("Not successful" + responseString);

            }

            return responseString;
        }   
      
    }
}
