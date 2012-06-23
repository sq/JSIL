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

            /*
            requestBody =
@"using System;

public static class Program {
  public static void Main () {
    Console.WriteLine(""Hello World"");
  }
}";
            */
        } else {
            using (var sr = new StreamReader(Request.InputStream))
                requestBody = sr.ReadToEnd();
        }

        bool deleteTempFiles = true;

        try {
            string entryPointName, warnings;
            var result = JSIL.Try.SnippetCompiler.Compile(requestBody, deleteTempFiles);

            WriteResponseJSON(new {
                ok = true,
                javascript = result.JavaScript,
                entryPoint = result.EntryPoint,
                warnings = result.Warnings,
                compileElapsed = result.CompileElapsed,
                translateElapsed = result.TranslateElapsed
            });
        } catch (Exception exc) {
            Fail(exc);
        }
    }
}