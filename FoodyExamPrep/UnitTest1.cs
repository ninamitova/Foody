using System;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FoodyExamPrep.Models;

namespace FoodySystemExamPrep;

[TestFixture]
[NonParallelizable]

public class Tests
{
    private RestClient client;
    private static string? createdFoodId;
    private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";


    [OneTimeSetUp]
    public void Setup()
    {
        string token = GetJwtToken("ninafoody", "ninafoody23");

        var options = new RestClientOptions(baseUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };

        client = new RestClient(options);
    }

    private string GetJwtToken(string username, string password)
    {
        var loginClient = new RestClient(baseUrl);
        var request = new RestRequest("/api/User/Authentication", Method.Post);

        request.AddJsonBody(new { username, password });

        var response = loginClient.Execute(request);

        var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

        return json.GetProperty("accessToken").GetString() ?? string.Empty;

    }

    [Order(1)]
    [Test]
    public void CreateNewFood_WithTheRequiredFields()
    {
        var food = new
        {
            Name = "New Food",
            Description = "Delicious new item",
            Url = ""
        };

        var request = new RestRequest("/api/Food/Create", Method.Post);
        request.AddJsonBody(food);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

        createdFoodId = json.GetProperty("foodId").GetString() ?? string.Empty;

        Assert.That(createdFoodId, Is.Not.Null.And.Not.Empty, "Food ID should not be null or empty");
    }

    [Order(2)]
    [Test]
    public void EditFoodTitle_ShouldReturnOk()
    {
        var changes = new[]
        {
            new {path = "/name", op = "replace", value = "Updated food name"}
        };

        var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch);

        request.AddJsonBody(changes);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
        Assert.That(response.Content, Does.Contain("Successfully edited"));
    }

    [Order(3)]
    [Test]
    public void GetAllFood_ShoulddReturnOk()
    {
        var request = new RestRequest("/api/Food/All", Method.Get);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(((HttpStatusCode)HttpStatusCode.OK)));

        var foods = System.Text.Json.JsonSerializer.Deserialize<List<object>>(response.Content);
        Assert.That(foods, Is.Not.Empty);
    }

    [Order(4)]
    [Test]
    public void DeleteTheEditedFood_ShoulddReturnOk()
    {
        var request = new RestRequest($"/api/Food/Delete/{createdFoodId}", Method.Delete);
        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content, Does.Contain("Deleted successfully!"));
    }

    [Order(5)]
    [Test]
    public void CreateFoodWithoutRequiredFields_ShoulddReturnBadRequest()
    {
        var food = new
        {
            Name = "",
            Description = "",
            Url = ""
        };
        var request = new RestRequest("/api/Food/Create", Method.Post);
        request.AddJsonBody(food);
        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Order(6)]
    [Test]
    public void EditFoodWithNonExistingFood_ShoulddReturnNotFound()
    {
        string fakeID = "123";
        var changes = new[]
        {
            new{ path = "/name", op = "replace", value = "New Title"}
        };

        var request = new RestRequest($"/api/Food/Edit/{fakeID}", Method.Patch);
        request.AddJsonBody(changes);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.Content, Does.Contain("No food revues..."));
    }

    [Order(7)]
    [Test]
    public void DeleteFoodWithNonExistingFood_ShoulddReturnNotFound()
    {
        string fakeID = "123";

        var request = new RestRequest($"/api/Food/Delete/{fakeID}", Method.Delete);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content, Does.Contain("Unable to delete this food revue!"));

    }



    [OneTimeTearDown]
    public void Cleanup()
    {
        client?.Dispose();
    }
}
