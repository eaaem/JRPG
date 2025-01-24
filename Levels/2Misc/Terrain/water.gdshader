shader_type spatial;

uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear_mipmap;
uniform vec4 output_color : source_color = vec4(0.0, 0.2, 1.0, 1.0);
uniform float amount : hint_range(0.2, 1.5) = 0.8;
uniform float beer_factor = 0.2;
uniform float specular_amount : hint_range(0.0, 1.0) = 0.6;

float generateOffset(float x, float z, float val1, float val2, float time) {
	float speed = 1.0;

	float radiansX = ((mod(x + z * x * val1, amount) / amount) + (time * speed) * mod(x * 0.8 + z, 1.5)) * 2.0 * 3.14;
	float radiansZ = ((mod(val2 * (z * x + x * z), amount) / amount) + (time * speed) * 2.0 * mod(x, 2.0)) * 2.0 * 3.14;

	return amount * 0.5 * (sin(radiansZ) + cos(radiansX));
}

vec3 applyDistortion(vec3 vertex, float time) {
	float xd = generateOffset(vertex.x, vertex.z, 0.2, 0.1, time);
	float yd = generateOffset(vertex.x, vertex.z, 0.1, 0.3, time);
	float zd = generateOffset(vertex.x, vertex.z, 0.15, 0.2, time);
	return vertex + vec3(xd, yd, zd);
}

void vertex() {
	VERTEX = applyDistortion(VERTEX, TIME * 0.1);
}

void fragment() {
NORMAL = normalize(cross(dFdx(VERTEX), dFdy(VERTEX)));
	METALLIC = 0.0;
	SPECULAR = 0.6;
	ROUGHNESS = 1.0;
	ALBEDO = output_color.xyz;

	float depth = texture(DEPTH_TEXTURE, SCREEN_UV).r;

	//depth = depth * 2.0 - 1.0;
	depth = PROJECTION_MATRIX[3][2] / (depth + PROJECTION_MATRIX[2][2]);
	depth = depth + VERTEX.z;

	depth = exp(-depth * beer_factor);
	ALPHA = clamp(1.0 - depth, 0.0, 1.0);
}
