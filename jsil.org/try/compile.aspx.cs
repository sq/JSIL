using System;
using System.IO;
using System.Web.Script.Serialization;

public partial class CompilePage : System.Web.UI.Page {
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

    protected void Page_Load (object sender, EventArgs e) {
        string requestBody;

        if (Request.RequestType != "POST") {
            Fail("Request must be a POST");
            return;
        } else {
            using (StreamReader sr = new StreamReader(Request.InputStream))
                requestBody = sr.ReadToEnd();
        }

        // Necessary to keep server memory usage low.
        GC.Collect();

        try {
            var javascript = JSIL.Try.SnippetCompiler.Compile(requestBody);

            WriteResponseJSON(new {
                ok = true,
                javascript = javascript
            });
        } catch (Exception exc) {
            Fail(exc);
        } finally {
            // Necessary to keep server memory usage low.
            GC.Collect();
        }
    }
}