namespace blazor_app.Data;

public class WeatherForecastClient
{
    public WeatherForecastClient(HttpClient client)
    {
        this.client = client;
    }

    private readonly HttpClient client;

    public Task<WeatherForecast[]> GetForecastAsync()
    {
        return client.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");
    }

    public async Task PostForecastAsync()
    {
        await client.PostAsync("/weatherforecast", null);
    }
}
