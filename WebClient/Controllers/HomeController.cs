using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers {

    public class HomeController : Controller {

        [Authorize]
        public async Task<IActionResult> Index() {

            string accessToken = await HttpContext.Authentication.GetTokenAsync("access_token");

            using (HttpClient client = new HttpClient()) {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync("https://api.cix.uk/v3.0/Forum/cixnews/details");

                if (response.StatusCode == HttpStatusCode.Unauthorized) {
                    return NoContent();
                }

                string content = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(content)) {
                    ViewBag.Details = content;
                }
            }

            return View();
        }
    }
}
