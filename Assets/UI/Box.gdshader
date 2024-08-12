shader_type canvas_item;
uniform sampler2D background;
uniform vec2 global_position;

void vertex() {
	
}

void fragment() {
	vec3 original_texture_color;
	float original_texture_alpha;
	{
		vec4 TEXTURE_tex_read = texture(TEXTURE, UV.xy);
		original_texture_color = TEXTURE_tex_read.rgb;
		original_texture_alpha = TEXTURE_tex_read.a;
	}

// Texture:2
	vec4 tex_frg_2_read = texture(background, (FRAGCOORD.xy + vec2(-global_position.x,global_position.y)) * 0.001f);
	vec3 bg_color = tex_frg_2_read.rgb;
	float bg_opacity = tex_frg_2_read.a;

// VectorOp:5
	vec3 n_out5p0 = mix(original_texture_color, bg_color, bg_opacity*2.0f);

// Output:0
	COLOR.rgb = n_out5p0;
	COLOR.a = original_texture_alpha;

}

void light() {
// Output:0

}
