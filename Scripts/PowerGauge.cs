using Godot;
using System;

public partial class PowerGauge : ProgressBar {

	bool pressing = false;
	bool isMouseOver = false;
	public override void _Ready() {
		MouseEntered += () => { isMouseOver = true; };
		MouseExited += () => { isMouseOver = false; };
	}

    public override void _Process(double delta) {
		if ((Input.IsActionJustPressed("left_click") && isMouseOver) || (Input.IsMouseButtonPressed(MouseButton.Left) && pressing)) {
			pressing = true;
			var mousePos = GetViewport().GetMousePosition();
			var ComputedValue = Mathf.Clamp(1 - (mousePos.Y - GlobalPosition.Y) / Size.Y, 0, 1);
			Value = ComputedValue;
		} else {
			pressing = false;
		}
	}
}
