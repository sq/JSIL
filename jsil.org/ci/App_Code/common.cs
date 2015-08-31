using System;
using System.IO;
using System.Web.Script.Serialization;

public abstract class JSONPage : System.Web.UI.Page {
    protected void WriteResponseJSON (object response) {
        Response.Clear();
        Response.ContentType = "application/json";
        var jss = new JavaScriptSerializer();
        Response.Write(jss.Serialize(response));
    }

    protected void Fail (object error) {
        Response.StatusCode = 400;
        WriteResponseJSON(new {
            ok = false,
            error = error.ToString()
        });
    }

    protected bool SetupRequest (out string targetPath) {
        targetPath = null;

        var key = (Request.Params["key"] ?? "").ToLower().Trim();
        if (key.IndexOf(".") >= 0) {
            Fail("Invalid key");
            return false;
        } else if (key.Length == 0) {
            Fail("No key specified");
            return false;
        }

        string expectedPassword;
        if (
            !AuthTokens.Tokens.TryGetValue(key, out expectedPassword) ||
            (expectedPassword != Request.Params["password"])
        ) {
            Fail("Password mismatch");
            return false;
        }

        var tag = Request.Params["tag"] ?? "";

        var targetDir = Path.Combine(
            Path.GetTempPath(),
            "JSIL CI"
        );

        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        targetPath = Path.Combine(
            targetDir, 
            string.Format("{0}-{1}.bin", key, tag)
        );
        return true;
    }
}