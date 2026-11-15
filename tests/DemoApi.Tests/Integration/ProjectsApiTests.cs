using DemoApi.Domain;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DemoApi.Tests.Integration;

[Collection(ApiCollection.Name)]
public class ProjectsApiTests(WebApplicationFactory<Program> factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ReturnsCreatedWithLocation()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new { name = TestApi.Unique("project") });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsConflict()
    {
        var name = TestApi.Unique("duplicate");
        (await _client.PostAsJsonAsync("/api/projects", new { name })).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/projects", new { name });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new { name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_Unknown_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/projects/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Stats_CountsTaskStatusesAndOverdueItems()
    {
        var project = await _client.CreateProjectAsync();
        await _client.CreateTaskAsync(project.Id);
        var active = await _client.CreateTaskAsync(project.Id);
        (await _client.ChangeStatusAsync(active.Id, TaskItemStatus.InProgress)).EnsureSuccessStatusCode();
        (await _client.PostAsJsonAsync("/api/tasks", new
        {
            projectId = project.Id,
            title = "Просрочена",
            dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2))
        })).EnsureSuccessStatusCode();

        var stats = await _client.GetFromJsonAsync<ProjectStats>($"/api/projects/{project.Id}/stats", TestApi.Json);
        Assert.Equal(new ProjectStats(project.Id, 3, 2, 1, 0, 1), stats);
    }

    [Fact]
    public async Task Stats_UnknownProjectReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/projects/{int.MaxValue}/stats");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task Rename_PreservesIdentityCreatedAtAndTasks()
    {
        var project = await _client.CreateProjectAsync();
        var task = await _client.CreateTaskAsync(project.Id);
        var name = TestApi.Unique("renamed");

        var response = await _client.PutAsJsonAsync($"/api/projects/{project.Id}", new { name });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var renamed = await response.Content.ReadFromJsonAsync<Project>(TestApi.Json);
        Assert.Equal(project with { Name = name }, renamed);
        Assert.Equal(renamed, await _client.GetFromJsonAsync<Project>($"/api/projects/{project.Id}", TestApi.Json));
        var preserved = await _client.GetFromJsonAsync<TaskItem>($"/api/tasks/{task.Id}", TestApi.Json);
        Assert.Equal(task, preserved);
    }

    [Fact]
    public async Task Rename_DuplicateName_ReturnsConflictWithoutChangingProject()
    {
        var project = await _client.CreateProjectAsync();
        var other = await _client.CreateProjectAsync();

        var response = await _client.PutAsJsonAsync($"/api/projects/{project.Id}", new { name = other.Name });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(project, await _client.GetFromJsonAsync<Project>($"/api/projects/{project.Id}", TestApi.Json));
    }

    [Fact]
    public async Task Rename_UnknownProject_ReturnsNotFoundEvenIfNameIsTaken()
    {
        var project = await _client.CreateProjectAsync();

        var response = await _client.PutAsJsonAsync($"/api/projects/{int.MaxValue}", new { name = project.Name });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rename_CurrentName_CanBeRepeated()
    {
        var project = await _client.CreateProjectAsync();
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var response = await _client.PutAsJsonAsync($"/api/projects/{project.Id}", new { name = project.Name });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(project, await response.Content.ReadFromJsonAsync<Project>(TestApi.Json));
        }
    }


    [Fact]
    public async Task Rename_EmptyNameReturnsBadRequest()
    {
        var project = await _client.CreateProjectAsync();
        using var response = await _client.PutAsJsonAsync($"/api/projects/{project.Id}", new { name = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
