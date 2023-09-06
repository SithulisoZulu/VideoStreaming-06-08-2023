using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace VideoStreaming.Controllers
{
	public class VideoStreamingController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		private readonly IConfiguration _config;

		public VideoStreamingController(IConfiguration config)
		{
			_config = config;
		}

		[HttpPost]
		public IActionResult GetRestSharpToken(long size, string fileName)
		{
			if ((size < 1) || (string.IsNullOrEmpty(fileName)) || (size > 2147483648))
				return BadRequest();

			var truncatedName = fileName.Substring(0, fileName.Length < 119 ? fileName.Length : 119);
			var request =
				new RestRequest(
						new Uri(
							$"https://api.cloudflare.com/client/v4/accounts/{_config["CloudFlareAccountId:ServiceApiKey"]}/stream?direct_user=true"),
						Method.Post)
					.AddHeader("Authorization", $"Bearer {_config["BearerToken:ServiceApiKey"]}")
					.AddHeader("Tus-Resumable", "1.0.0")
					.AddHeader("Upload-Length", size.ToString())
					.AddHeader("Upload-Metadata",
						$"name " + EncodeTo64(truncatedName) + ",requiresignedurls " + EncodeTo64("true") +
						",allowedorigins " + EncodeTo64(_config["AppSettings:AllowOrigins"] ?? string.Empty));

			var response = new RestClient().Execute(request);

			var location = response.Headers?.FirstOrDefault(x => x.Name == "Location");
			var streammeidaid = response.Headers?.FirstOrDefault(x => x.Name == "stream-media-id");

			Response.Headers.Add("Access-Control-Expose-Headers", "Location");
			Response.Headers.Add("Access-Control-Allow-Headers", "*");
			Response.Headers.Add("Access-Control-Allow-Origin", "*");
			Response.Headers.Add("Location", location?.Value?.ToString());

			return Ok();
		}

		private static string EncodeTo64(string toEncode)
		{
			return Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(toEncode));
		}

		//[HttpPost, ValidateAntiForgeryToken]
		//public string SignVideoURL(string vUID, int? expiryTimeSeconds)
		//{
		//	//Need to check if the user has permission to access the video
		//	//The user must not be able to set the expiry time on the video
		//	//CallBack(vUID);
		//	var url = new StringBuilder("https://signvideos.ilearn.software/?vUID=" + vUID + "&keyId=" +
		//								_config["IDKey:ServiceApiKey"]);

		//	if (expiryTimeSeconds != null)
		//	{
		//		url.Append("&exp=" + expiryTimeSeconds);
		//	}

		//	var request = new RestRequest(new Uri(url.ToString()))
		//		.AddHeader("Authorization", $"Basic {EncodeTo64(_config["SignCredentials:ServiceApiKey"] ?? String.Empty)}");

		//	var response = new RestClient().Execute(request);

		//	ResultToken? returnJResult = null;

		//	if (response.Content != null)
		//	{
		//		returnJResult = JsonConvert.DeserializeObject<ResultToken>(response.Content);
		//	}

		//	return returnJResult?.signedToken ?? string.Empty;
		//}

		[HttpPost]
		public bool DeleteVideo(string vUID)
		{
			//Check if vUID is valid
			var request =
				new RestRequest(
						new Uri(
							$"https://api.cloudflare.com/client/v4/accounts/{_config["CloudFlareAccountId:ServiceApiKey"]}/stream/" +
							vUID),
						Method.Delete)
					.AddHeader("Authorization", $"Bearer {_config["BearerToken:ServiceApiKey"]}")
					.AddHeader("Content-Type", "application/json");

			return new RestClient().Execute(request).StatusCode == HttpStatusCode.OK;
		}

		//[HttpPost]
		//public IActionResult CallBack(string vUID)
		//{
		//	//curl - X GET "https://api.cloudflare.com/client/v4/accounts/<ACCOUNT_ID>/stream?after=2014-01-02T02:20:00Z&before=2014-01-02T02:20:00Z&include_counts=false&creator=<CREATOR_ID>&limit=undefined&asc=false&status=downloading,queued,inprogress,ready,error" \
		//	//   -H "Authorization: Bearer <API_TOKEN>" \
		//	//   -H "Content-Type: application/json"
		//	var request =
		//			new RestRequest(
		//					new Uri(
		//						$"https://api.cloudflare.com/client/v4/accounts/{_config["CloudFlareAccountId:ServiceApiKey"]}/stream/" +
		//						vUID),
		//					Method.Post)
		//				.AddHeader("Authorization", $"Bearer {_config["BearerToken:ServiceApiKey"]}")
		//				.AddHeader("Content-Type", "application/json");

		//	var signature = Request.Headers["Webhook-Signature"].ToString();
		//	using (var sr = new StreamReader(Request.Body))
		//	{
		//		var content = sr.ReadToEndAsync().GetAwaiter().GetResult();
		//		//check spec to understand the fields
		//		//https://developers.cloudflare.com/stream/manage-video-library/using-webhooks/
		//		var result = JsonConvert.DeserializeObject<VideoCompletedResult>(content);
		//		var sigSplitResult = ReturnSignature(signature.Split(","));
		//		var check = sigSplitResult.UnixTime + "." + content;
		//		var byteResult = CreateToken(check, vUID);

		//		if (sigSplitResult.Signature != byteResult)
		//		{
		//			return NotFound();
		//		}
		//	}
		//	return Ok();
		//}




		//private static CallBackSignature ReturnSignature(string[] signature)
		//{
		//	return new CallBackSignature
		//	{
		//		UnixTime = signature[0].Split('=')[1],
		//		Signature = signature[1].Split('=')[1]
		//	};
		//}

		private static string ToHex(byte[] bytes)
		{
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}

		private static string CreateToken(string message, string secret)
		{
			var encoding = new ASCIIEncoding();
			using var hmacsha256 = new HMACSHA256(encoding.GetBytes(secret));
			return ToHex(hmacsha256.ComputeHash(encoding.GetBytes(message)));
		}

	}
}
