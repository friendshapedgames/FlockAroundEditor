shader_type spatial;

render_mode blend_mix, depth_draw_opaque, cull_back;

// dont forget to copy these into the C# if they change
uniform float fade_start = 1;
uniform float fade_end = 2;
uniform float speed = 1.0;
uniform float scale = 1.0;
uniform float opacity = 0.25;

uniform vec3 base_color = vec3(1.0, 0.5, 1.0);

// Texture layers
uniform sampler2D tex1;
uniform sampler2D tex2;

// Scroll directions (units per second)
uniform vec2 scroll1 = vec2(0.2, 0.0);
uniform vec2 scroll2 = vec2(0.5, 0);

// Blend strength
uniform float tex1_strength = 0.5;
uniform float tex2_strength = 0.5;

varying vec3 world_pos;
varying vec3 camera_pos;
varying vec3 camera_forward;
varying vec2 uv;

void vertex() {
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    camera_pos = (INV_VIEW_MATRIX * vec4(0.0, 0.0, 0.0, 1.0)).xyz;
    camera_forward = -normalize((INV_VIEW_MATRIX * vec4(0.0, 0.0, -1.0, 0.0)).xyz);
    vec3 local_pos = world_pos - camera_pos;
    uv = local_pos.xz;
}

void fragment() {

    float dist = distance(camera_pos, world_pos);
    vec3 forward_pos = world_pos + camera_forward * dist;
    float dist2 = distance(camera_pos, forward_pos);


    float fade = (1.0 - smoothstep(fade_start, fade_end, dist));

    // Animate UVs
    vec2 uv1 = uv * (dist2 / max(dist, 0.001)) * 10.0 * scale - scroll1 * TIME * speed + vec2(0.5, 0.5);
    uv1 = uv1.yx;
    vec2 uv2 = uv * (dist2 / max(dist, 0.001)) * 15.0 * scale - scroll2 * TIME * speed;

    // Sample textures
    vec3 t1 = texture(tex1, uv1).rgb * tex1_strength;
    vec3 t2 = texture(tex2, uv2).rgb * tex2_strength;

    // Combine layers (additive style)
    vec3 combined = base_color + t1 + t2;

    ALBEDO = combined * opacity;
    ALPHA = fade * opacity;
}