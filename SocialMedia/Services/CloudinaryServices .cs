using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace SocialMedia.Services
{
    public class CloudinaryServices
    {

        private const string Tags = "social-media";
        private const string Folder = "social-media";
        private readonly Cloudinary _cloudinary;

        public CloudinaryServices(Cloudinary cloudinary)
        {
            _cloudinary=cloudinary;
        }

        public List<Dictionary<string, string>> PutFilesToCloundinary(IFormFile[] files)
        {
            var results = new List<Dictionary<string, string>>();

            foreach (var file in files)
            {
                    RawUploadParams uploadParams = new RawUploadParams();

                if (file.ContentType.StartsWith("image/"))
                {
                    uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream()),
                        Folder = Folder,
                        Tags = Tags
                    };
                }
                else if (file.ContentType.StartsWith("video/"))
                {
                    uploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream()),
                        Folder = Folder,
                        Tags = Tags,
                        EagerAsync = true,
                    };
                }
                else 
                {
                    uploadParams = new AutoUploadParams
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream()),
                        Folder = Folder,
                        Tags = Tags,
                    };
                }

                var result =  _cloudinary.Upload(uploadParams);

                var imageProperties = new Dictionary<string, string>();
                foreach (var token in result.JsonObj.Children())
                {
                    if (token is JProperty prop)
                    {
                        imageProperties.Add(prop.Name, prop.Value.ToString());
                    }
                }

                results.Add(imageProperties);
            }
            return results;
        }
    }
}
