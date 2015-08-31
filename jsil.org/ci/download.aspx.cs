using System;
using System.IO;
using System.Web.Script.Serialization;

public partial class DownloadPage : JSONPage {
    protected void Page_Load (object sender, EventArgs e) {
        if (Request.RequestType != "GET") {
            Fail("Request must be a GET");
            return;
        }
        
        string targetPath;
        if (!SetupRequest(out targetPath))
            return;

        if (!File.Exists(targetPath)) {
            Fail("File not found");
            return;
        }

        Response.Clear();
        Response.StatusCode = 200;
        Response.ContentType = "application/octet-stream";
        Response.AddHeader("Content-Disposition", "attachment; filename=\"" + Path.GetFileName(targetPath) + "\"");
        Response.Flush();

        using (var fileStream = File.OpenRead(targetPath))
            fileStream.CopyTo(Response.OutputStream);

        Response.Flush();
    }
}