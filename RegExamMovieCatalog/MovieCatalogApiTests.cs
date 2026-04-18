using RegExamMovieCatalog.Models;
using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using NUnit.Framework;


namespace RegExamMovieCatalog
{

    [TestFixture]
    public class MovieTests
    {
        private RestClient client;
        private static string lastMovieId;
        private static string BaseUrl = "http://144.91.123.158:5000";
       

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("Desi478@tester.com", "1234512345");

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var loginClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { email, password });

            var response = loginClient.Execute(request);
            
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            
            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }

        [Order(1)]
        [Test]
        public void CreateIdea_WithRequiredFields_ShouldReturnsSuccess()
        {
            var movie = new 
            {
                Title = "Inception",
                Description = "The Best Movie Ever!",
               

            };
            
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            
            request.AddJsonBody(movie);
            
            var response = client.Execute(request);
            
            
           var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
             

           Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
           Assert.That(createResponse.Msg, Is.EqualTo("Movie created successfully!"));
            
            
          
        }
        [Order(2)]
        [Test]
        public void GetAllMovies_ShouldReturnsSuccess()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            var response = client.Execute(request);

            var movies = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(movies, Is.Not.Null);
            Assert.That(movies, Is.Not.Empty);
            Assert.That(movies.Count, Is.GreaterThan(0));

            lastMovieId = movies.Last()?.MovieId;
        }








        [Order(3)]
        [Test]
        public void EditExistingMovie_ShouldReturnOK()
        {
            var updatedMovie = new 
            {
                Title = "Inception",
                Description = "Guilt, memory, and loss",
            
            };

            
            var request = new RestRequest($"/api/Movie/Edit", Method.Put);
           
            request.AddQueryParameter("movieId", lastMovieId);

            request.AddJsonBody(updatedMovie);
            
            var response = client.Execute(request);
            
            Console.WriteLine(response.Content);
           
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse?.Msg, Is.EqualTo("Movie edited successfully!"));

        }

      


        [Order(4)]
        [Test]

        public void DeleteMovie_ShouldReturnSuccess() 
        {
            var request = new RestRequest($"/api/Movie/Delete", Method.Delete);

            request.AddQueryParameter("movieId", lastMovieId);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));    
            Assert.That(response.Content, Does.Contain("Movie deleted successfully!"));

         }

        [Order(5)]
        [Test]

        public void CreateMovie_WithoutTitle_ShouldReturnBadRequest()
        {
            var movie = new
            {
                Description = "The Best Movie Ever",
                PosterUrl = "",
                TrailerLink = "",
                IsWatched = true
            };
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            
            request.AddJsonBody(movie);
            
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnNotFound()
        {
           string nonExistingMovieId = "non-existing-id";
            
            var updatedMovie = new
            {
                Title = "Pretty",
                Description = "The Best Movie Ever",
                PosterUrl = "",
                TrailerLink = "",
                IsWatched = true
            };
            
            var request = new RestRequest($"/api/Movie/Edit", Method.Put);
            
            request.AddQueryParameter("movieId", "non-existing-id");
            
            request.AddJsonBody(updatedMovie);
            
            var response = client.Execute(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to edit the movie!"));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnNotFound()
        {
            string nonExistingMovieId = "non-existing-id";
            
            var request = new RestRequest($"/api/Movie/Delete", Method.Delete);
            
            request.AddQueryParameter("movieId", nonExistingMovieId);
            
            var response = client.Execute(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete the movie!"));
        }



          [OneTimeTearDown]
          public void Cleanup()
        {
            client?.Dispose();
        }
    }
}