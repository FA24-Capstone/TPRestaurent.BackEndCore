﻿namespace TPRestaurent.BackEndCore.Common.ConfigurationModel;

public class RedisConfiguration
{
    public bool Enabled { get; set; }
    public string? ConnectionString { get; set; }
}