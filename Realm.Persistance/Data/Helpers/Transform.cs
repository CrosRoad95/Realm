﻿using Newtonsoft.Json;

namespace Realm.Persistance.Data.Helpers;

public class TransformAndMotion
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public byte Interior { get; set; }
    public ushort Dimension { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 AngularVelocity { get; set; }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public static TransformAndMotion CreateFromString(string json)
    {
        return JsonConvert.DeserializeObject<TransformAndMotion>(json) ?? throw new Exception("Failed to create TransformAndMotion from string json");
    }
}