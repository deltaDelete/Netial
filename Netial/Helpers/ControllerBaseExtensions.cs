using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Netial.Helpers;

public static class ControllerBaseExtensions {
    private static readonly JsonSerializerOptions? _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public static ContentResult FixedOk<T>(this ControllerBase controller, T obj) {
        return controller.Content(JsonSerializer.Serialize(obj, _jsonSerializerOptions), "application/json");
    }
}