shader_type spatial;
render_mode cull_back;

//uniform float player_displacement_strength : hint_range(0.0, 1.0) = 0.4;
//uniform float player_displacement_size : hint_range(0.0, 2.0) = 1.0;

uniform vec3 wind_direction;
uniform float wind_strength : hint_range(0.0, 1.0) = 0.3;
uniform sampler2D wind_noise;
uniform float wind_noise_size : hint_range(0.0, 1.0) = 0.05;
uniform float wind_noise_speed : hint_range(0.0, 1.0) = 0.1;

uniform sampler2D tree_texture : source_color;

//instance uniform vec3 player_position;

void vertex() {
	vec3 world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;

	vec3 wind_texture = texture(wind_noise, world_position.xz * wind_noise_size + normalize(-wind_direction.xz) * (TIME/ 2.5) * wind_noise_speed).rgb;
	vec3 wind_vector = (vec4(wind_texture * normalize(wind_direction) * wind_strength, 0.0) * MODEL_MATRIX).xyz;

	VERTEX += COLOR.rgb * (wind_vector * (1.0) * (0.7));
}

void fragment() {
	ALBEDO = texture(tree_texture, UV).rgb;
}