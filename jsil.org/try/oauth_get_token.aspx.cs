using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public partial class GetTokenPage : System.Web.UI.Page {
    protected void WriteResponseJSON (object response) {
        Response.Clear();
        Response.ContentType = "application/json";
        var jss = new JavaScriptSerializer();
        Response.Write(jss.Serialize(response));
    }

    protected void Fail (object error) {
        WriteResponseJSON(new {
            ok = false,
            error = error.ToString()
        });
    }    

    // The GitHub SSL certificate is corrupt, or something? Who cares.
    public static bool Validator (
        object sender, X509Certificate certificate, 
        X509Chain chain, SslPolicyErrors sslPolicyErrors
    ) {
        return true;
    }
 
    protected void Page_Load (object sender, EventArgs e) {
        if (Request.RequestType != "GET") {
            Fail("Request must be a GET");
            return;
        }

        string temporaryCode = Request.Params["code"];
        if (String.IsNullOrWhiteSpace(temporaryCode)) {
            Fail("Argument required: code");
            return;
        }

        try {
            var serviceUri = "https://github.com/login/oauth/access_token";
            var nvc = new NameValueCollection {
                {"client_id", OAuth.ClientID},
                {"client_secret", OAuth.Secret},
                {"code", temporaryCode}
            };

            ServicePointManager.ServerCertificateValidationCallback = Validator;

            using (var client = new WebClient()) {
                var responseBytes = client.UploadValues(serviceUri, nvc);
                var responseText = Encoding.ASCII.GetString(responseBytes);

                var responseNvc = HttpUtility.ParseQueryString(responseText);
                var responseDict = new Dictionary<string, object>();

                foreach (var k in responseNvc.Keys)
                    responseDict[(string)k] = responseNvc[(string)k];

                WriteResponseJSON(new {
                    ok = responseDict.ContainsKey("access_token"),
                    response = responseDict
                });
            }
        } catch (Exception exc) {
            Fail(exc);
        }
    }
}