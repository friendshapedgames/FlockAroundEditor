shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 albedo : source_color = vec4(1,1,1,1);
uniform sampler2D texture_albedo : source_color, filter_linear_mipmap, repeat_enable;

uniform vec2 scroll_speed = vec2(0.1, 0.0);
uniform vec2 uv_offset = vec2(0.0, 0.0);

void vertex() {
}

void fragment() {
    vec2 uv = UV + uv_offset + scroll_speed * TIME * 1.0/60.0;

	vec4 albedo_tex = texture(texture_albedo, uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	ALPHA *= albedo.a * albedo_tex.a;
}
