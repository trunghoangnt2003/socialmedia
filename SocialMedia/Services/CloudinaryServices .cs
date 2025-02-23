using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Newtonsoft.Json.Linq;
using System.IO;

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

        public async Task<List<Dictionary<string, string>>>  PutFilesToCloundinary(IFormFile[] files)
        {
            var results = new List<Dictionary<string, string>>();

            foreach (var file in files)
            {
                RawUploadParams uploadParams = new RawUploadParams();
                var stream = file.OpenReadStream();

                var fileDescription = new FileDescription(file.FileName, stream);
                if (file.ContentType.StartsWith("image/"))
                {
                    uploadParams = new ImageUploadParams
                    {
                        File = fileDescription,
                        Folder = Folder,
                        Tags = Tags
                    };
                }
                else if (file.ContentType.StartsWith("video/"))
                {
                    uploadParams = new VideoUploadParams
                    {
                        File = fileDescription,
                        Folder = Folder,
                        Tags = Tags,
                        EagerAsync = true,
                    };
                }
                else 
                {
                    uploadParams = new AutoUploadParams
                    {
                        File = fileDescription,
                        Folder = Folder,
                        Tags = Tags,
                    };
                }

                var result =  await _cloudinary.UploadAsync(uploadParams).ConfigureAwait(false); ;

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
