// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MqttHub.Entities.Models;

public class PayloadContentType
{
    // Static readonly fields for each content type.
    public static readonly PayloadContentType JSON = new("application/json", "JSON format");
    public static readonly PayloadContentType TextPlain = new("text/plain", "Plain text format");
    public static readonly PayloadContentType Xml = new("application/xml", "XML format");
    public static readonly PayloadContentType OctetStream = new("application/octet-stream", "Binary data");
    public static readonly PayloadContentType UrlEncoded = new("application/x-www-form-urlencoded", "URL encoded format");

    // Private constructor
    private PayloadContentType(string value, string description)
    {
        Value = value;
        Description = description;
    }

    // Public properties to access value and description
    public string Value { get; }
    public string Description { get; }

    // Optionally, add a method to override ToString() for better readability
    public override string ToString() => $"{Description} ({Value})";
}
