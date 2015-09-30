using System;
using System.IO;
using System.Web.Script.Serialization;

public partial class DownloadPage : JSONPage {
    protected void Page_Load (object sender, EventArgs e) {
        if (Request.RequestType != "GET") {
            Response.Clear();
            Response.StatusCode = 400;
            return;
        }
        
        string targetPath;
        if (!SetupRequest(out targetPath)) {
            Response.Clear();
            Response.StatusCode = 400;
            return;
        }

        if (!File.Exists(targetPath)) {
            Response.Clear();
            Response.AddHeader("TargetPath", targetPath);
            Response.StatusCode = 404;
            return;
        }

        Response.Clear();
        Response.StatusCode = 200;
        Response.ContentType = "application/octet-stream";
        Response.AddHeader("Content-Disposition", "attachment; filename=\"" + Path.GetFileName(targetPath) + "\"");
        Response.AddHeader("TargetPath", targetPath);
        Response.Buffer = false;
        Response.Flush();

        using (var fileStream = File.OpenRead(targetPath))
            fileStream.CopyTo(Response.OutputStream);

        Response.Flush();
    }
}