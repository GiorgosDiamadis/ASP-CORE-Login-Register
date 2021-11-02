using System;
using RestSharp;
using RestSharp.Authenticators;
using WebApplication.Services.Interfaces;

namespace WebApplication.Services
{
    public class Mailer:IMailer
    {
        public void ForgotPassWordEmail(string email,string token)
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.eu.mailgun.net/v3");
            client.Authenticator =
                new HttpBasicAuthenticator("api",
                    "key-3fd13d45e6a1d3243db9bbd88bf69780");
            RestRequest request = new RestRequest();
            request.AddParameter("domain", "mg.diamadisgiorgos.com", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Diamadis Giorgos | BugTracker <mailgun@mg.diamadisgiorgos.com>");

            request.AddParameter("to", email);

            request.AddParameter("subject", "Hello Diamadis Giorgos");
            request.AddParameter("template", "passwordrecovery");
            token = token.Replace("+", "%2B");
            request.AddParameter("v:token", token);
            request.Method = Method.POST;
            client.Execute(request);
        }

        public void ConfirmEmail(string email,string token)
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.eu.mailgun.net/v3");
            client.Authenticator =
                new HttpBasicAuthenticator("api",
                    "key-3fd13d45e6a1d3243db9bbd88bf69780");
            RestRequest request = new RestRequest();
            request.AddParameter("domain", "mg.diamadisgiorgos.com", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Diamadis Giorgos | BugTracker <mailgun@mg.diamadisgiorgos.com>");

            request.AddParameter("to", email);

            request.AddParameter("subject", "Hello Diamadis Giorgos");
            request.AddParameter("template", "confirmationmail");
            token = token.Replace("+", "%2B");
            var link = "http://localhost:5000/confirmEmail?token=" + token;
            request.AddParameter("v:link", link);
            request.Method = Method.POST;
            client.Execute(request);
        }
        


    }
}