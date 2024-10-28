shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

void fragment() {
	vec4 orig_color = texture(screen_texture, UV);
	float grey = (orig_color.r + orig_color.g + orig_color.b) / 3.0;
	float yellow = (orig_color.r + orig_color.g) / 2.0;
	COLOR = vec4(yellow, yellow, grey * 0.75, 1.0);
}
