using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NC.WebEngine.Core.VueSync;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NC.WebEngine.Site.CoresharpImport
{
    public class CoreSharpImportVueModel : IVueModel
    {
        /// <summary>
        /// Import the page into block format
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [VueCallableMethod]
        public object ImportCoresharpPage( JsonObject parameters )
        {
            var pageId = parameters["pageId"].ToString();

            var client = new HttpClient();
            var html = client.GetStringAsync(parameters["url"].ToString()).Result;
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var blocks = document.DocumentNode.QuerySelectorAll("#blogContent > *").ToList();

            var resultBlocks = new List<object>();
            foreach ( var block in blocks )
            {
                // sanitize all smileys
                var smileys = block.QuerySelectorAll(".wlEmoticon").ToList();
                foreach (var smiley in smileys)
                {
                    smiley.Attributes["src"].Remove();
                    smiley.Attributes["style"].Remove();
                    smiley.Attributes["class"].Remove();
                }

                if ( block.Name == "p" &&
                     block.FirstChild.Name == "a" &&
                     block.FirstChild.FirstChild.Name == "img" &&
                     (block.FirstChild.GetAttributeValue("href", "").EndsWith("png") ||
                     block.FirstChild.GetAttributeValue("href", "").EndsWith("jpg")) &&
                     block.ChildNodes.Count == 1 )
                {
                    // this is the block with just one image
                    // Download the image 
                    var imageUrl = block.FirstChild.GetAttributeValue("href", "");
                    var fileName = imageUrl.Substring(imageUrl.LastIndexOf("/") + 1);
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "blog", pageId, fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                    var waybackImage = imageUrl.Replace("/http://coresharp.net", "im_/http://coresharp.net");

                    byte[] imageData = null;
                    var response = client.GetAsync(waybackImage).Result;

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // see if we can use the thumbnail
                        if (block.FirstChild.FirstChild.Name == "img")
                        {
                            var img = block.FirstChild.FirstChild.GetAttributeValue("src", "");
                            response = client.GetAsync(img).Result;

                            if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                // Image is not found
                                resultBlocks.Add(new
                                {
                                    id = Guid.NewGuid().ToString(),
                                    type = "paragraph",
                                    data = new
                                    {
                                        text = $"Missing Image: {imageUrl.Substring(imageUrl.IndexOf("/http://"))}"
                                    }
                                });

                                continue;
                            }
                        }
                    }

                    imageData = response.Content.ReadAsByteArrayAsync().Result;

                    File.WriteAllBytes(fullPath, imageData);

                    resultBlocks.Add(new
                    {
                        id = Guid.NewGuid().ToString(),
                        type = "image",
                        data = new
                        {
                            file = new
                            {
                                url = $"/images/blog/{pageId}/{fileName}"
                            },
                            withBorder = false,
                            withBackground = false,
                            stretched = true,
                        }
                    });

                    continue;
                }

                // header block
                if (block.Name.Length == 2 && block.Name.StartsWith("h"))
                {
                    resultBlocks.Add(new
                    {
                        id = Guid.NewGuid().ToString(),
                        type = "header",
                        data = new
                        {
                            text = block.InnerHtml,
                            level = int.Parse( block.Name.Substring(1) )
                        }
                    });

                    continue;
                }

                // block with only one image
                if (block.Name == "p" &&
                    block.FirstChild.Name == "img" &&
                    block.ChildNodes.Count == 1 )
                {
                    // this is the block with just one image
                    // Download the image 
                    var imageUrl = block.FirstChild.GetAttributeValue("src", "");
                    var fileName = imageUrl.Substring(imageUrl.LastIndexOf("/") + 1);
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "blog", pageId, fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                    byte[] imageData = null;
                    var response = client.GetAsync(imageUrl).Result;

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Image is not found
                        resultBlocks.Add(new
                        {
                            id = Guid.NewGuid().ToString(),
                            type = "paragraph",
                            data = new
                            {
                                text = $"Missing Image: {imageUrl.Substring(imageUrl.IndexOf("/http://") + 1)}"
                            }
                        });
                        continue;
                    }


                    imageData = response.Content.ReadAsByteArrayAsync().Result;

                    File.WriteAllBytes(fullPath, imageData);

                    resultBlocks.Add(new
                    {
                        id = Guid.NewGuid().ToString(),
                        type = "image",
                        data = new
                        {
                            file = new
                            {
                                url = $"/images/blog/{pageId}/{fileName}"
                            },
                            withBorder = false,
                            withBackground = false,
                            stretched = true,
                        }
                    });

                    continue;
                }

                if (block.Name == "ol" || block.Name == "ul")
                {
                    var items = block.QuerySelectorAll("li").Select( n => n.InnerHtml).ToList();


                    resultBlocks.Add(new
                    {
                        id = Guid.NewGuid().ToString(),
                        type = "list",
                        data = new
                        {
                            type = block.Name == "ol" ? "ordered" : "unordered",
                            items  = items,
                        }
                    });

                    continue;
                }

                // any other type, make it a paragraph
                resultBlocks.Add(new
                {
                    id = Guid.NewGuid().ToString(),
                    type = "paragraph",
                    data = new
                    {
                        text = block.InnerHtml
                    }
                });
            }

            return new
            {
                IsSuccess = true,
                Blocks = resultBlocks,
            };
        }

        public void OnCreated(HttpContext ctx)
        {
        }

        public void OnPostback(HttpContext ctx)
        {
        }
    }
}
