using System;
using System.Text.RegularExpressions;
using SmartX.Shared.Contracts.Sensors;
using SmartX.Shared.Validation;

namespace SmartX.Core.Validation;

/// <summary>
/// REG-01: validates sensor registration fields before the API creates
/// or updates a sensor profile.
/// </summary>
public static partial class SensorProfileValidator
{
    /// <summary>
    /// REG-02: checks the device identifier, display name, deployment
    /// location, category and expected reporting interval.
    /// Returns validation failures without changing application state.
    /// </summary>
    public static ValidationResult Validate(
        RegisterSensorRequest request,
        bool locationExists)
    {
        // DATA STRUCTURE DS-08 USE:
        // ValidationResult.Issues is an ordered List<ValidationIssue>.
        // Each entry associates a registration field with its error message.
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Id)
            || !IdentifierPattern().IsMatch(request.Id))
        {
            result.Issues.Add(new(
                "Id",
                "Use 3–64 letters, digits, colons, hyphens or underscores."));
        }

        if (string.IsNullOrWhiteSpace(request.Name)
            || request.Name.Length > 80)
        {
            result.Issues.Add(new(
                "Name",
                "A display name of 1–80 characters is required."));
        }

        if (!locationExists)
        {
            result.Issues.Add(new(
                "LocationId",
                "Choose an enabled deployment location."));
        }

        if (!Enum.IsDefined(request.Category))
        {
            result.Issues.Add(new(
                "Category",
                "Choose a supported category."));
        }

        if (request.IntervalSeconds is < 1 or > 3600)
        {
            result.Issues.Add(new(
                "IntervalSeconds",
                "Reporting interval must be 1–3600 seconds."));
        }

        return result;
    }

    /// <summary>
    /// REG-03: generates the identifier validator at build time.
    /// Absolute string boundaries ensure that trailing line breaks
    /// cannot be accepted as part of an otherwise valid identifier.
    /// </summary>
    /// <remarks>
    /// \A requires the beginning of the input.
    /// \z requires the absolute end of the input.
    /// The first character must be an ASCII letter or digit.
    /// The remaining characters use the existing permitted identifier set.
    /// </remarks>
    [GeneratedRegex(@"\A[A-Za-z0-9][A-Za-z0-9:_-]{2,63}\z")]
    private static partial Regex IdentifierPattern();
}