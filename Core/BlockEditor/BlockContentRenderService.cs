using Microsoft.CodeAnalysis.CSharp.Syntax;
using NC.WebEngine.Core.Content;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Web;

namespace NC.WebEngine.Core.BlockEditor
{
    public class BlockContentRenderService : IService
    {
        public class ImageData
        {
            public File file { get; set; }
            public string caption { get; set; }
            public bool withBorder { get; set; }
            public bool withBackground { get; set; }
            public bool stretched { get; set; }
        }

        public class File
        {
            public string url { get; set; }
        }

        public class BlockData<T>
        {
            public string type { get; set; }
            public T data { get; set; }
        }

        private class RenderContext
        {
            public List<string> AdditionalScripts { get; set; } = new();
            public List<string> InitCodes { get; set; } = new();
        }

        private delegate string Renderer( string blockId, JsonObject blockData, RenderContext context );

        private Dictionary<string, Renderer> _renderers;

        public void RegisterBuilder(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton(this);

             _renderers= new()
            {
                {"image", RenderImage },
                {"paragraph", RenderParagraph },
                {"list", RenderList },
                {"header", RenderHeader },
                {"code", RenderCode },
            };
        }

        public IEnumerable<string> RenderContent( ContentPart part)
        {
            if (part.IsBlockContent == false )
            {
                throw new ArgumentException("Part must be Block Content");
            }

            var editorJsDom = JsonNode.Parse(part.Content)!.AsObject();
            var blocks = editorJsDom["blocks"]!.AsArray();
            var ctx = new RenderContext();

            foreach ( var block in blocks )
            {
                Renderer? renderer;
                if (_renderers.TryGetValue(block["type"].AsValue().ToString(), out renderer))
                {
                    yield return renderer(block["id"].ToString(), block["data"].AsObject(), ctx);
                }
            }

            foreach (var script in ctx.AdditionalScripts.Distinct())
            {
                yield return $"<script src=\"{script}\"></script>";
            }

            yield return $"<script>{string.Join(";\r\n", ctx.InitCodes.Distinct())}</script>";
        }

        private string RenderImage(string blockId, JsonObject blockData, RenderContext ctx)
        {
            var imageData = JsonSerializer.Deserialize<ImageData>(blockData.ToJsonString());
            var bordered = imageData.withBorder ? "bordered" : "";
            var stretched = imageData.stretched ? "stretched" : "";
            var backgrounded = imageData.withBackground ? "backgrounded" : "";

            return $"<div class=\"ncweb-imageblock {stretched} {bordered} {backgrounded}\"><img src=\"{imageData.file.url}\" /></div>";
        }

        private string RenderParagraph(string blockId, JsonObject blockData, RenderContext ctx)
        {
            return $"<p>{blockData["text"]}</p>";
        }

        private string RenderList(string blockId, JsonObject blockData, RenderContext ctx)
        {
            var items = blockData["items"].AsArray().Select( i => $"<li>{i.ToString()}</li>");
            if (blockData["style"].ToString() == "unordered")
            {
                return $"<ul>{string.Join("\r\n", items)}</ul>";
            }
            else
            {
                return $"<ol>{string.Join("\r\n", items)}</ol>";
            }
        }

        private string RenderHeader(string blockId, JsonObject blockData, RenderContext ctx)
        {
            var level = blockData["level"].ToString();

            return $"<h{level}>{blockData["text"]}</h{level}>";
        }

        private string RenderCode(string blockId, JsonObject blockData, RenderContext ctx)
        {
            var language = blockData["language"].ToString();
            var showlinenumbers = blockData["showlinenumbers"].ToString();
            var code = HttpUtility.HtmlEncode(blockData["code"].ToString());

            ctx.AdditionalScripts.Add("/js/prism/prism.js");
            ctx.InitCodes.Add(@$"
if (!document.getElementById('prismcss')) {{
    var link = document.createElement('link');
    link.id = 'prismcss';
    link.rel = 'stylesheet';
    link.href = '/js/prism/prism.css';
    document.head.appendChild(link);
}}
");

            return $"<pre><code class=\"language-{language}\">{code}</code></pre>";
        }
    }
}
