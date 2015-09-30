using System;
using System.IO;
using System.Web.Script.Serialization;

public partial class UploadPage : JSONPage {
    protected void Page_Load (object sender, EventArgs e) {
        string targetPath;
        if (!SetupRequest(out targetPath))
            return;

        var tempPath = Path.GetTempFileName();
        using (var outStream = File.Open(tempPath, FileMode.Create, FileAccess.Write))
            Request.InputStream.CopyTo(outStream);

        File.Copy(tempPath, targetPath, true);
        File.Delete(tempPath);

        Response.StatusCode = 200;
        WriteResponseJSON(new {
            ok = true,
            size = (new FileInfo(targetPath)).Length,
            targetPath = targetPath
        });
    }
}