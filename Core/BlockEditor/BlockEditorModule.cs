using NC.WebEngine.Core.Data;
using System.IO.Pipelines;

namespace NC.WebEngine.Core.EditableImage
{
    public class BlockEditorModule : IModule
    {
        public void Register(WebApplication app)
        {
            app.MapPost("/__blockeditor/imageupload", this.HandleUpload);
        }

        private async Task<IResult> HandleUpload( DatabaseService db, HttpContext ctx)
        {
            if (ctx.Request.Form.Files.Count != 1  ||
                ctx.Request.Form.ContainsKey("pageId") == false)
            {
                return Results.BadRequest();
            }

            // Image is resized at client side, we only save the file
            var uploadedFile = ctx.Request.Form.Files[0];
            var targetFile = Path.Combine( Directory.GetCurrentDirectory(),
                                "wwwroot",
                                "attachments",
                                ctx.Request.Form["pageId"]!.ToString().Replace("/", "\\"),
                                uploadedFile.FileName);

            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

            using var targetStream = File.OpenWrite(targetFile);
            await uploadedFile.OpenReadStream().CopyToAsync(targetStream);

            return Results.Ok( new
            {
                success = 1,
                file = new
                {
                    url = $"/attachments/{ctx.Request.Form["pageId"]}/" + uploadedFile.FileName
                }
            });
        }
    }
}
