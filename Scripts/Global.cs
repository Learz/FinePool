using Godot;
using System;
using System.Collections.Generic;

public class Global : Node
{
    public Vector3 GRAVITY { get; } = new Vector3(0,-9.8f,0);

    public Dictionary<String, object> debug = new Dictionary<string, object>(); 
}
