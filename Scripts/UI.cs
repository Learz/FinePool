using ExtensionMethods;
using Godot;
using System;

public class UI : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    // Called once for every event, before _unhandled_input(), allowing you to
    // consume some events.
    public override void _Input(InputEvent input)
    {
        if(input.IsActionPressed("h")){
            win();
        }
    }

    void win(){

    }
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        GetNode<Label>("DebugLabel").Text = "";
        foreach(var entry in this.GetGlobal().debug)
        {
                GetNode<Label>("DebugLabel").Text += (entry.Key ?? "Null").ToString() + " : " + (entry.Value ?? "Null").ToString() + "\n";
        }
    }
}
