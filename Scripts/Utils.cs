using Godot;
using System;
using System.Collections.Generic;
public static class Utils {
    public static string NetworkClientString(this Node node) {
        return node.Multiplayer.IsServer() ? "[Server]" : "[Client]";
    }
}