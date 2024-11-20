using Metalama.Framework.Aspects;
using System.Text;

public static class EnrichExceptionHelper
{
    private const string SlotName = "Context";

    [ExcludeAspect(typeof(EnrichExceptionAttribute))]
    public static void AppendContextFrame(this Exception e, string frame)
    {
        // Get or create a StringBuilder for the exception where we will add additional context data.
        var stringBuilder = (StringBuilder?)e.Data[SlotName];

        if (stringBuilder == null)
        {
            stringBuilder = new StringBuilder();
            e.Data[SlotName] = stringBuilder;
        }

        // Add current context information to the string builder.
        stringBuilder.Append(frame);
        stringBuilder.AppendLine();
    }

    public static string? GetContextInfo(this Exception e)
        => ((StringBuilder?)e.Data[SlotName])?.ToString();
}