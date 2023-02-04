shader_type canvas_item;
uniform sampler2D background;
uniform vec4 bgColor : hint_color;

void vertex() {
// Output:0

}

void fragment() {
	vec4 tex_frg_2_read = texture(background, ((SCREEN_UV.xy / SCREEN_PIXEL_SIZE.xy) * 0.0025f) + (TIME / 10.0f));
	vec3 bg_color = tex_frg_2_read.rgb;
	float bg_opacity = tex_frg_2_read.a;

	vec3 n_out5p0 = mix(bgColor.rgb, bg_color, bg_opacity*2.0f);

	COLOR.rgb = n_out5p0;
	COLOR.a = 1.0f;

}

void light() {
// Output:0

}
