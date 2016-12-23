using System;
using System.Linq;
using System.Threading.Tasks;
using SkyBiometry.Client.FC;

namespace SkyRecognitionRemoval
{
    public class Program
    {
        static void Main(string[] args)
        {
            var sky = new SkyRemoval();
            
            Task.Run(async () =>
            {
                //await sky.RemoveUser("userId", "userNamespace");
            }).Wait();

        }
    }

    public class SkyRemoval
    {
        private const string ApiKey = "API_KEY";
        private const string ApiSecret = "API_SECREAT";
        private readonly FCClient client;

        public SkyRemoval()
        {
                 client = new FCClient(ApiKey, ApiSecret);
        }

        public async Task RemoveUser(string userId, string userNamespace)
        {
            Console.WriteLine("Deletion started");

            Console.WriteLine("Getting tags");
            var tags = await client.Tags.GetAsync(new string[] { $"{userId}@{userNamespace}" }, null, null, null);

            if (!IsResultSuccess(tags))
            {
                Console.WriteLine($"Getting tags failed - {tags.ErrorMessage}");
                return;
            }
            
            if (!AreTagsValid(tags))
            {
                Console.WriteLine($"Tags are invalid - {tags.ErrorMessage}");
                return;
            }

            Console.WriteLine($"Removing {tags.Photos.Count} user photos");

            var result = await client.Tags.RemoveAsync(tags.Photos.SelectMany(p => p.Tags).Select(t => t.TagId));
            if (!IsResultSuccess(result))
            {
                Console.WriteLine($"Removing tags failed - {tags.ErrorMessage}");
                return;
            }

            Console.WriteLine("Deleted user photos");
            Console.WriteLine("Training empty model");

            result = await client.Faces.TrainAsync(new[] {userId}, userNamespace);
            if (!IsResultSuccess(result))
            {
                Console.WriteLine($"Training empty model failed - {tags.ErrorMessage}");
                return;
            }

            Console.WriteLine("User has been deleted!!!");


            return;
        }

        private static bool IsResultSuccess(FCResult result)
        {
            return result.Status == Status.Success;
        }

        private static bool AreTagsValid(FCResult tags)
        {
            return tags != null
                && tags.Photos != null
                && tags.Photos.Any()
                && tags.Photos.First().Tags != null
                && tags.Photos.First().Tags.Any();
        }
    }
}
